﻿using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DoodleDigits {
    class SerializedState {
        public SerializedState(string content, int cursorIndex, SerializedPoint windowDimensions, float zoom) {
            Content = content;
            CursorIndex = cursorIndex;
            WindowDimensions = windowDimensions;
            Zoom = zoom;
        }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("cursor_index")]
        public int CursorIndex { get; set; }

        [JsonPropertyName("window_dimensions")]
        public SerializedPoint WindowDimensions { get; set; }

        [JsonPropertyName("zoom_level")]
        public float Zoom { get; set; }

        [JsonIgnore]
        private static string DirectoryPath => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Doodle Digits");

        [JsonIgnore]
        public static string SavePath => Path.Join(DirectoryPath, "state.json");

        public static async Task<SerializedState?> Load() {
            if (File.Exists(SavePath) == false) {
                return null;
            }

            string stateContent = await File.ReadAllTextAsync(SavePath);
            return JsonSerializer.Deserialize<SerializedState>(stateContent);

        }

        public async Task Save() {
            if (!Directory.Exists(DirectoryPath)) {
                Directory.CreateDirectory(DirectoryPath);
            }

            await File.WriteAllTextAsync(SavePath, JsonSerializer.Serialize(this));
        }
    }

    class SerializedPoint {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
