using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RemoteHttpClient.Http.Performance
    {
    internal static class NativeMethods
        {
        [DllImport("Kernel32.dll")]
        internal static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        internal static extern bool QueryPerformanceFrequency(out long lpFrequency);
        }
    }