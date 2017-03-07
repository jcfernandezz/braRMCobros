using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Globalization;

using Microsoft.Dynamics.GP.eConnect;
using Microsoft.Dynamics.GP.eConnect.Serialization;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;


namespace DIBB.WinApp.Business
{
    public class GPImportar
    {
        private string connectionString = "";
        private string _pre = "";
        private string _custnmbr = String.Empty;
        private string _docnmbr = String.Empty;
        private Decimal _amount = 0;
        private Parametros _param;

        BackgroundWorker backgroundWorker = new BackgroundWorker();
        int cantidad = 0;
        
        Model.LstDatos _datos;

        public GPImportar(string pre)
        {
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[pre].ToString();
            _pre = pre;
            _param = new Parametros(_pre);
            InitializeBackgroundWorker();
        }

        public string GetCompany()
        {
            string sql = "select CMPNYNAM from dynamics..sy01500 where INTERID = DB_NAME()";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandType = CommandType.Text;

                cmd.CommandTimeout = 0;
                cmd.Connection.Open();

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    return dt.Rows[0]["CMPNYNAM"].ToString();
                }
            }
        }

        private string getCustnmbr(string beneficiario)
        {
            return getCustnmbrDocnumbr(beneficiario, "custnmbr");
        }

        private string getCustnmbrDocnumbr(string beneficiario, string campo)
        {
            string sql = "select custnmbr, docnumbr from vwRmTransaccionesTodas where docnumbr = @beneficiario";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@beneficiario", SqlDbType.VarChar, 50).Value = beneficiario;

                cmd.CommandTimeout = 0;
                cmd.Connection.Open();

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                        return dt.Rows[0][campo].ToString();
                    else
                        return null;
                }
            }
        }
        /// <summary>
        /// Obtiene el id de cliente y el número de factura a aplicar
        /// </summary>
        /// <param name="beneficiario">Número de RPS</param>
        /// <param name="dueDate">Fecha de vencimiento del boleto bancario</param>
        /// <returns></returns>
        private void getCustnmbrDocnumbr(string beneficiario, DateTime dueDate)
        {
            int intDueDate = dueDate.Year * 10000 + dueDate.Month * 100 + dueDate.Day;
            _custnmbr = String.Empty;
            _docnmbr = String.Empty;
            _amount = 0;

            //string sql = "select top 1 custnmbr, docnumbr from vwLocBraBoletosBancarios where inv_no = @beneficiario and intDueDate = @intDueDate";
            string sql = "select top 1 custnmbr, docnumbr, Amount from dbo.vwLocBraRmFacturasYBolBancarios where inv_no = @beneficiario and Amount!=0 order by Payment_Date";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@beneficiario", SqlDbType.VarChar, 50).Value = beneficiario;
                cmd.Parameters.Add("@intDueDate", SqlDbType.Int).Value = intDueDate;

                cmd.CommandTimeout = 0;
                cmd.Connection.Open();

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        _custnmbr = dt.Rows[0]["custnmbr"].ToString();
                        _docnmbr = dt.Rows[0]["docnumbr"].ToString();
                        _amount = Convert.ToDecimal( dt.Rows[0]["Amount"]);
                    }
                }
            }
        }

        public void ImportarGPPM(Model.LstDatos datos)
        {
            _datos = datos;

            backgroundWorker.RunWorkerAsync();
        }

        private void InitializeBackgroundWorker()
        {
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;

            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string mensajeError = "";
            string mensajeOk = "";

            mensajeError = ((string[])(e.UserState))[0];
            mensajeOk = ((string[])(e.UserState))[1];
            

            if (mensajeError != "")
            {
                ErrorImportarEventArgs args = new ErrorImportarEventArgs();
                //args.Archivo = archivo;
                args.Error = mensajeError;

                OnErrorImportar(args);
            }

            if (mensajeOk != "")
            {
                ProcesoOkImportarEventArgs args = new ProcesoOkImportarEventArgs();
                //args.Archivo = archivo;
                args.Msg = mensajeOk;

                OnProcesoOkImportar(args);
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;
            
            ProcesarDatos(worker, e);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown. 

            //evento de finalizacion
        }

        private void ProcesarDatos(BackgroundWorker worker, DoWorkEventArgs e)
        {
            string mensajeOk = "";
            string mensajeError = "";

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string nroLote = DateTime.Now.ToString("yyyyMMdd.HHmmss");
            worker.ReportProgress(0, new string[] { "Batch " + nroLote, "Batch " + nroLote });

            foreach (var dato in _datos.Informacion)
            {
                mensajeOk = "";
                mensajeError = "";
                DateTime fechaCobro = dato.FechaTotalLiquidado.AddDays(_param.FechaTotalLiquidadoAddDays);

                using (eConnectMethods eConnectMethods = new eConnectMethods())
                {
                    eConnectMethods.RequireProxyService = true;
                    List<RMCashReceiptsType> masterRMCashReceiptsType = new List<RMCashReceiptsType>();
                    List<RMApplyType> masterRMApplyType = new List<RMApplyType>();

                    try
                    {
                        bool error = false;

                        RMCashReceiptsType RMCashReceiptsTypeEntry = new RMCashReceiptsType();

                        taRMCashReceiptInsert CashReceiptItem = new taRMCashReceiptInsert();
                        this.getCustnmbrDocnumbr(dato.NumeroFactura.Substring(0, 7), dato.FechaVencimientoPago);

                        CashReceiptItem.CUSTNMBR = _custnmbr;   // this.getCustnmbr(dato.NumeroFactura);
                        CashReceiptItem.DOCNUMBR = "RB" + dato.NumeroCobro;
                        CashReceiptItem.DOCDATE = fechaCobro.ToString(System.Configuration.ConfigurationManager.AppSettings[_pre + "_FormatoFecha"]);
                        CashReceiptItem.ORTRXAMT = dato.ValorPago;
                        CashReceiptItem.GLPOSTDT = CashReceiptItem.DOCDATE;
                        CashReceiptItem.BACHNUMB = nroLote;
                        CashReceiptItem.CSHRCTYP = 0;
                        CashReceiptItem.CHEKBKID = System.Configuration.ConfigurationManager.AppSettings[_pre + "_CHEKBKID"];
                        CashReceiptItem.CHEKNMBR = dato.NumeroCobro.ToString();
                        CashReceiptItem.TRXDSCRN = dato.NumeroFactura;
                        //CashReceiptItem.CURNCYID = "BRL";
                        RMApplyType RMApplyTypeEntry = new RMApplyType();

                        taRMApply ApplyItem = new taRMApply();
                        ApplyItem.APTODCNM = _docnmbr.Trim();  // dato.NumeroFactura;
                        ApplyItem.APFRDCNM = "RB" + dato.NumeroCobro;
                        ApplyItem.APPTOAMT = dato.ValorPago - _amount > 0 ? _amount:dato.ValorPago;
                        ApplyItem.APFRDCTY = 9;
                        ApplyItem.APTODCTY = 1;
                        ApplyItem.APPLYDATE = fechaCobro.ToString(System.Configuration.ConfigurationManager.AppSettings[_pre + "_FormatoFecha"]);
                        ApplyItem.GLPOSTDT = fechaCobro.ToString(System.Configuration.ConfigurationManager.AppSettings[_pre + "_FormatoFecha"]);
                        
                        cantidad++;

                        if (!error)
                        {
                            // Serialize the master vendor type in memory.
                            eConnectType eConnectType = new eConnectType();
                            MemoryStream memoryStream = new MemoryStream();
                            XmlSerializer xmlSerializer = new XmlSerializer(eConnectType.GetType());


                            RMCashReceiptsTypeEntry.taRMCashReceiptInsert = CashReceiptItem;
                            masterRMCashReceiptsType.Add(RMCashReceiptsTypeEntry);
                            eConnectType.RMCashReceiptsType = masterRMCashReceiptsType.ToArray();


                            RMApplyTypeEntry.taRMApply = ApplyItem;
                            masterRMApplyType.Add(RMApplyTypeEntry);
                            eConnectType.RMApplyType = masterRMApplyType.ToArray();

                            xmlSerializer.Serialize(memoryStream, eConnectType);

                            // Reset the position of the memory stream to the start.              
                            memoryStream.Position = 0;

                            // Create an XmlDocument from the serialized eConnectType in memory.
                            XmlDocument xmlDocument = new XmlDocument();
                            xmlDocument.Load(memoryStream);
                            memoryStream.Close();

                            string xmlEconn = xmlDocument.OuterXml;

                            eConnectMethods.CreateEntity(connectionString, xmlEconn);

                            mensajeOk = dato.NumeroFactura + " - " + dato.NumeroCobro + ": OK" + Environment.NewLine;
                        }
                        else
                        {
                            mensajeError = dato.NumeroFactura + " - " + dato.NumeroCobro + ": Error" + Environment.NewLine;
                        }

                        System.Threading.Thread.Sleep(100);

                    }
                    catch (Exception ex)
                    {
                        String msj = String.Empty;
                        if (_custnmbr.Equals(String.Empty))
                            msj = "Invoice or Bank slip doesn't exist in GP.";

                        mensajeError = dato.NumeroFactura + " - " + dato.NumeroCobro + ": Error. " + msj + Environment.NewLine + ex.Message + Environment.NewLine;
                    }
                    finally
                    {
                        eConnectMethods.Dispose();

                        worker.ReportProgress(0, new string[] { mensajeError, mensajeOk });
                    }
                }
            }
            worker.ReportProgress(0, new string[] { "Process finished.", "Process finished." });
        }

        #region Eventos
        #region Error
        public event EventHandler<ErrorImportarEventArgs> ErrorImportar;

        protected virtual void OnErrorImportar(ErrorImportarEventArgs e)
        {
            EventHandler<ErrorImportarEventArgs> handler = ErrorImportar;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public class ErrorImportarEventArgs : EventArgs
        {
            public string Archivo { get; set; }
            public string Error { get; set; }
        }
        #endregion

        #region OK
        public event EventHandler<ProcesoOkImportarEventArgs> ProcesoOkImportar;

        protected virtual void OnProcesoOkImportar(ProcesoOkImportarEventArgs e)
        {
            EventHandler<ProcesoOkImportarEventArgs> handler = ProcesoOkImportar;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public class ProcesoOkImportarEventArgs : EventArgs
        {
            public string Archivo { get; set; }
            public string Msg { get; set; }
        }
        #endregion
        #endregion
    }
}