﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DoodleDigits.Core;
using DoodleDigits.Core.Execution.Results;
using DoodleDigits.Core.Execution.ValueTypes;

namespace DoodleDigits {

    public class ResultPresenter : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly Dictionary<int, List<TempResult>> resultsPerLine = new();


        private record TempResult(string Content, Result Result);

        public List<ResultViewModel> Results { get; set; } = new();

        public void ParseResults(TextMeasure measure, CalculationResult calculationResult) {
            resultsPerLine.Clear();
            foreach (Result result in calculationResult.Results) {
                string? text = GetResultString(result);
                if (text == null) {
                    continue;
                }

                int line = measure.GetLineForIndex(result.Position.End.Value);

                if (resultsPerLine.TryGetValue(line, out var list) == false) {
                    resultsPerLine[line] = list = new List<TempResult>();
                }

                list.Add(new TempResult(text, result));
            }

            List<ResultViewModel> resultViewModels = new();

            measure.ApplyNewTextBoxDimensions();

            foreach (int line in resultsPerLine.Keys) {
                var results = resultsPerLine[line];
                results.Sort(CompareResults);
                string content = string.Join(", ", results.Select(x => x.Content));

                Point position = measure.GetFinalRectOfLine(line).BottomRight + new Vector(11, -23);
                resultViewModels.Add(new ResultViewModel(content, position));
            }

            Results = resultViewModels;
            OnPropertyChanged(nameof(Results));
        }

        private int CompareResults(TempResult resultA, TempResult resultB) {
            int GetValue(Result result) {
                int value = result.Position.End.Value;
                if (result is ResultValue) {
                    value -= 1000;
                }

                return value;
            }

            int a = GetValue(resultA.Result);
            int b = GetValue(resultB.Result);

            return a - b;
        }

        private string? GetResultString(Result result) {
            switch (result) {
                case ResultValue resultValue:
                    return GetValueString(resultValue.Value, true);
                case ResultError resultError:
                    return resultError.Error;
                case ResultConversion resultConversion:
                    return $"converted {resultConversion.PreviousValue} to {resultConversion.NewValue}";
            }

            return null;
        }

        private string? GetValueString(Value value, bool includeEqualSign) {
            string equalSign = " = ";
            string leadsToSign = " → ";
            if (includeEqualSign == false) {
                equalSign = "";
                leadsToSign = "";
            }

            if (value is TooBigValue tooBig) {
                return tooBig.ValueSign switch {
                    TooBigValue.Sign.Positive => leadsToSign + "A huge number",
                    TooBigValue.Sign.PositiveInfinity => equalSign + "∞",
                    TooBigValue.Sign.Negative => leadsToSign + "A huge negative number",
                    TooBigValue.Sign.NegativeInfinity => equalSign + "-∞",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (value.TriviallyAchieved) {
                return null;
            }

            if (value is UndefinedValue undefinedValue) {
                return undefinedValue.Type == UndefinedValue.UndefinedType.Undefined ? equalSign + "undefined" : null;
            }

            if (value is BooleanValue booleanValue) {
                return leadsToSign + booleanValue.ToString();
            }
            if (value is MatrixValue matrixValue) {
                return equalSign + GetMatrixString(matrixValue);
            }
            if (value is RealValue realValue) {
                string formPrefix = "";
                if (realValue.Form == RealValue.PresentedForm.Hex) {
                    formPrefix = "0x";
                } else if (realValue.Form == RealValue.PresentedForm.Binary) {
                    formPrefix = "0b";
                }
                return equalSign + formPrefix + realValue.ToString(25, 30, "ᴇ");
            }

            return null;
        }

        private string GetMatrixString(MatrixValue matrix) {
            string openSymbol, closeSymbol;
            if (matrix.DimensionCount == 1) {
                openSymbol = "(";
                closeSymbol = ")";
            } else {
                openSymbol = "[";
                closeSymbol = "]";
            }

            StringBuilder sb = new();

            void RecurseBuild(MatrixValue.MatrixDimension dimension) {
                sb.Append(openSymbol);

                bool first = true;
                foreach (MatrixValue.IMatrixElement element in dimension) {
                    if (first == false) {
                        sb.Append(", ");
                    }
                    first = false;
                    if (element is MatrixValue.MatrixDimension md) {
                        RecurseBuild(md);
                    } else if (element is MatrixValue.MatrixValueElement mve) {
                        sb.Append(GetValueString(mve.Value, false) ?? "unknown");
                    }
                }

                sb.Append(closeSymbol);
            }
            RecurseBuild(matrix.Dimension);

            return sb.ToString();
        }
    }
}
