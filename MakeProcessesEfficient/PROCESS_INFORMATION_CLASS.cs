namespace MakeProcessesEfficient
{
    internal enum PROCESS_INFORMATION_CLASS : uint
    {
        ProcessMemoryPriority,
        ProcessMemoryExhaustionInfo,
        ProcessAppMemoryInfo,
        ProcessInPrivateInfo,
        ProcessPowerThrottling,
        ProcessReservedValue1,
        ProcessTelemetryCoverageInfo,
        ProcessProtectionLevelInfo,
        ProcessLeapSecondInfo,
        ProcessMachineTypeInfo,
        ProcessOverrideSubsequentPrefetchParameter,
        ProcessMaxOverridePrefetchParameter,
        ProcessInformationClassMax
    }
}