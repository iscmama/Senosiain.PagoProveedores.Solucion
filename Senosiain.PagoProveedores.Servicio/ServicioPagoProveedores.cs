using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Runtime.InteropServices;
using Senosiain.Bussiness.Logic;
using System.Configuration;

using System.Resources;
using System.Reflection;

namespace Senosiain.PagoProveedores.Servicio
{
    public partial class ServicioPagoProveedores : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        private string Path
        {
            get
            {
                string servicePath = this.GetType().Assembly.Location;
                string appConfigPath = servicePath.Substring(0, servicePath.LastIndexOf('\\') + 1) + "Config.ini";

                return appConfigPath;
            }
        }

        public ServicioPagoProveedores()
        {
            InitializeComponent();

            eventLog1 = new System.Diagnostics.EventLog();

            string eventLogName = INI.Read("SenosiainProveedoresServicio", "EventLogName", Path);

            if (!System.Diagnostics.EventLog.SourceExists(eventLogName))
            {
                System.Diagnostics.EventLog.CreateEventSource(eventLogName, eventLogName);
            }

            eventLog1.Source = eventLogName;
            eventLog1.Log = eventLogName;
        }
        protected override void OnStart(string[] args)
        {
            try
            {
                eventLog1.WriteEntry("El servicio de pago de proveedores Inicia.");

                // Set up a timer to trigger every minute.
                System.Timers.Timer timer = new System.Timers.Timer();
                string intervalTimeMilliseconds = INI.Read("SenosiainProveedoresServicio", "IntervalTimeMilliseconds", Path);

                eventLog1.WriteEntry("El servicio se ejecutara cada " + intervalTimeMilliseconds + " milisegundos.");

                timer.Interval = int.Parse(intervalTimeMilliseconds); // 5 minutos
                timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
                timer.Start();

                // Update the service state to Start Pending.
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry(ex.Message);
            }
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("El servicio de pago de proveedores se detuvo.");
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            eventLog1.WriteEntry("Ejecutando proceso de generacion de facturas", EventLogEntryType.Information);
            ArchivosComponent archivos = new ArchivosComponent();
            archivos.Procesar();
        }
        protected override void OnContinue()
        {
            eventLog1.WriteEntry("El servicio de pago de proveedores continua.");
            // Set up a timer to trigger every minute.
            System.Timers.Timer timer = new System.Timers.Timer();
            string intervalTimeMilliseconds = INI.Read("SenosiainProveedoresServicio", "IntervalTimeMilliseconds", Path);

            eventLog1.WriteEntry("El servicio se ejecutara cada " + intervalTimeMilliseconds + " milisegundos.");

            timer.Interval = int.Parse(intervalTimeMilliseconds); // 5 minutos
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }  
    }
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public long dwServiceType;
        public ServiceState dwCurrentState;
        public long dwControlsAccepted;
        public long dwWin32ExitCode;
        public long dwServiceSpecificExitCode;
        public long dwCheckPoint;
        public long dwWaitHint;
    };
}
