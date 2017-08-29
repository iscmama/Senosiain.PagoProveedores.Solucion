using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Senosiain.Bussiness.Logic;

namespace Senosiain.PagoProveedores.Servicio
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;

            ServicesToRun = new ServiceBase[] 
            { 
                new ServicioPagoProveedores() 
            };

            ServiceBase.Run(ServicesToRun);

            ArchivosComponent archivos = new ArchivosComponent();
            archivos.Procesar();
        }
    }
}
