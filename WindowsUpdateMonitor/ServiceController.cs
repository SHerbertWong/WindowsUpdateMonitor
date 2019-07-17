using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.ServiceProcess;
using Microsoft.Win32.SafeHandles;
using Pinvoke;

/*
 * SystemEx.ServiceProcess.ServiceController: An improved version of ServiceController taking advantage of 
 *                                            the Windows API directly to provide more features as well as
 *                                            more reliable service information
 */
namespace SystemEx.ServiceProcess
{
	[SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	internal class SafeSCManagerHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		public SafeSCManagerHandle() : this(null, null, Advapi32.MAXIMUM_ALLOWED) {}

		public SafeSCManagerHandle(String machineName, String databaseName) : this (machineName, databaseName, Advapi32.MAXIMUM_ALLOWED) {}

		public SafeSCManagerHandle(String machineName, String databaseName, uint desiredAccess) : base(true)
		{
			if ((handle = Advapi32.OpenSCManager(machineName, databaseName, desiredAccess)).Equals(IntPtr.Zero))
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		protected override bool ReleaseHandle()
		{
			return Advapi32.CloseServiceHandle(handle);
		}
	}

	[SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	internal class SafeServiceHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		public SafeServiceHandle(SafeHandle safeSCHandle, string serviceName) : this(safeSCHandle, serviceName, Advapi32.MAXIMUM_ALLOWED) {}

		public SafeServiceHandle(SafeHandle safeSCHandle, string serviceName, uint desiredAccess) : base(true)
		{
			if ((handle = Advapi32.OpenService(safeSCHandle.DangerousGetHandle(), serviceName, desiredAccess)).Equals(IntPtr.Zero))
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		protected override bool ReleaseHandle()
		{
			return Advapi32.CloseServiceHandle(handle);
		}
	}

	public class ServiceController : System.ServiceProcess.ServiceController
	{
		private readonly SafeHandle scManagerHandle, serviceHandle;

		public ServiceController(string serviceName) : base(serviceName)
		{
			scManagerHandle = new SafeSCManagerHandle();
			serviceHandle = new SafeServiceHandle(scManagerHandle, serviceName);
		}

		// Alias: ServiceName
		public string Name
		{
			get
			{
				return ServiceName;
			}
			set
			{
				ServiceName = value;
			}
		}

		// Alias: ServicesDependedOn
		public ServiceController[] RequiredServices
		{
			get
			{
				return ServicesDependedOn;
			}
		}

		// New: the process ID of the service
		public uint ProcessId
		{
			get
			{
				return GetServiceStatusProcessStruct(serviceHandle).dwProcessID;
			}
		}

		// Overridden: ServicesDependedOn now returns objects of this class instead of those of the parent class
		public new ServiceController[] ServicesDependedOn
		{
			get
			{
				System.ServiceProcess.ServiceController[] baseServices = base.ServicesDependedOn;
				ServiceController[] services = new ServiceController[baseServices.Length];

				for (int i = 0; i < baseServices.Length; i++)
				{
					services[i] = new ServiceController(baseServices[i].ServiceName);
				}

				return services;
			}
		}

		// Overridden: ServiceHandle now has the maxium allowed access clearance to the service it points to
		public new SafeHandle ServiceHandle
		{
			get
			{
				return serviceHandle;
			}
		}

		// Overridden: StartType now reports service start mode reliably and is writable
		public new ServiceStartMode StartType
		{
			get
			{
				return (ServiceStartMode) GetQueryServiceConfigStruct(serviceHandle).dwStartType;
			}
			set
			{
				if (!Advapi32.ChangeServiceConfig(
					serviceHandle.DangerousGetHandle(),
					Advapi32.SERVICE_NO_CHANGE,
					(uint)value,
					Advapi32.SERVICE_NO_CHANGE,
					null,
					null,
					IntPtr.Zero,
					null,
					null,
					null,
					null
				)) throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		// Overridden: Status now reports service status reliably
		public new ServiceControllerStatus Status
		{
			get
			{
				return (ServiceControllerStatus) GetServiceStatusProcessStruct(serviceHandle).dwCurrentState;
			}
		}

		// Helper function: common routine for acquiring a QUERY_SERVICE_CONFIG struct
		private Advapi32.QUERY_SERVICE_CONFIG GetQueryServiceConfigStruct(SafeHandle serviceHandle)
		{
			int pcbBytesNeeded = 0;
			IntPtr lpBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Advapi32.QUERY_SERVICE_CONFIG)));

			/* WORKAROUND: Since Marshal.SizeOf(typeof(Advapi32.QUERY_SERVICE_CONFIG)) can't predict the required value for cbBufSize,
			 * we'll pretend we care about it by first letting the Windows API modify the pcbBytesNeeded variable and then feed 
			 * it back to the API as our cbBufSize in our second QueryServiceConfig() call (even though .NET will allocate the 
			 * memory needed for each string in Advapi32.QUERY_SERVICE_CONFIG for us anyway). */
			Advapi32.QueryServiceConfig(
				serviceHandle.DangerousGetHandle(),
				lpBuffer,
				0,
				out pcbBytesNeeded
			);

			// obtain the actual, desired information
			if (!Advapi32.QueryServiceConfig(
					serviceHandle.DangerousGetHandle(),
					lpBuffer,
					pcbBytesNeeded,
					out pcbBytesNeeded
			)) throw new Win32Exception(Marshal.GetLastWin32Error());

			return (Advapi32.QUERY_SERVICE_CONFIG) Marshal.PtrToStructure(lpBuffer, typeof(Advapi32.QUERY_SERVICE_CONFIG));
		}

		// Helper function: common routine for acquiring a SERVICE_STATUS_PROCESS struct
		private Advapi32.SERVICE_STATUS_PROCESS GetServiceStatusProcessStruct(SafeHandle serviceHandle)
		{
			int pcbBytesNeeded = 0;
			IntPtr lpBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Advapi32.SERVICE_STATUS_PROCESS)));

