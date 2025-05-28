using System.Runtime.InteropServices;

namespace MakeProcessesEfficient.ProcessPowerThrottlingStateResources
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PROCESS_POWER_THROTTLING_STATE
    {
        public uint Version;     
        public uint ControlMask; 
        public uint StateMask;  

        public PROCESS_POWER_THROTTLING_STATE(bool activate)
        {
            Version = 1;
            ControlMask = 0x1;
            StateMask = activate 
                ? 1u 
                : 0u;
        }
    }
}
