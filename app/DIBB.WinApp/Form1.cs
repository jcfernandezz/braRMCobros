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
using System.Globalization;

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
        private DateTime ConvierteAFecha(string tipoDato, string strFecha, string formatoFecha, CultureInfo cultura)
        {
            DateTime fecha;
            switch (tipoDato)
            {
                case "System.String":
                    fecha = DateTime.ParseExact(strFecha.Trim(), formatoFecha, cultura);
                    break;
                case "System.Double":
                    fecha = DateTime.FromOADate(double.Parse(strFecha));
                    break;
                default:
                    throw new FormatException("La fecha tiene formato incorrecto: " + strFecha);
            }
            return fecha;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;
            lblProcesos.Text = string.Empty;
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
            CultureInfo invC = CultureInfo.InvariantCulture;

            Stream outputFile = File.Create(@"C:\gpusuario\traceCargaCobros.txt");
            TextWriterTraceListener textListener = new TextWriterTraceListener(outputFile);
            TraceSource trace = new TraceSource("trSource", SourceLevels.All);

            try
            {
                trace.Listeners.Clear();
                trace.Listeners.Add(textListener);
                trace.TraceInformation("Carga cobros");

                Business.Parametros param = new Business.Parametros(companySelected(), cbTipoArchivo.SelectedIndex.ToString());
                BoletosBrasil lst = new BoletosBrasil();

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string archivo = openFileDialog1.FileName;

                    var ds = LeerExcelDataReader(archivo);

                    lst.LBoletosBrasil = new List<BoletoBrasil>();

                    lblProcesos.Text += "Opening " + archivo + Environment.NewLine;
                    lblError.Text += "Opening " + archivo + Environment.NewLine + Environment.NewLine;
                    string tipoDato = string.Empty;
                    for (int i = param.IniciaDatosFila; i < ds.Tables[0].Rows.Count; i++)
                    {
                        if (ds.Tables[0].Rows[i].ItemArray.GetValue(0).ToString() != string.Empty)
                        {
                            BoletoBrasil d = new BoletoBrasil();
                            trace.TraceInformation("fila: " + i.ToString() + " col FechaTotalLiquidadoCol:" + param.FechaTotalLiquidadoCol.ToString());
                            tipoDato = ds.Tables[0].Rows[i].ItemArray.GetValue(param.FechaTotalLiquidadoCol).GetType().ToString();
                            d.FechaTotalLiquidado = ConvierteAFecha(tipoDato, ds.Tables[0].Rows[i].ItemArray.GetValue(param.FechaTotalLiquidadoCol).ToString(), param.FormatoFecha, invC);

                            trace.TraceInformation("fila: " + i.ToString() + " col NumeroCobroCol:" + param.NumeroCobroCol.ToString());
                            d.NumeroCobro = ds.Tables[0].Rows[i].ItemArray.GetValue(param.NumeroCobroCol).ToString().Trim();
                            //en caso de tarjeta, agregar la fecha al número de cobro y NC
                            if (cbTipoArchivo.SelectedIndex == 1)
                                d.NumeroCobro += d.FechaTotalLiquidado.ToString("yyyMMdd");

                            trace.TraceInformation("fila: " + i.ToString() + " col NumeroFacturaCol:" + param.NumeroFacturaCol.ToString());
                            d.NumeroFactura = ds.Tables[0].Rows[i].ItemArray.GetValue(param.NumeroFacturaCol).ToString().Trim();
                            d.NumeroFacturaYCuota = d.NumeroFactura;

                            //el número de la planilla puede venir así: B-10201, B-10201., B-10201 01, B-10201. 01, B10201
                            d.NumeroFactura = d.NumeroFactura.Trim().Length > 7 ? Model.Utiles.Izquierda(d.NumeroFactura, 8) : d.NumeroFactura;
                            if (!d.NumeroFactura.Substring(1, 1).Equals("-"))   //las facturas deben tener guión luego de la serie
                                d.NumeroFactura = d.NumeroFactura.Insert(1, "-");

                            trace.TraceInformation("fila: " + i.ToString() + " col CodigoLiquidacionCol:" + param.CodigoLiquidacionCol.ToString());
                            d.CodigoLiquidacion = ds.Tables[0].Rows[i].ItemArray.GetValue(param.CodigoLiquidacionCol).ToString().Trim();

                            trace.TraceInformation("fila: " + i.ToString() + " col FechaVencimientoPago:" + param.FechaVencimientoPagoCol.ToString());
                            tipoDato = ds.Tables[0].Rows[i].ItemArray.GetValue(param.FechaVencimientoPagoCol).GetType().ToString();
                            d.FechaVencimientoPago = ConvierteAFecha(tipoDato, ds.Tables[0].Rows[i].ItemArray.GetValue(param.FechaVencimientoPagoCol).ToString(), param.FormatoFecha, invC);

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
                                
                            lst.LBoletosBrasil.Add(d);

                            using (var context = new GBRAEntities())
                            {
                                int sp = context.dibb_spCollectionOfBankSlips_insUpd(Utiles.getLast(d.NumeroCobro.ToString(), 21), Utiles.getLast(d.NumeroFacturaYCuota, 21), Utiles.getLast(openFileDialog1.FileName, 150), Utiles.getLast(Environment.UserName, 50));
                                trace.TraceInformation("fila: " + i.ToString() + " resultado dibb_spCollectionOfBankSlips_insUpd: " + sp.ToString());
                            }

                        }
                        else
                            lblError.Text += "Row: " + (i+1).ToString() + " is blank." + Environment.NewLine;
                    }


                    Business.AdminBandejasGP bandeja = new Business.AdminBandejasGP(param);
                    bandeja.IntegraCobrosXL.EventoErrorIntegracion += new EventHandler<Business.ErrorIntegracionEventArgs>(ErroresAlImportar);
                    bandeja.IntegraCobrosXL.EventoAlertaIntegracion += new EventHandler<Business.AlertaIntegracionEventArgs>(Alertas);

                    bandeja.ProcesaBandejaXL(lst, Business.Bandeja.TargetGP.RMCobro);

                    if (cbTipoArchivo.SelectedIndex==1)
                    {
                        //procesa el monto de los juros
                        bandeja.ProcesaBandejaXL(lst, Business.Bandeja.TargetGP.RMNotaCredito);
                    }

                }

            }
            catch (Exception of)
            {
                lblError.Text += "Please check the file. It doesn't look like a file for uploading "+ cbTipoArchivo.SelectedItem.ToString() + Environment.NewLine + Environment.NewLine + of.Message + " (Form1.button1_Click) " + of.StackTrace + Environment.NewLine;
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
            int cbSelectedIndex = 0;
            bool error = false;
            int count = 1;
            cbTipoArchivo.SelectedIndex = cbSelectedIndex;
            while (!error)
            {
                Business.Parametros p = new Business.Parametros("GP_" + count.ToString(), cbSelectedIndex.ToString());
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
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }
    }
}
