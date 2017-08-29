using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using System.Resources;
using System.Reflection;
using System.Globalization;

namespace Senosiain.Web
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CultureInfo ci = new CultureInfo(CultureInfo.CurrentUICulture.Name);
                string homeDirectory = @"D:\InventivaGS\2014\Noviembre\04-11-2014\PagoProveedores\Archivos\";
                string extensionFiles = "*.txt";

                string[] filePaths = Directory.GetFiles(homeDirectory, extensionFiles);

                foreach (string archivo in filePaths)
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

                    if (File.Exists(homeDirectory + archivoIni))
                        File.Delete(homeDirectory + archivoIni);

                    using (StreamWriter writer = new StreamWriter(homeDirectory + archivoIni))
                    {
                        writer.Write(sb.ToString());
                    }

                    DataTable dtIMEIS = GetDataFromFile(archivo);

                    ReportViewer1.ProcessingMode = ProcessingMode.Local;
                    ReportViewer1.Reset();
                    ReportViewer1.LocalReport.Dispose();
                    ReportViewer1.LocalReport.DataSources.Clear();

                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/reporteFacturas.rdlc");

                    ReportDataSource dataSource = new ReportDataSource("dsFacturas", dtIMEIS);
                    ReportViewer1.LocalReport.DataSources.Add(dataSource);


                    ReportParameter parameter1 = new ReportParameter("tipoArchivo", "PAGADAS");

                    ReportViewer1.LocalReport.SetParameters(parameter1);
                    ReportViewer1.LocalReport.Refresh();
                }
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