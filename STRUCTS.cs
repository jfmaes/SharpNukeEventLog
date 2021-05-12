using System;
using System.Runtime.InteropServices;


namespace SharpEventMuter
{
    public class STRUCTS
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct CLIENT_ID
        {
            public IntPtr UniqueProcess;
            public IntPtr UniqueThread;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct THREAD_BASIC_INFORMATION
        {
            public uint ExitStatus;
            public IntPtr TebBaseAdress;
            public CLIENT_ID ClientId;
            public IntPtr AffinityMask;
            public uint Priority;
            public uint BasePriority;
        }

        public enum ThreadInfoClass : uint
        {
            ThreadBasicInformation = 0,
            ThreadQuerySetWin32StartAddress = 0x9
        }

        public enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            Inherit = 0x80000000,
            All = 0x0000001F,
            NoHeaps = 0x40000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct THREADENTRY32
        {

            public UInt32 dwSize;
            public UInt32 cntUsage;
            public UInt32 th32ThreadID;
            public UInt32 th32OwnerProcessID;
            public UInt32 tpBasePri;
            public UInt32 tpDeltaPri;
            public UInt32 dwFlags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MODULEINFO
        {
            public IntPtr lpBaseOfDll;
            public uint sizeOfImage;
            public IntPtr entryPoint;
        }

    }
}
