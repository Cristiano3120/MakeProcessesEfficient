using System.Runtime.InteropServices;

namespace MakeProcessesEfficient.MemoryPriorityResources
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MEMORY_PRIORITY_INFORMATION(MemoryPriority memoryPriority)
    {
        public MemoryPriority MemoryPriority = memoryPriority;
    }
}
