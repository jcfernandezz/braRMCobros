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

            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string archivo = openFileDialog1.FileName;

                    // Get the file we are going to process
                    var ds = LeerExcelDataReader(archivo);

                    LstDatos lst = new LstDatos();
                    lst.Informacion = new List<Datos>();

                    string[] aux = ds.Tables[0].Rows[9][0].ToString().Split(':');
                    DateTime fechaTotalLiquidado = DateTime.Parse(aux[1].ToString().Trim());

                    lblProcesos.Text = "Opening " + archivo + Environment.NewLine;
                    lblError.Text = "Opening " + archivo + Environment.NewLine;

                    for (int i = 14; i < ds.Tables[0].Rows.Count; i++)
                    {

                        if (ds.Tables[0].Rows[i].ItemArray.GetValue(0).ToString() != string.Empty)
                        {
                            Datos d = new Datos();
                            d.NumeroCobro = long.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(0).ToString().Trim());
                            d.NumeroFactura = ds.Tables[0].Rows[i].ItemArray.GetValue(1).ToString().Trim();
                            d.CodigoLiquidacion = int.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(2).ToString().Trim());
                            d.FechaVencimientoPago = DateTime.FromOADate(double.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(3).ToString()));
                            d.ValorBoleto = decimal.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(4).ToString().Trim());
                            d.Juros = decimal.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(5).ToString().Trim());
                            d.Abatimento = decimal.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(6).ToString().Trim());
                            d.ValorPago = decimal.Parse(ds.Tables[0].Rows[i].ItemArray.GetValue(7).ToString().Trim());
                            d.NombrePagador = ds.Tables[0].Rows[i].ItemArray.GetValue(8).ToString();
                            d.FechaTotalLiquidado = fechaTotalLiquidado;
                            lst.Informacion.Add(d);

                            using (var context = new GBRAEntities())
                            {
                                int sp = context.dibb_spCollectionOfBankSlips_insUpd(Utiles.getLast(d.NumeroCobro.ToString(), 21), Utiles.getLast(d.NumeroFactura, 21), Utiles.getLast(openFileDialog1.FileName, 150), Utiles.getLast(Environment.UserName, 50));
                                //Console.WriteLine("resultado: " + sp.ToString());
                            }

                        }
                        else
                            break;
                    }

                    Business.GPImportar gpi = new Business.GPImportar(companySelected());

                    gpi.ErrorImportar += new EventHandler<Business.GPImportar.ErrorImportarEventArgs>(ErrorImportar);
                    gpi.ProcesoOkImportar += new EventHandler<Business.GPImportar.ProcesoOkImportarEventArgs>(ProcesoOkImportar);

                    gpi.ImportarGPPM(lst);
                }

            }
            catch (Exception of)
            {
                lblError.Text += "Error. Please check the file and the following message: (Form1.button1_Click) " + of.Message + Environment.NewLine;
            }
        }

        private void ProcesoOkImportar(object sender, Business.GPImportar.ProcesoOkImportarEventArgs e)
        {
            lblProcesos.Text += e.Msg + Environment.NewLine;
            //lblProcesos.Refresh();
        }

        private void ErrorImportar(object sender, Business.GPImportar.ErrorImportarEventArgs e)
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
                if (System.Configuration.ConfigurationManager.ConnectionStrings["GP_" + count.ToString()] != null)
                {
                    Business.GPImportar oGPI = new Business.GPImportar("GP_" + count.ToString());

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

        private void versión10ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }
    }
}
