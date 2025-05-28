namespace MakeProcessesEfficient.Autostart
{
    internal readonly struct AutostartOptions
    {
        public bool AutostartActive { get; init; }
        public TimeSpan LoginDelay { get; init; }
        public TimeSpan RepetitionDelay { get; init; }
        public TimeSpan StopAfterDelay { get; init; }
    }
}
