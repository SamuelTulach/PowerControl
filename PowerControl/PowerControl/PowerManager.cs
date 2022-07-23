using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace PowerControl
{
    internal class PowerManager : ApplicationContext
    {
        private NotifyIcon _mainNotifyIcon;
        private Thread _mainThread;

        private Guid _lowPowerPlan = new Guid("a1841308-3541-4fab-bc81-f71556f20b4a");
        private Guid _balancedPowerPlan = new Guid("381b4222-f694-41f0-9685-ff5bb260df2e");
        private Guid _highPowerPlan = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");

        private void Exit(object sender, EventArgs e)
        {
            if (_mainThread.IsAlive)
                _mainThread.Abort();

            _mainNotifyIcon.Visible = false;
            Application.Exit();
        }

        private Guid GetActiveGuid()
        {
            var activeScheme = Guid.Empty;
            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            if (Native.PowerGetActiveScheme((IntPtr)null, out ptr) == 0)
                activeScheme = (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));

            Marshal.FreeHGlobal(ptr);
            return activeScheme;
        }

        private void SwitchToLowPowerPlan()
        {
            var current = GetActiveGuid();
            if (current != _lowPowerPlan)
            {
                Native.PowerSetActiveScheme(IntPtr.Zero, ref _lowPowerPlan);
            }
        }

        private void SwitchToHighPowerPlan()
        {
            var current = GetActiveGuid();
            if (current != _balancedPowerPlan)
            {
                Native.PowerSetActiveScheme(IntPtr.Zero, ref _balancedPowerPlan);
            }
        }

        private void MainThread()
        {
            var targetProcessList = new List<string>();
            targetProcessList.Add("devenv");
            targetProcessList.Add("steam");
            targetProcessList.Add("Code");
            targetProcessList.Add("clion64");
            targetProcessList.Add("rider64");
            targetProcessList.Add("vmware"); // workstation
            targetProcessList.Add("ida64");
            targetProcessList.Add("ida");
            targetProcessList.Add("windbg");
            targetProcessList.Add("dnSpy");
            targetProcessList.Add("obs64");

            while (true)
            {
                Thread.Sleep(1000);

                var processFound = false;
                var processList = Process.GetProcesses();
                foreach (var process in processList)
                {
                    if (targetProcessList.Contains(process.ProcessName))
                        processFound = true;
                }

                if (processFound)
                {
                    _mainNotifyIcon.Icon = Properties.Resources.high_power;
                    SwitchToHighPowerPlan();
                }
                else
                {
                    _mainNotifyIcon.Icon = Properties.Resources.low_power;
                    SwitchToLowPowerPlan();
                }
            }
        }

        public PowerManager()
        {
            _mainNotifyIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.low_power,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };

            _mainThread = new Thread(MainThread);
            _mainThread.Start();
        }
    }
}
