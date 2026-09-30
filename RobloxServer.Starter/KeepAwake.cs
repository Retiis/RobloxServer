using System;
using System.Runtime.InteropServices;

namespace RobloxServer.Starter
{
    /// <summary>
    /// Stops Windows from sleeping while the site runs (SetThreadExecutionState). The screen can still
    /// turn off; the computer and the network stay on. Windows clears it when this program exits.
    /// </summary>
    public static class KeepAwake
    {
        [Flags]
        enum ExecutionState : uint
        {
            SystemRequired = 0x00000001,
            AwayModeRequired = 0x00000040,
            Continuous = 0x80000000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern ExecutionState SetThreadExecutionState(ExecutionState state);

        public static bool IsSupported
        {
            get { return Environment.OSVersion.Platform == PlatformID.Win32NT; }
        }

        /// <summary>Must be called from the thread that lives as long as the program (the UI thread).</summary>
        public static bool Set(bool awake)
        {
            if (!IsSupported)
            {
                return false;
            }

            try
            {
                ExecutionState state = awake
                    ? ExecutionState.Continuous | ExecutionState.SystemRequired | ExecutionState.AwayModeRequired
                    : ExecutionState.Continuous;
                if (SetThreadExecutionState(state) != 0)
                {
                    return true;
                }
                // Away mode is not available on every edition; keeping the system on is what matters.
                return awake && SetThreadExecutionState(ExecutionState.Continuous | ExecutionState.SystemRequired) != 0;
            }
            catch (EntryPointNotFoundException)
            {
                return false;
            }
        }
    }
}
