using MakeProcessesEfficient.MemoryPriorityResources;

namespace MakeProcessesEfficient
{
    internal readonly struct ProcessTweak
    {
        public string ProcessName { get; init; }
        public MemoryPriority MemoryPriority { get; init; }
        public bool PowerThrottling { get; init; }

        public ProcessTweak(string processName, MemoryPriority memoryPriority, bool powerThrottling)
        {
            ProcessName = processName;
            MemoryPriority = memoryPriority;
            PowerThrottling = powerThrottling;
        }
    }
}
