using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DIBB.WinApp.Model;
using System.IO;
using Excel;
using System.Diagnostics;

namespace DIBB.WinApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string companySelected()
        {
            return ((ComboBoxItem)cmbEmpresas.SelectedItem).Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.Title = "Seleccionar archivo";

            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;

            openFileDialog1.DefaultExt = "xls";
            openFileDialog1.Filter = "Excel Files(.xls)|*.xls| Excel Files(.xlsx)|*.xlsx| Excel Files(*.xlsm)|*.xlsm";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            openFileDialog1.ReadOnlyChecked = true;
            openFileDialog1.ShowReadOnly = true;

            Stream outputFile = File.Create(@"C:\gpusuario\traceCargaCobros.txt");
            TextWriterTraceListener textListener = new TextWriterTraceListener(outputFile);
            TraceSource trace = new TraceSource("trSource", SourceLevels.All);

            try
            {
                trace.Listeners.Clear();
                trace.Listeners.Add(textListener);
                trace.TraceInformation("Carga cobros");

                Business.Parametros param = new Business.Parametros(companySelected());
                trace.TraceInformation("Inicializa parámetros");

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string archivo = openFileDialog1.FileName;

                    // Get the file we are going to process
                    var ds = LeerExcelDataReader(archivo);
                    trace.TraceInformation("leer excel data reader");

                    BoletosBrasil lst = new BoletosBrasil();
                    lst.LBoletosBrasil = new List<BoletoBrasil>();

                    //string[] aux = ds.Tables[0].Rows[param.FechaFila][param.FechaCol].ToString().Split(':');
                    //DateTime fechaTotalLiquidado = DateTime.Parse(aux[0].ToString().Trim());

                    lblProcesos.Text = "Opening " + archivo + Environment.NewLine;
                    lblError.Text = "Opening " + archivo + Environment.NewLine;

                    for (int i = param.IniciaDatosFila; i < ds.Tables[0].Rows.Count; i++)
                    {
                        if (ds.Tables[0].Rows[i].ItemArray.GetValue(0).ToString() != string.Empty)
                        {
                            BoletoBrasil d = new BoletoBrasil();
                            trace.TraceInformation("fila: " + i.ToString() + " col NumeroCobroCol:" + param.NumeroCobroCol.ToString());
                            d.NumeroCobro = long.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(param.NumeroCobroCol).ToString().Trim());
                            trace.TraceInformation("fila: " + i.ToString() + " col NumeroFacturaCol:" + param.NumeroFacturaCol.ToString());
                            d.NumeroFactura = ds.Tables[0].Rows[i].ItemArray.GetValue(param.NumeroFacturaCol).ToString().Trim();
                            trace.TraceInformation("fila: " + i.ToString() + " col CodigoLiquidacionCol:" + param.CodigoLiquidacionCol.ToString());
                            //d.CodigoLiquidacion = int.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(param.CodigoLiquidacionCol).ToString().Trim());
                            d.CodigoLiquidacion = 0;
                            trace.TraceInformation("fila: " + i.ToString() + " col FechaVencimientoPago:" + param.FechaVencimientoPagoCol.ToString());
                            d.FechaVencimientoPago = DateTime.FromOADate(double.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(param.FechaVencimientoPagoCol).ToString()));
                            trace.TraceInformation("fila: " + i.ToString() + " col ValorBoletoCol:" + param.ValorBoletoCol.ToString());
                            d.ValorBoleto = decimal.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(param.ValorBoletoCol).ToString().Trim());
                            trace.TraceInformation("fila: " + i.ToString() + " col JurosCol:" + param.JurosCol.ToString() + " " + ds.Tables[0].Rows[i].ItemArray.GetValue(param.JurosCol).ToString());

                            d.Juros = ds.Tables[0].Rows[i].ItemArray.GetValue(param.JurosCol) == null || ds.Tables[0].Rows[i].ItemArray.GetValue(param.JurosCol).ToString().Trim().Equals(String.Empty) ? 0 : decimal.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(param.JurosCol).ToString().Trim());
                            trace.TraceInformation("fila: " + i.ToString() + " col AbatimentoCol:" + param.AbatimentoCol.ToString());
                            d.Abatimento = ds.Tables[0].Rows[i].ItemArray.GetValue(param.AbatimentoCol) == null || ds.Tables[0].Rows[i].ItemArray.GetValue(param.AbatimentoCol).ToString().Trim().Equals(String.Empty) ? 0 : decimal.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(param.AbatimentoCol).ToString().Trim());
                            trace.TraceInformation("fila: " + i.ToString() + " col ValorPagoCol:" + param.ValorPagoCol.ToString());
                            d.ValorPago = decimal.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(param.ValorPagoCol).ToString().Trim());
                            trace.TraceInformation("fila: " + i.ToString() + " col NombrePagadorCol:" + param.NombrePagadorCol.ToString());
                            d.NombrePagador = ds.Tables[0].Rows[i].ItemArray.GetValue(param.NombrePagadorCol).ToString();
                            trace.TraceInformation("fila: " + i.ToString() + " col FechaTotalLiquidadoCol:" + param.FechaTotalLiquidadoCol.ToString());
                            d.FechaTotalLiquidado = DateTime.FromOADate(double.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(param.FechaTotalLiquidadoCol).ToString()));
                            lst.LBoletosBrasil.Add(d);

                            using (var context = new GBRAEntities())
                            {
                                int sp = context.dibb_spCollectionOfBankSlips_insUpd(Utiles.getLast(d.NumeroCobro.ToString(), 21), Utiles.getLast(d.NumeroFactura, 21), Utiles.getLast(openFileDialog1.FileName, 150), Utiles.getLast(Environment.UserName, 50));
                                //Console.WriteLine("resultado: " + sp.ToString());
                            }

                        }
                        else
                            break;
                    }


                    Business.AdminBandejasGP bandeja = new Business.AdminBandejasGP(param);

                    bandeja.IntegraCobrosXL.EventoErrorIntegracion += new EventHandler<Business.ErrorIntegracionEventArgs>(ErroresAlImportar);
                    bandeja.IntegraCobrosXL.EventoAlertaIntegracion += new EventHandler<Business.AlertaIntegracionEventArgs>(Alertas);

                    bandeja.ProcesaBandejaXL(lst, Business.Bandeja.TargetGP.RMCobro);

                    if (cbTipoArchivo.SelectedIndex==1)
                    {
                        param.NumeroCobroCol -= 1;
                        bandeja.ProcesaBandejaXL(lst, Business.Bandeja.TargetGP.RMNotaCredito);
                    }

                }

            }
            catch (Exception of)
            {
                lblError.Text += "Exception. Please check the file and the following message: " + of.Message + " (Form1.button1_Click) " + of.TargetSite.ToString() + Environment.NewLine;
            }
            finally
            {
                trace.Flush();
                trace.Close();
            }
        }

        private void IntegraCobros_ErrorIntegracion(object sender, Business.ErrorIntegracionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Alertas(object sender, Business.AlertaIntegracionEventArgs e)
        {
            lblProcesos.Text += e.Msg + Environment.NewLine;
            //lblProcesos.Refresh();
        }

        private void ErroresAlImportar(object sender, Business.ErrorIntegracionEventArgs e)
        {
            lblError.Text += e.Error + Environment.NewLine;
            lblError.Text += "---------------------------" + Environment.NewLine;
            //lblError.Refresh();
        }

        private DataSet LeerExcelDataReader(string archivo)
        {
            FileStream stream = File.Open(archivo, FileMode.Open, FileAccess.Read);

            //1. Reading from a binary Excel file ('97-2003 format; *.xls)
            IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            //...
            //2. Reading from a OpenXml Excel file (2007 format; *.xlsx)
            //IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            //...
            //3. DataSet - The result of each spreadsheet will be created in the result.Tables
            DataSet result = excelReader.AsDataSet();

            return result;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bool error = false;
            int count = 1;
            while (!error)
            {
                Business.Parametros p = new Business.Parametros("GP_" + count.ToString());
                p.ConnStringTarget = System.Configuration.ConfigurationManager.ConnectionStrings["GP_" + count.ToString()]?.ConnectionString;
                if (p.ConnStringTarget != null)
                {
                    Business.AdminBandejasGP oGPI = new Business.AdminBandejasGP(p);

                    cmbEmpresas.Items.Add(new ComboBoxItem("GP_" + count.ToString(), oGPI.GetCompany()));
                    count++;
                }
                else
                    error = true;
            }

            cmbEmpresas.SelectedIndex = 0;
            lblUsuario.Text = Environment.UserDomainName + "\\" + Environment.UserName;
            lblFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
            cbTipoArchivo.SelectedIndex = 0;
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }
    }
}
