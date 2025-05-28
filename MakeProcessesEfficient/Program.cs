using MakeProcessesEfficient.Autostart;

namespace MakeProcessesEfficient
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Make Processes Efficient";

            if (args.Length == 1)
            {
                ProcessOptimizer.SetPerformanceSettings();
            }
            else
            {
                AutostartManager.InitAutostart();
                PrintOptions();
            }
        }

        static void PrintOptions()
        {
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Run performance optimizer");
            Console.WriteLine("2. Add process to optimize");
            Console.WriteLine("3. Remove process from optimization");
            Console.WriteLine("4. Update settings for a specific process");
            Console.WriteLine("5. Output all processes that are being optimized");
            Console.WriteLine("6. Get performance infos of an running process");
            Console.WriteLine("7. Change autostart behaviour");
            Console.WriteLine("8. Clear console");
            Console.WriteLine("9. Exit application");

            ConsoleKey consoleKey = Console.ReadKey(true).Key;
            HandleSelectedOption(consoleKey);
        }

        static void HandleSelectedOption(ConsoleKey consoleKey)
        {
            switch (consoleKey)
            {
                case ConsoleKey.D1:
                    ProcessOptimizer.SetPerformanceSettings();
                    break;
                case ConsoleKey.D2:
                    ProcessesJsonHandler.AddProcessToJsonFile();
                    break;
                case ConsoleKey.D3:
                    ProcessesJsonHandler.RemoveProcessFromJsonFile();
                    break;
                case ConsoleKey.D4:
                    ProcessesJsonHandler.UpdateJsonProperty();
                    break;
                case ConsoleKey.D5:
                    ProcessesJsonHandler.OutputJsonFile();
                    break;
                case ConsoleKey.D6:
                    ProcessOptimizer.GetPerformanceSettings();
                    break;
                case ConsoleKey.D7:
                    AutostartManager.ShowAutostartOptions();
                    break;
                case ConsoleKey.D8:
                    Console.Clear();
                    break;
                case ConsoleKey.D9:
                    Console.WriteLine("\nExiting application...");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine();
                    break;
            }

            PrintOptions();
        }
    }
}
