using MakeProcessesEfficient.MemoryPriorityResources;
using MakeProcessesEfficient.ProcessPowerThrottlingStateResources;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace MakeProcessesEfficient
{
    internal static partial class ProcessOptimizer
    {
        #region Win32ApiImports
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetProcessInformation(IntPtr hProcess, PROCESS_INFORMATION_CLASS processInformationClass,
            IntPtr processInformation, int processInformationSize);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetProcessInformation(IntPtr hProcess, PROCESS_INFORMATION_CLASS processInformationClass,
            IntPtr processInformation, uint processInformationSize);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr OpenProcess(ProcessAccessRights dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint processId);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool CloseHandle(IntPtr hObject);

        #endregion

        #region Get/Set PerformanceSettings(Internal)
        internal static void SetPerformanceSettings()
        {
            Console.WriteLine("\nOptimizing processes...");

            JsonElement processesJson = ProcessesJsonHandler.GetProcessesJson();
            JsonProperty[] jsonProperties = [.. processesJson.EnumerateObject()];

            if (jsonProperties.Length == 0)
            {
                Console.WriteLine("\nNo processes found in JSON file. Nothing to optimize\n");
                return;
            }

            foreach (JsonProperty jsonProperty in jsonProperties)
            {
                string processName = jsonProperty.Name;
                MemoryPriority memoryPriority = (MemoryPriority)jsonProperty.Value.GetProperty("MemoryPriority").GetInt16();
                bool efficiencyMode = jsonProperty.Value.GetProperty("EfficiencyMode").GetBoolean();

                Console.WriteLine();

                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0)
                {
                    Console.WriteLine($"\n{processName} is currently not running. Can´t optimize it!\n");
                    continue;
                }

                foreach (Process process in processes)
                {
                    SetProcessMemoryPriority(process, memoryPriority);
                    SetProcessPowerThrottling(process, efficiencyMode);
                }
            }

            Console.WriteLine("\nDone optimizing\n");
        }

        internal static void GetPerformanceSettings()
        {
            Console.Write("\nEnter the name of the process to retrieve infos of: ");
            string processName = Console.ReadLine()?.Trim() ?? "";

            if (processName == "")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\n[ERROR]: Invalid input\n");
                Console.ResetColor();

                return;
            }

            Console.WriteLine();

            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                Console.WriteLine($"{processName} is currently not running. Can´t optimize it!\n");
                return;
            }

            foreach (Process process in processes)
            {
                GetProcessMemoryPriority(process);
                GetProcessPowerThrottling(process);
            }

            Console.WriteLine();
        }

        #endregion

        #region HelperMethods

        #region ProcessMemoryPriority
        static void SetProcessMemoryPriority(Process process, MemoryPriority memoryPriority)
        {
            IntPtr handle = OpenProcess(ProcessAccessRights.PROCESS_SET_INFORMATION, false, (uint)process.Id);
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<MEMORY_PRIORITY_INFORMATION>());

            try
            {
                MEMORY_PRIORITY_INFORMATION memoryPriorityInfo = new(memoryPriority);
                Marshal.StructureToPtr(memoryPriorityInfo, ptr, false);

                bool result = SetProcessInformation(handle, PROCESS_INFORMATION_CLASS.ProcessMemoryPriority,
                    ptr, Marshal.SizeOf(memoryPriorityInfo));

                if (result)
                {
                    Console.WriteLine($"{process.ProcessName}({process.Id}) set memory priority to {memoryPriority}");
                }
                else
                {
                    Console.WriteLine($"Failed to set {process.ProcessName}({process.Id}) memory priority to {memoryPriority}");
                    Console.WriteLine($"[Error]: {Marshal.GetLastWin32Error()}");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
                CloseHandle(handle);
            }
        }

        static void GetProcessMemoryPriority(Process process)
        {
            IntPtr handle = OpenProcess(ProcessAccessRights.PROCESS_QUERY_LIMITED_INFORMATION, false, (uint)process.Id);

            int size = Marshal.SizeOf<MEMORY_PRIORITY_INFORMATION>();
            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                bool result = GetProcessInformation(handle, PROCESS_INFORMATION_CLASS.ProcessMemoryPriority, ptr, (uint)size);

                if (result)
                {
                    MEMORY_PRIORITY_INFORMATION memoryPriorityInfo = Marshal.PtrToStructure<MEMORY_PRIORITY_INFORMATION>(ptr);
                    Console.WriteLine($"{process.ProcessName}({process.Id}) memory priority is set to {memoryPriorityInfo.MemoryPriority}");
                }
                else
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    Console.WriteLine($"Failed to read {process.ProcessName}({process.Id}) memory priority");
                    Console.WriteLine($"Fehler: {errorCode}");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
                CloseHandle(handle);
            }
        }

        #endregion

        #region ProcessPowerThrottling

        static void SetProcessPowerThrottling(Process process, bool efficiencyMode)
        {
            IntPtr handle = OpenProcess(ProcessAccessRights.PROCESS_SET_INFORMATION, false, (uint)process.Id);

            PROCESS_POWER_THROTTLING_STATE powerThrottlingState = new(efficiencyMode);
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(powerThrottlingState));

            try
            {
                Marshal.StructureToPtr(powerThrottlingState, ptr, false);

                bool result = SetProcessInformation(handle, PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
                    ptr, Marshal.SizeOf(powerThrottlingState));

                if (result)
                {
                    Console.WriteLine($"{process.ProcessName}({process.Id}) set efficiency mode to {efficiencyMode}");
                }
                else
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    Console.WriteLine($"Failed to set {process.ProcessName}({process.Id}) efficiency mode to {efficiencyMode}");
                    Console.WriteLine($"[Error]: {errorCode}");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
                CloseHandle(handle);
            }
        }

        static void GetProcessPowerThrottling(Process process)
        {
            IntPtr handle = OpenProcess(ProcessAccessRights.PROCESS_QUERY_LIMITED_INFORMATION, false, (uint)process.Id);

            int size = Marshal.SizeOf<PROCESS_POWER_THROTTLING_STATE>();
            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                PROCESS_POWER_THROTTLING_STATE initState = new(false);
                Marshal.StructureToPtr(initState, ptr, false);

                bool result = GetProcessInformation(handle,
                    PROCESS_INFORMATION_CLASS.ProcessPowerThrottling, ptr, (uint)size);

                if (result)
                {
                    PROCESS_POWER_THROTTLING_STATE state = Marshal.PtrToStructure<PROCESS_POWER_THROTTLING_STATE>(ptr);
                    Console.WriteLine($"{process.ProcessName}({process.Id}) efficiency mode is set to {(state.StateMask & 0x1) != 0}");
                }
                else
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    Console.WriteLine($"Failed to read {process.ProcessName}({process.Id}) efficiency mode");
                    Console.WriteLine($"Fehler: {errorCode}");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
                CloseHandle(handle);
            }
        }

        #endregion

        #endregion
    }
}
