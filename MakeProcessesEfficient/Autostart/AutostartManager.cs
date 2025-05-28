using Microsoft.Win32.TaskScheduler;
using System.Text;
using System.Text.Json;

namespace MakeProcessesEfficient.Autostart
{
    internal static class AutostartManager
    {
        private const string _autostartTaskName = "MakeProcessesEfficient";
        private static string PathToAutostartOptions => JsonHelper.GetDynamicPath(@"Autostart/AutostartOptions.json");
        private static AutostartOptions DefaultAutostartOptions => new()
        {
            AutostartActive = true,
            LoginDelay = TimeSpan.FromMinutes(2),
            RepetitionDelay = TimeSpan.FromMinutes(10),
            StopAfterDelay = TimeSpan.Zero,
        };
        private static AutostartOptions CurrentAutostartOptions
            => JsonSerializer.Deserialize<AutostartOptions>(File.ReadAllText(PathToAutostartOptions));

        internal static void ShowAutostartOptions()
        {
            Console.WriteLine("\n1. Activate autostart(with default options)");
            Console.WriteLine("2. Configure a custom autostart behaviour");
            Console.WriteLine("3. Deactivate autostart");
            Console.WriteLine("4. Show default autostart options");
            Console.WriteLine("5. Show current autostart state/options");

            ConsoleKey consoleKey = Console.ReadKey(true).Key;
            HandleSelectedOption(consoleKey);
        }

        static void HandleSelectedOption(ConsoleKey consoleKey)
        {
            switch (consoleKey)
            {
                case ConsoleKey.D1:
                    ActivateAutostart();
                    break;
                case ConsoleKey.D2:
                    ConfigureAutostart();
                    break;
                case ConsoleKey.D3:
                    DeactivateAutostart();
                    break;
                case ConsoleKey.D4:
                    DisplayAutostartSettings(DefaultAutostartOptions); 
                    break;
                case ConsoleKey.D5:
                    DisplayAutostartSettings(CurrentAutostartOptions);
                    break;
                default:
                    Console.WriteLine("\nInvalid option\n");
                    break;
            }
        }

        static void DisplayAutostartSettings(AutostartOptions autostartOptions)
        {
            if (!autostartOptions.AutostartActive)
            {
                Console.WriteLine("\nAutostart disabled\n");
                return;
            }

            Console.WriteLine($"\nStarts {autostartOptions.LoginDelay.TotalMinutes}min after login");
            Console.WriteLine($"Repeats every {autostartOptions.RepetitionDelay.TotalMinutes}min after that");

            if (autostartOptions.StopAfterDelay == TimeSpan.Zero)
            {
                Console.WriteLine($"Will never stop repeating\n");
            }
            else
            {
                Console.WriteLine($"Stops repeating after the pc ran for {autostartOptions.StopAfterDelay.TotalMinutes}min\n");
            }
        }

        #region ChangeAuotstartBehaviour
        static void ActivateAutostart()
        {
            Console.WriteLine("\nActivated autostart(strongly recommended this app will close itself anyway after a few secs)\n");
            using (TaskService ts = new())
            {
                Microsoft.Win32.TaskScheduler.Task? task = ts.GetTask(_autostartTaskName);
                if (task != null)
                {
                    task.Enabled = true;
                    return;
                }

                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Performace optimizer. Will automatically close a few secs after startup";

                AutostartOptions autostartOptions = DefaultAutostartOptions;
                var trigger = new LogonTrigger
                {
                    Delay = autostartOptions.LoginDelay,
                    Repetition = new RepetitionPattern(autostartOptions.RepetitionDelay, autostartOptions.StopAfterDelay)
                };
                td.Triggers.Add(trigger);
            
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string workingDirectory = Path.GetDirectoryName(exePath) ?? AppDomain.CurrentDomain.BaseDirectory;

                exePath = exePath.Replace(".dll", ".exe");
                string batPath = Path.Combine(Path.GetDirectoryName(exePath) ?? AppDomain.CurrentDomain.BaseDirectory, "RunAutostart.bat");

                string batContent = $"@echo off\r\n\"{exePath}\" Autostart\r\n";

                File.WriteAllText(batPath, batContent, new UTF8Encoding(false));

                td.Actions.Add(new ExecAction(batPath, null, workingDirectory));
                ts.RootFolder.RegisterTaskDefinition(_autostartTaskName, td);

                JsonHelper.WriteToJson(PathToAutostartOptions, autostartOptions);
            }
        }

        static void ConfigureAutostart()
        {
            Console.Write("How many minutes should be the start of this be delayed after windows login(0- 720): ");
            string delayInput = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!int.TryParse(delayInput, out int startupDelay) || startupDelay < 0 || startupDelay > 720)
            {
                Console.WriteLine("\nInput invalid\n");
                return;
            }

            Console.Write("How many minutes should be paused till this process runs again(0- 720): ");
            string repetitionDelayInput = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!int.TryParse(repetitionDelayInput, out int repetitionDelay) || repetitionDelay < 0 || repetitionDelay > 720)
            {
                Console.WriteLine("\nInput invalid\n");
                return;
            }

            Console.Write("How many minutes should be paused till the pc needs to restart for this to run again(0(never stops)- 4320): ");
            string stopAfterDelayInput = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!int.TryParse(stopAfterDelayInput, out int stopAfterDelay) || repetitionDelay < 0 || repetitionDelay > 4320)
            {
                Console.WriteLine("\nInput invalid\n");
                return;
            }

            AutostartOptions autostartOptions = new()
            {
                AutostartActive = true,
                LoginDelay = TimeSpan.FromMinutes(startupDelay),
                RepetitionDelay = TimeSpan.FromMinutes(repetitionDelay),
                StopAfterDelay = TimeSpan.FromMinutes(stopAfterDelay),
            };

            UpdateAutostart(autostartOptions);
        }

        static void UpdateAutostart(AutostartOptions autostartOptions)
        {
            Console.WriteLine("\nUpdated autostart options\n");
            using (TaskService ts = new())
            {
                Microsoft.Win32.TaskScheduler.Task? task = ts.GetTask(_autostartTaskName);
                task.Enabled = true;

                var loginTrigger = new LogonTrigger
                {
                    Delay = autostartOptions.LoginDelay,
                    Repetition = new RepetitionPattern(autostartOptions.RepetitionDelay, autostartOptions.StopAfterDelay)
                };

                task.Definition.Triggers.Clear();
                task.Definition.Triggers.Add(loginTrigger);

                ts.RootFolder.RegisterTaskDefinition(_autostartTaskName, task.Definition);
                JsonHelper.WriteToJson(PathToAutostartOptions, autostartOptions);
            }
        }

        static void DeactivateAutostart()
        {
            Console.WriteLine("\nDeactivated autostart\n");
            using (TaskService ts = new())
            {
                Microsoft.Win32.TaskScheduler.Task? task = ts.GetTask(_autostartTaskName);
                task?.Enabled = false;
            }
        }

        internal static void InitAutostart()
        {
            using (TaskService ts = new())
            {
                Microsoft.Win32.TaskScheduler.Task? task = ts.GetTask(_autostartTaskName);
                if (task == null)
                {
                    ActivateAutostart();
                }
            }
        }

        #endregion
    }
}