			if (!Advapi32.QueryServiceStatusEx(
				serviceHandle.DangerousGetHandle(),
				0,
				lpBuffer,
				Marshal.SizeOf(typeof(Advapi32.SERVICE_STATUS_PROCESS)),
				out pcbBytesNeeded
			)) throw new Win32Exception(Marshal.GetLastWin32Error());

			return (Advapi32.SERVICE_STATUS_PROCESS) Marshal.PtrToStructure(lpBuffer, typeof(Advapi32.SERVICE_STATUS_PROCESS));
		}

		private Advapi32.SERVICE_STATUS_PROCESS ControlService(SafeHandle serviceHandle, uint control)
		{
			IntPtr lpServiceStatus = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Advapi32.SERVICE_STATUS_PROCESS)));

			if (!Advapi32.ControlService(
				serviceHandle.DangerousGetHandle(),
				control,
				lpServiceStatus
			)) throw new Win32Exception(Marshal.GetLastWin32Error());

			return (Advapi32.SERVICE_STATUS_PROCESS)Marshal.PtrToStructure(lpServiceStatus, typeof(Advapi32.SERVICE_STATUS_PROCESS));
		}
		// New: force the service to stop immediately
		public void ForceStop()
		{
			ForceStop(new TimeSpan(0));
		}

		// New: force the service to stop after a grace period of "timeout"
		public void ForceStop(TimeSpan timeout)
		{
			// try and stop the service normally
			ControlService(serviceHandle, Advapi32.SERVICE_CONTROL_STOP);
			try
			{
				WaitForStatus(ServiceControllerStatus.Stopped, timeout);
			}
			catch
			{
				// stop the service by killing its process
				Process.GetProcessById((int) ProcessId).Kill();
			}
		}

		// Overridden: adjusted to better differentiate objects of this class from those of System.ServiceProcess.ServiceController
		public override bool Equals(Object obj)
		{
			return obj.GetType().Equals(typeof(ServiceController)) && ((ServiceController) obj).ServiceName.Equals(this.ServiceName);
		}

		// Overridden: programmically identical to the parent class's equivalent
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
