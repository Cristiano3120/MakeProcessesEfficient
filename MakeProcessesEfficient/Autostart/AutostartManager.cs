using Microsoft.Win32.TaskScheduler;

namespace MakeProcessesEfficient.Autostart
{
    internal static class AutostartManager
    {
        private const string _autostartTaskName = "MakeProcessesEfficient";
        internal static void ShowAutostartOptions()
        {
            Console.WriteLine("\n1. Activate autostart(with default options)");
            Console.WriteLine("2. Configure a custom autostart behaviour");
            Console.WriteLine("3. Deactivate autostart");
            Console.WriteLine("4. Show default autostart options");
            Console.WriteLine("5. Show current autostart state/options");

            ConsoleKey consoleKey = Console.ReadKey().Key;
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
                    throw new NotImplementedException();
                case ConsoleKey.D5:
                    throw new NotImplementedException();
                default:
                    Console.WriteLine("\nInvalid option\n");
                    break;
            }
        }

        #region ChangeAuotstartBehaviour
        static void ActivateAutostart()
        {
            Console.WriteLine("\n Activated autostart\n");
            using (TaskService ts = new())
            {
                Microsoft.Win32.TaskScheduler.Task? task = ts.GetTask(_autostartTaskName);
                if (task != null)
                {
                    task.Enabled = true;
                    return;
                }

                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Starts 2 mins after login and repeats every 10 min after that";

                var trigger = new LogonTrigger
                {
                    Delay = TimeSpan.FromMinutes(2),
                    Repetition = new RepetitionPattern(TimeSpan.FromMinutes(10), TimeSpan.FromDays(1))
                };
                td.Triggers.Add(trigger);

                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                td.Actions.Add(new ExecAction(exePath, "Autostart", null));
                ts.RootFolder.RegisterTaskDefinition(_autostartTaskName, td);
            }
        }

        static void ConfigureAutostart()
        {

        }

        static void DeactivateAutostart()
        {
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
