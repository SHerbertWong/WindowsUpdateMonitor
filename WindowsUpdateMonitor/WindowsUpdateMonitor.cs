using System;
using System.ServiceProcess;
using System.Threading;

namespace System
{
	public partial class WindowsUpdateMonitor : ServiceBase
	{
		private static readonly string WindowsUpdateServiceName = "wuauserv";

		public static void Main(string[] args)
		{
			ServiceBase.Run(new WindowsUpdateMonitor());
		}

		private int GracePreiod = 15;
		private SystemEx.ServiceProcess.ServiceController ServiceController;
		private Thread MonitorThread;

		public WindowsUpdateMonitor()
		{
			InitializeComponent();

			ServiceController = new SystemEx.ServiceProcess.ServiceController(WindowsUpdateServiceName);

			MonitorThread = new Thread(StartMonitorRouintine)
			{
				Name = "Windows Update Monitor",
				IsBackground = true
			};
		}

		private void InitializeComponent()
		{
			this.CanShutdown = true;
			this.ServiceName = "WuauServMon";
			this.CanStop = true;
		}

		private void StartMonitorRouintine()
		{
			try
			{
				while (true)
				{
					// Wait for Windows Update Service to enter "Running" status
					ServiceController.WaitForStatus(ServiceControllerStatus.Running);

					// Stop Windows Update service
					ServiceController.ForceStop(new TimeSpan(GracePreiod * 10000000));

					// Set Windows Update start mode to "Disabled"
					ServiceController.StartType = ServiceStartMode.Disabled;
				}
			}
			catch (ThreadAbortException)
			{
				// Do nothing
			}
		}

		protected override void OnShutdown()
		{
			base.OnShutdown();

			// Stop monitoring Windows Update
			MonitorThread.Abort();
			
			// Stop Windows Update service *immediately*
			ServiceController.ForceStop();

			// Set Windows Update start mode to "Disabled"
			ServiceController.StartType = ServiceStartMode.Disabled;
		}

		protected override void OnStart(string[] args)
		{
			base.OnStart(args);

			// Start monitoring Windows Update for activity
			MonitorThread.Start();
		}

		protected override void OnStop()
		{
			base.OnStop();

			// Stop monitoring Windows Update
			MonitorThread.Abort();

			// Set Windows Update start mode to "Manual"
			ServiceController.StartType = ServiceStartMode.Manual;
		}
	}
}
