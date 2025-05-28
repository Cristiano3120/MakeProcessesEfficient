using System.Text.Json;

namespace MakeProcessesEfficient
{
    internal static class JsonHelper
    {
        private static JsonSerializerOptions JsonSerializerOptions => new()
        {
            WriteIndented = true,
        };

        internal static string GetDynamicPath(string relativePath)
        {
            string projectBasePath = AppDomain.CurrentDomain.BaseDirectory;

            int binIndex = projectBasePath.IndexOf(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal);

            if (binIndex != -1)
            {
                projectBasePath = projectBasePath[..binIndex];
            }

            return Path.Combine(projectBasePath, relativePath);
        }

        internal static bool CheckIfFileExists(string filepath)
        {
            if (!File.Exists(filepath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[ERROR]: JSON file does not exist. Creating a new one...\n");
                Console.ResetColor();

                File.WriteAllText(filepath, "{}");
                return false;
            }

            return true;
        }

        internal static void WriteToJson<T>(string path, T content)
        {
            CheckIfFileExists(path);

            string jsonStr = JsonSerializer.Serialize(content, JsonSerializerOptions);
            File.WriteAllText(path, jsonStr);
        }

        internal static JsonElement ReadJson(string path)
        {
            CheckIfFileExists(path);
            return JsonDocument.Parse(File.ReadAllText(path)).RootElement;
        }
    }
}
