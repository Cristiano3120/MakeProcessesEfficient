using MakeProcessesEfficient.MemoryPriorityResources;
using System.Text.Json;

namespace MakeProcessesEfficient
{
    internal static class ProcessesJsonHandler
    {
        private static string ProcessesJsonPath => GetDynamicPath("Processes.json");
        private static JsonSerializerOptions JsonSerializerOptions => new()
        {
            WriteIndented = true,
        };

        internal static void AddProcessToJsonFile()
        {
            ProcessTweak? processTweak = GetSettings();
            if (!processTweak.HasValue)
                return;

            WriteToJsonFile(processTweak.Value);
        }

        internal static void RemoveProcessFromJsonFile()
        {
            JsonProperty[] jsonProperties = OutputJsonFile();
            string processName = SelectProcess(jsonProperties);

            if (processName == string.Empty)
                return;

            JsonElement jsonElement = ReadJson();
            Dictionary<string, JsonElement> jsonObjectDict = jsonElement.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value);

            jsonObjectDict.Remove(processName);
            string jsonStr = JsonSerializer.Serialize(jsonObjectDict, JsonSerializerOptions);

            File.WriteAllText(ProcessesJsonPath, jsonStr);
            Console.WriteLine($"Sucessfully removed {processName}\n");
        }

        internal static void UpdateJsonProperty()
        {
            JsonProperty[] jsonProperties = OutputJsonFile();
            string processName = SelectProcess(jsonProperties);

            if (processName == string.Empty)
                return;

            JsonElement jsonElement = ReadJson();

            Dictionary<string, JsonElement> jsonObjectDict = jsonElement.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value);

            Console.Write("\nEnter new memory priority (0-5): ");
            string memoryInput = Console.ReadLine()?.Trim() ?? "";
            if (!int.TryParse(memoryInput, out int memoryPriority) || memoryPriority < 0 || memoryPriority > 5)
            {
                Console.WriteLine("Invalid memory priority. Please enter a number between 0 and 5\n");
                return;
            }

            Console.Write("Do you want to enable efficiency mode? (y/n): ");
            ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
            if (consoleKeyInfo.Key != ConsoleKey.Y && consoleKeyInfo.Key != ConsoleKey.N)
            {
                Console.WriteLine("\nInvalid input. Please enter y or n\n");
                return;
            }

            jsonObjectDict[processName] = JsonSerializer.SerializeToElement(new
            {
                MemoryPriority = memoryPriority,
                EfficiencyMode = consoleKeyInfo.Key == ConsoleKey.Y
            });

            string updatedJson = JsonSerializer.Serialize(jsonObjectDict, JsonSerializerOptions);
            File.WriteAllText(ProcessesJsonPath, updatedJson);

            Console.WriteLine($"\nUpdated {processName} to MemoryPriority: {memoryPriority} and EfficiencyMode: {consoleKeyInfo.Key == ConsoleKey.Y}\n");
        }

        internal static JsonProperty[] OutputJsonFile()
        {
            JsonElement jsonElement = ReadJson();

            JsonProperty[] jsonObject = [.. jsonElement.EnumerateObject()];
            if (jsonObject.Length == 0)
            {
                Console.WriteLine("\nNo processes found in JSON file\n");
                return jsonObject;
            }

            Console.WriteLine("\nCurrent processes in JSON file:");
            for (int i = 0; i < jsonObject.Length; i++)
            {
                short memoryPriority = jsonObject[i].Value.GetProperty("MemoryPriority").GetInt16();
                bool efficiencyMode = jsonObject[i].Value.GetProperty("EfficiencyMode").GetBoolean();

                Console.WriteLine($"{i + 1}. {jsonObject[i].Name}" +
                    $" [Memory priority: {(MemoryPriority)memoryPriority}({memoryPriority})]" +
                    $" [EfficiencyMode: {efficiencyMode}]");
            }

            Console.WriteLine();
            return jsonObject;
        }

        internal static JsonElement ReadJson()
        {
            CheckIfFileExists();  
            return JsonDocument.Parse(File.ReadAllText(ProcessesJsonPath)).RootElement;
        }

        #region DirectJsonHelperMethods

        static ProcessTweak? GetSettings()
        {
            Console.WriteLine();
            Console.Write("Enter a process name: ");

            string processName = Console.ReadLine()?.Trim() ?? string.Empty;
            MemoryPriority memoryPriority;
            bool efficiencyMode;

            if (processName == string.Empty)
            {
                Console.WriteLine("Invalid\n");
                return null;
            }

            Console.Write("Memory priority(0-5): ");
            if (!int.TryParse(Console.ReadLine(), out int memoryPriorityNum) || memoryPriorityNum < 0 || memoryPriorityNum > 5)
            {
                Console.WriteLine("Invalid memory priority. Please enter a number between 0 and 5\n");
                return null;
            }

            memoryPriority = (MemoryPriority)memoryPriorityNum;

            Console.Write("Do u wanna put the process in efficiency mode(y/n): ");
            ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
            if (consoleKeyInfo.Key != ConsoleKey.Y && consoleKeyInfo.Key != ConsoleKey.N)
            {
                Console.WriteLine("\n\nInvalid input. Please enter y or n\n");
                return null;
            }

            Console.WriteLine("\n");
            efficiencyMode = consoleKeyInfo.Key == ConsoleKey.Y;

            return new ProcessTweak(processName, memoryPriority, efficiencyMode);
        }

        static void WriteToJsonFile(ProcessTweak processTweak)
        {
            var processSettings = new
            {
                MemoryPriority = (int)processTweak.MemoryPriority,
                EfficiencyMode = processTweak.PowerThrottling
            };

            JsonElement jsonElement = ReadJson();
            Dictionary<string, JsonElement> jsonObject = jsonElement.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value);

            if (jsonObject.ContainsKey(processTweak.ProcessName))
            {
                Console.WriteLine($"Process {processTweak.ProcessName} already exists in JSON file. Updating settings...");
            }

            jsonObject[processTweak.ProcessName] = JsonSerializer.SerializeToElement(processSettings);
            string updatedJson = JsonSerializer.Serialize(jsonObject, JsonSerializerOptions);

            File.WriteAllText(ProcessesJsonPath, updatedJson);

            Console.WriteLine($"Process {processTweak.ProcessName} added to JSON file with MemoryPriority: {processTweak.MemoryPriority} and EfficiencyMode: {processTweak.PowerThrottling}\n");
        }

        static string SelectProcess(JsonProperty[] jsonProperties)
        {
            Console.Write("\nEnter the process number of the process: ");
            string? input = Console.ReadLine()?.Trim();
            if (int.TryParse(input, out int processNumber) && processNumber > 0 && processNumber <= jsonProperties.Length)
            {
                return jsonProperties[processNumber - 1].Name;
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid process number\n");
                return string.Empty;
            }
        }
        #endregion

        #region IndirectJsonHelperMethods

        static string GetDynamicPath(string relativePath)
        {
            string projectBasePath = AppDomain.CurrentDomain.BaseDirectory;

            int binIndex = projectBasePath.IndexOf(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal);

            if (binIndex != -1)
            {
                projectBasePath = projectBasePath[..binIndex];
            }

            return Path.Combine(projectBasePath, relativePath);
        }

        static bool CheckIfFileExists()
        {
            if (!File.Exists(ProcessesJsonPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[ERROR]: JSON file does not exist. Creating a new one...\n");
                Console.ResetColor();

                File.WriteAllText(ProcessesJsonPath, "{}");
                return false;
            }

            return true;
        }

        #endregion
    }
}
