using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DInvoke.Data;

namespace SharpEventMuter
{
    class DELEGATES
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate Native.NTSTATUS NtQueryInformationThread(IntPtr threadHandle, STRUCTS.ThreadInfoClass threadInformationClass, out IntPtr startAddr, ulong threadInformationLength, IntPtr returnLengthPtr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr CreateToolhelp32Snapshot(STRUCTS.SnapshotFlags dwFlags, uint th32ProcessID);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool EnumProcessModules(IntPtr hProcess, [Out] IntPtr lphModule, uint cb, out uint lpcbNeeded);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, int nSize);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool Thread32First(IntPtr hSnapshot, ref STRUCTS.THREADENTRY32 lpte);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool Thread32Next(IntPtr hSnapshot, ref STRUCTS.THREADENTRY32 lpte);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, IntPtr lpModInfo, uint cb);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr OpenThread(DInvoke.Data.Win32.Kernel32.ThreadAccess access, bool bInheritHandle, uint dwThreadId);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint SuspendThread(IntPtr handle);
    }
}
