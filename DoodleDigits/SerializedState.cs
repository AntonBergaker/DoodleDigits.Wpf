﻿using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace DoodleDigits {
    class SerializedState {
        public SerializedState(string content, int cursorIndex, SerializedPoint windowDimensions) {
            Content = content;
            CursorIndex = cursorIndex;
            WindowDimensions = windowDimensions;
        }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("cursor_index")]
        public int CursorIndex { get; set; }

        [JsonPropertyName("window_dimensions")]
        public SerializedPoint WindowDimensions { get; set; }

        [JsonIgnore]
        private static string DirectoryPath => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Doodle Digits");

        [JsonIgnore]
        public static string SavePath => Path.Join(DirectoryPath, "state.json");

        public static SerializedState? Load() {
            if (File.Exists(SavePath) == false) {
                return null;
            }

            string stateContent = File.ReadAllText(SavePath);
            return JsonSerializer.Deserialize<SerializedState>(stateContent);

        }

        public async Task Save(CancellationToken cancellationToken) {
            if (!Directory.Exists(DirectoryPath)) {
                Directory.CreateDirectory(DirectoryPath);
            }

            await File.WriteAllTextAsync(SavePath, JsonSerializer.Serialize(this), cancellationToken);
        }
    }

    class SerializedPoint {
        public double X { get; set; }
        public double Y { get; set; }

        public SerializedPoint(double x, double y) {
            X = x;
            Y = y;
        }
    }
}
