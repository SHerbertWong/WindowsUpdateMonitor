using System;
using System.Runtime.InteropServices;

/*
 * Pinvoke.Advapi32: an abstraction layer between our code and unmanaged Windows API functions
 *                   as provided by advapi32.dll - see official documentation for corresponding API
 *                   usage.
 */
namespace Pinvoke
{
	public class Advapi32
	{
		public static readonly uint SERVICE_NO_CHANGE = 0xffffffff;

		public static readonly uint DELETE = 0x00010000;
		public static readonly uint READ_CONTROL = 0x00020000;
		public static readonly uint WRITE_DAC = 0x00040000;
		public static readonly uint WRITE_OWNER = 0x00080000;

		public static readonly uint MAXIMUM_ALLOWED = 0x02000000;

		public static readonly uint SERVICE_CONTROL_CONTINUE = 0x00000003;
		public static readonly uint SERVICE_CONTROL_INTERROGATE = 0x00000004;
		public static readonly uint SERVICE_CONTROL_NETBINDADD = 0x00000007;
		public static readonly uint SERVICE_CONTROL_NETBINDDISABLE = 0x0000000A;
		public static readonly uint SERVICE_CONTROL_NETBINDENABLE = 0x00000009;
		public static readonly uint SERVICE_CONTROL_NETBINDREMOVE = 0x00000008;
		public static readonly uint SERVICE_CONTROL_PARAMCHANGE = 0x00000006;
		public static readonly uint SERVICE_CONTROL_PAUSE = 0x00000002;
		public static readonly uint SERVICE_CONTROL_STOP = 0x00000001;

		public static readonly uint SC_MANAGER_ALL_ACCESS = 0xF003F;
		public static readonly uint SC_MANAGER_CREATE_SERVICE = 0x0002;
		public static readonly uint SC_MANAGER_CONNECT = 0x0001;
		public static readonly uint SC_MANAGER_ENUMERATE_SERVICE = 0x0004;
		public static readonly uint SC_MANAGER_LOCK = 0x0008;
		public static readonly uint SC_MANAGER_MODIFY_BOOT_CONFIG = 0x0020;
		public static readonly uint SC_MANAGER_QUERY_LOCK_STATUS = 0x0010;

		public static readonly uint SERVICE_ALL_ACCESS = 0xF01FF;
		public static readonly uint SERVICE_CHANGE_CONFIG = 0x0002;
		public static readonly uint SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
		public static readonly uint SERVICE_INTERROGATE = 0x0080;
		public static readonly uint SERVICE_PAUSE_CONTINUE = 0x0040;
		public static readonly uint SERVICE_QUERY_CONFIG = 0x0001;
		public static readonly uint SERVICE_QUERY_STATUS = 0x0004;
		public static readonly uint SERVICE_START = 0x0010;
		public static readonly uint SERVICE_STOP = 0x0020;
		public static readonly uint SERVICE_USER_DEFINED_CONTROL = 0x0100;

		public static readonly uint GENERIC_READ = 0x80000000;
		public static readonly uint GENERIC_WRITE = 0x40000000;
		public static readonly uint GENERIC_EXECUTE = 0x20000000;
		public static readonly uint GENERIC_ALL = 0x10000000;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct QUERY_SERVICE_CONFIG
		{
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
			public uint dwServiceType;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
			public uint dwStartType;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
			public uint dwErrorControl;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
			public String lpBinaryPathName;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
			public String lpLoadOrderGroup;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
			public uint dwTagID;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
			public String lpDependencies;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
			public String lpServiceStartName;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
			public String lpDisplayName;
		};

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct SERVICE_STATUS_PROCESS
		{
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
			public uint dwServiceType;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
			public uint dwCurrentState;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
			public uint dwControlsAccepted;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
			public uint dwWin32ExitCode;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
			public uint dwServiceSpecificExitCode;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
			public uint dwCheckPoint;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
			public uint dwWaitHint;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
			public uint dwProcessID;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
			public uint dwServiceFlags;
		}

		[DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfigW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ChangeServiceConfig(
			IntPtr hService,
			uint dwServiceType,
			uint dwStartType,
			uint dwErrorControl,
			String lpBinaryPathName,
			String lpLoadOrderGroup,
			IntPtr lpdwTagId,
			[In] char[] lpDependencies,
			String lpServiceStartName,
			String lpPassword,
			String lpDisplayName
		);

		[DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseServiceHandle(
			IntPtr hSCObject
		);

		[DllImport("advapi32.dll", EntryPoint = "ControlService", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool ControlService(
			IntPtr hService,
			uint dwControl,
			IntPtr lpServiceStatus
		);

		[DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr OpenSCManager(
			String lpMachineName,
			String lpDatabaseName,
			uint dwDesiredAccess
		);

		[DllImport("advapi32.dll", EntryPoint = "OpenServiceW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr OpenService(
			IntPtr hSCManager,
			String lpServiceName,
			uint dwDesiredAccess
		);

		[DllImport("advapi32.dll", EntryPoint = "QueryServiceConfigW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool QueryServiceConfig(
			IntPtr hService,
			IntPtr lpBuffer,
			int cbBufSize,
			out int pcbBytesNeeded
		);

		[DllImport("advapi32.dll", EntryPoint = "QueryServiceStatusEx", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool QueryServiceStatusEx(
			IntPtr hService,
			int InfoLevel,
			IntPtr lpBuffer,
			int cbBufSize,
			out int pcbBytesNeeded
		);
	}
}
