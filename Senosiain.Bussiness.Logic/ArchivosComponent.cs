using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using System.Configuration;
using System.Diagnostics;
using System.Net;

namespace Senosiain.Bussiness.Logic
{
    [Serializable]
    public partial class ArchivosComponent
    {
        EventLog eventLog1 = new EventLog();

        private string _Path
        {
            get
            {
                string servicePath = this.GetType().Assembly.Location;
                string appConfigPath = servicePath.Substring(0, servicePath.LastIndexOf('\\') + 1) + "Config.ini";

                return appConfigPath;
            }
        }

        public ArchivosComponent()
        {
            eventLog1 = new System.Diagnostics.EventLog();

            string servicePath = this.GetType().Assembly.Location;
            string appConfigPath = servicePath.Substring(0, servicePath.LastIndexOf('\\') + 1) + "Config.ini";

            string eventLogName = INI.Read("SenosiainProveedoresServicio", "EventLogName", appConfigPath);

            if (!System.Diagnostics.EventLog.SourceExists(eventLogName))
            {
                System.Diagnostics.EventLog.CreateEventSource(eventLogName, eventLogName);
            }

            eventLog1.Source = eventLogName;
            eventLog1.Log = eventLogName;
        }

        public void Procesar()
        {
            string directoryFiles = INI.Read("SenosiainProveedoresServicio", "DirectoryFiles", _Path);
            string directoryInvoice = INI.Read("SenosiainProveedoresServicio", "DirectoryInvoice", _Path);
            string directoryTemplate = INI.Read("SenosiainProveedoresServicio", "DirectoryTemplate", _Path);
            string server = INI.Read("SenosiainProveedoresServicio", "Server", _Path);
            string user = INI.Read("SenosiainProveedoresServicio", "User", _Path);
            string pass = INI.Read("SenosiainProveedoresServicio", "Pass", _Path);

            try
            {
                string homeDirectoryFiles = directoryFiles;

                string extensionFiles = "*.txt";

                eventLog1.WriteEntry("Solicitando acceso con credenciales UNCAccessWithCredentials", EventLogEntryType.Information);

                using (UNCAccessWithCredentials unc = new UNCAccessWithCredentials())
                {
                    eventLog1.WriteEntry("Accediendo a carpeta para lectura", EventLogEntryType.Information);

                    if (unc.NetUseWithCredentials(directoryFiles, user, server, pass))
                    {
                        eventLog1.WriteEntry("Acceso concedido", EventLogEntryType.Information);

                        eventLog1.WriteEntry("Eliminando archivos PDF...", EventLogEntryType.Information);

                        string[] filesPDF = Directory.GetFiles(directoryFiles, "*.pdf");

                        foreach (string file in filesPDF)
                        {
                            if (File.Exists(file))
                                File.Delete(file);
                        }

                        eventLog1.WriteEntry("Archivos PDF eliminados...", EventLogEntryType.Information);

                        string[] filePaths = Directory.GetFiles(directoryFiles, extensionFiles);
                        eventLog1.WriteEntry("Archivos encontrados: " + filePaths.Count().ToString(), EventLogEntryType.Information);

                        int total = 0;

                        foreach (string archivo in filePaths)
                        {
                            if (new FileInfo(archivo).Length > 0)
                            {
                                string archivoIni = "schema.ini";

                                StringBuilder sb = new StringBuilder();
                                sb.AppendFormat("[{0}]", Path.GetFileName(archivo));
                                sb.AppendLine("");
                                sb.AppendLine("ColNameHeader=False");
                                sb.AppendLine("Format=Delimited(|)");
                                sb.AppendLine("Col1=FECHA Datetime");
                                sb.AppendLine("Col2=IDPROVEEDOR Text");
                                sb.AppendLine("Col3=NOMBREPROVEEDOR Text");
                                sb.AppendLine("Col4=IDFACTURA Text");
                                sb.AppendLine("Col5=CLAVEFACTURA Text");
                                sb.AppendLine("Col6=REFERENCIA Text");
                                sb.AppendLine("Col7=IMPORTE Double");
                                sb.AppendLine("Col8=MONEDA Text");
                                sb.AppendLine("Col9=TIPOCAMBIO Double");

                                if (File.Exists(homeDirectoryFiles + @"\" + archivoIni))
                                    File.Delete(homeDirectoryFiles + @"\" + archivoIni);

                                using (StreamWriter writer = new StreamWriter(homeDirectoryFiles + @"\" + archivoIni))
                                {
                                    writer.Write(sb.ToString());
                                }

                                DataTable dtIMEIS = GetDataFromFile(archivo);

                                ReportViewer ReportViewer1 = new ReportViewer();
                                ReportViewer1.ProcessingMode = ProcessingMode.Local;
                                ReportViewer1.Reset();
                                ReportViewer1.LocalReport.Dispose();
                                ReportViewer1.LocalReport.DataSources.Clear();

                                ReportViewer1.LocalReport.ReportPath = directoryTemplate + @"\" + "reporteFacturas.rdlc";

                                ReportDataSource dataSource = new ReportDataSource("dsFacturas", dtIMEIS);
                                ReportViewer1.LocalReport.DataSources.Add(dataSource);

                                string tipoArchivo = String.Empty;
                                string nombreclatura = String.Empty;

                                if (Path.GetFileName(archivo).ToUpper().StartsWith("F"))
                                {
                                    tipoArchivo = "PAGADAS";
                                    nombreclatura = "F";
                                }
                                else
                                    if (Path.GetFileName(archivo).ToUpper().StartsWith("C"))
                                    {
                                        tipoArchivo = "CANCELADAS";
                                        nombreclatura = "C";
                                    }

                                ReportParameter parameter1 = new ReportParameter("tipoArchivo", tipoArchivo);

                                ReportViewer1.LocalReport.SetParameters(parameter1);
                                ReportViewer1.LocalReport.Refresh();

                                Warning[] warnings;
                                string[] streamids;
                                string mimeType = string.Empty;
                                string encoding = string.Empty;
                                string extension = string.Empty;

                                string homeDirectoryFacturas = directoryInvoice;

                                byte[] bytes = ReportViewer1.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamids, out warnings);

                                string idProveedor = dtIMEIS.Rows[0]["IDPROVEEDOR"].ToString();

                                string fileNameWithOutExtension = Path.GetFileNameWithoutExtension(archivo);

                                string fileNew = homeDirectoryFacturas + @"\" + fileNameWithOutExtension + ".pdf";

                                using (FileStream fs = File.Create(fileNew))
                                {
                                    fs.Write(bytes, 0, bytes.Length);

                                    if (File.Exists(archivo))
                                    {
                                        File.Delete(archivo);
                                    }
                                }

                                total++;
                            } // Fin de archivos no vacios.
                            else
                            {
                                if (File.Exists(archivo))
                                {
                                    File.Delete(archivo);
                                }
                            }
                        } // Fin del foreach.

                        eventLog1.WriteEntry("Total de archivos procesados: " + total.ToString(), EventLogEntryType.SuccessAudit);

                        unc.NetUseDelete();

                        eventLog1.WriteEntry("Cerrando conexion", EventLogEntryType.Information);
                    }
                    else
                    {
                        eventLog1.WriteEntry("Error=" + unc.LastError.ToString(), EventLogEntryType.Error);
                    }

                    unc.Dispose();
                }
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry("Error al procesar los archivos: " + ex.Message, EventLogEntryType.Error);
            }            
        }
        private DataTable GetDataFromFile(string fileFull)
        {
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            DataTable dtData = new DataTable();

            try
            {
                string full = Path.GetFullPath(fileFull);
                string file = Path.GetFileName(full);
                string dir = Path.GetDirectoryName(full);                

                string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;"
                              + "Data Source=\"" + dir + "\\\";"
                              + "Extended Properties=\"text;HDR=NO;FMT=Delimited\"";
                
                string query = "SELECT * FROM [" + file + "] WHERE FECHA IS NOT NULL";

                adapter = new OleDbDataAdapter(query, connectionString);
                var dsInformation = new DataSet();
                adapter.Fill(dsInformation);
                dtData = dsInformation.Tables[0];
                return dtData;
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry(ex.Message, EventLogEntryType.Error);
                Exception e = new Exception("Ocurrio un error al intentar leer los datos del archivo/archivo con formato incorrecto.", ex);                
                throw (e);
            }
            finally
            {
                adapter.Dispose();
            }
        }
    }
}

