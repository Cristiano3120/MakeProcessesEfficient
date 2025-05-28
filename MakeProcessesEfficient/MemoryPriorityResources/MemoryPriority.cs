namespace MakeProcessesEfficient.MemoryPriorityResources
{
    public enum MemoryPriority : uint
    {
        MEMORY_PRIORITY_VERY_LOW = 1,
        MEMORY_PRIORITY_LOW = 2,
        MEMORY_PRIORITY_MEDIUM = 3, 
        MEMORY_PRIORITY_BELOW_NORMAL = 4,
        /// <summary>
        /// Normal memory priority. This is the default priority for all threads and processes on the system.
        /// </summary>
        MEMORY_PRIORITY_NORMAL = 5,
    }
}