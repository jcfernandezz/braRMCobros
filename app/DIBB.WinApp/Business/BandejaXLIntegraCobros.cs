using Microsoft.Dynamics.GP.eConnect.Serialization;
using Microsoft.Dynamics.GP.eConnect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Data.SqlClient;
using System.Data;

namespace DIBB.WinApp.Business
{
    public class BandejaXLIntegraCobrosBoletos : BandejaXL, IBandejaXLCobros
    {
        Model.IParametrosCobrosBoletosXL parametrosCobrosXL;
        BackgroundWorker backgroundWorker;
        Model.BoletosBrasil _cobros;
        int cantidad = 0;
        
        #region Eventos
        public event EventHandler<ErrorIntegracionEventArgs> EventoErrorIntegracion;

        public virtual void OnErrorIntegracion(ErrorIntegracionEventArgs e)
        {
            EventHandler<ErrorIntegracionEventArgs> handler = EventoErrorIntegracion;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<AlertaIntegracionEventArgs> EventoAlertaIntegracion;

        public virtual void OnAlertaIntegracion(AlertaIntegracionEventArgs e)
        {
            EventHandler<AlertaIntegracionEventArgs> handler = EventoAlertaIntegracion;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion


        public BandejaXLIntegraCobrosBoletos(Model.IParametrosCobrosBoletosXL param)
        {
            parametrosCobrosXL = param;

        }

        public void ProcesaBandeja(Model.BoletosBrasil cobros, TargetGP destinoGP)
        {
            _cobros = cobros;
            switch (destinoGP)
            {
                case TargetGP.RMCobro:
                    InitializeBackgroundWorker_RMCobrosYAplicaciones();
                    break;
                case TargetGP.RMNotaCredito:
                    InitializeBackgroundWorker_RMNotaCreditoYAplicaciones();
                    break;
                default:
                    throw new InvalidOperationException("No se puede integrar a GP porque no está implementado el destino: "+destinoGP.ToString() + " [BandejaXLIntegraCobrosBoletos.ProcesaBandeja]");
            }
            backgroundWorker.RunWorkerAsync();
        }


        private void InitializeBackgroundWorker_RMCobrosYAplicaciones()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;

            backgroundWorker.DoWork += new DoWorkEventHandler(BackgroundWorker_RMCobrosYAplicaciones);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);
        }

        private void InitializeBackgroundWorker_RMNotaCreditoYAplicaciones()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;

            backgroundWorker.DoWork += new DoWorkEventHandler(BackgroundWorker_RMNotaCreditoYAplicaciones);
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
                ErrorIntegracionEventArgs args = new ErrorIntegracionEventArgs();
                //args.Archivo = archivo;
                args.Error = mensajeError;

                OnErrorIntegracion(args);
            }

            if (mensajeOk != "")
            {
                AlertaIntegracionEventArgs args = new AlertaIntegracionEventArgs();
                //args.Archivo = archivo;
                args.Msg = mensajeOk;

                OnAlertaIntegracion(args);
            }
        }

        private void BackgroundWorker_RMCobrosYAplicaciones(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            IntegraRMCobrosYAplicaciones(worker, e);
        }

        private void BackgroundWorker_RMNotaCreditoYAplicaciones(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            IntegraRMNotaCreditoYAplicaciones(worker, e);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown. 

            //evento de finalizacion
        }

        public XmlDocument Serializa(eConnectType eConnect)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                XmlSerializer xmlSerializer = new XmlSerializer(eConnect.GetType());

                xmlSerializer.Serialize(memoryStream, eConnect);

                memoryStream.Position = 0;

                // Create an XmlDocument from the serialized eConnectType in memory.
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(memoryStream);
                memoryStream.Close();

                return xmlDocument;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Obtiene el id de cliente y el número de factura a aplicar
        /// </summary>
        /// <param name="numDoc">Número de RPS</param>
        /// <param name="dueDate">Fecha de vencimiento del boleto bancario</param>
        /// <returns></returns>
        private Model.RMFactura getCustnmbrDocnumbr(string numDoc, DateTime dueDate)
        {
            int intDueDate = dueDate.Year * 10000 + dueDate.Month * 100 + dueDate.Day;

            //string sql = "select top 1 custnmbr, docnumbr from vwLocBraBoletosBancarios where inv_no = @beneficiario and intDueDate = @intDueDate";
            string sql = "select top 1 custnmbr, docnumbr, Amount from dbo.vwLocBraRmFacturasYBolBancarios where inv_no = @beneficiario and Amount!=0 order by Payment_Date";
            using (SqlConnection conn = new SqlConnection(parametrosCobrosXL.ConnStringTarget))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@beneficiario", SqlDbType.VarChar, 50).Value = numDoc;
                cmd.Parameters.Add("@intDueDate", SqlDbType.Int).Value = intDueDate;

                cmd.CommandTimeout = 0;
                cmd.Connection.Open();

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        return new Model.RMFactura()
                            { Custnmbr= dt.Rows[0]["custnmbr"].ToString().Trim(),
                              Docnmbr = dt.Rows[0]["docnumbr"].ToString().Trim(),
                              Amount = Convert.ToDecimal(dt.Rows[0]["Amount"])
                            };
                        }
                    else
                        throw new ArgumentNullException("Invoice "+numDoc+" doesn't exist in GP with balance > 0.");
                }
            }
        }

        /// <summary>
        /// Integra recibos de cobro y aplica a facturas
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        private void IntegraRMCobrosYAplicaciones(BackgroundWorker worker, DoWorkEventArgs e)
        {
            string mensajeOk = "";
            string mensajeError = "";
            Model.RMFactura docGP = null;

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string nroLote = DateTime.Now.ToString("yyyyMMdd.HHmmss");
            worker.ReportProgress(0, new string[] { "Collection receipt batch " + nroLote, "Collection receipt batch " + nroLote });
            cantidad = 0;
            foreach (var dato in _cobros.LBoletosBrasil)
            {
                mensajeOk = "";
                mensajeError = "";
                DateTime fechaCobro = dato.FechaTotalLiquidado.AddDays(parametrosCobrosXL.FechaTotalLiquidadoAddDays);
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
                        //el número de la planilla puede venir así: B-10201, B-10201., B-10201 01, B-10201. 01
                        //string numFactura = dato.NumeroFactura.Trim().Length > 7 ? Model.Utiles.Izquierda(dato.NumeroFactura, 8) : Model.Utiles.Izquierda(dato.NumeroFactura, 7);
                        docGP = this.getCustnmbrDocnumbr(dato.NumeroFactura.Trim(), dato.FechaVencimientoPago);
                        decimal valorBoleto = decimal.Round( dato.ValorBoleto, 2);
                        decimal valorPago = decimal.Round(dato.ValorPago, 2);

                        CashReceiptItem.CUSTNMBR = docGP.Custnmbr;   // custData["custnmbr"].ToString(); //_custnmbr;   
                        CashReceiptItem.DOCNUMBR = "RB" + dato.NumeroCobro;
                        CashReceiptItem.DOCDATE = fechaCobro.ToString(parametrosCobrosXL.FormatoFecha); //System.Configuration.ConfigurationManager.AppSettings[_pre + "_FormatoFecha"]);
                        CashReceiptItem.ORTRXAMT = valorPago;
                        CashReceiptItem.GLPOSTDT = CashReceiptItem.DOCDATE;
                        CashReceiptItem.BACHNUMB = nroLote;
                        CashReceiptItem.CSHRCTYP = 0;
                        CashReceiptItem.CHEKBKID = parametrosCobrosXL.ChekbkidDefault;                         //System.Configuration.ConfigurationManager.AppSettings[_pre + "_CHEKBKID"];
                        CashReceiptItem.CHEKNMBR = dato.NumeroCobro.ToString();
                        CashReceiptItem.TRXDSCRN = dato.NumeroFacturaYCuota;
                        RMApplyType RMApplyTypeEntry = new RMApplyType();

                        taRMApply ApplyItem = new taRMApply();
                        ApplyItem.APTODCNM = docGP.Docnmbr;  // custData["docnumbr"].ToString();   // _docnmbr.Trim(); 
                        ApplyItem.APFRDCNM = "RB" + dato.NumeroCobro;
                        ApplyItem.APPTOAMT = valorPago - Convert.ToDecimal(docGP.Amount) > 0 ? Convert.ToDecimal(docGP.Amount) : valorPago;
                        ApplyItem.APFRDCTY = 9;
                        ApplyItem.APTODCTY = 1;
                        ApplyItem.APPLYDATE = fechaCobro.ToString(parametrosCobrosXL.FormatoFecha);     // System.Configuration.ConfigurationManager.AppSettings[_pre + "_FormatoFecha"]);
                        ApplyItem.GLPOSTDT = fechaCobro.ToString(parametrosCobrosXL.FormatoFecha);      // System.Configuration.ConfigurationManager.AppSettings[_pre + "_FormatoFecha"]);

                        cantidad++;

                        if (!error)
                        {
                            eConnectType eConnDoc = new eConnectType();
                            RMCashReceiptsTypeEntry.taRMCashReceiptInsert = CashReceiptItem;
                            masterRMCashReceiptsType.Add(RMCashReceiptsTypeEntry);
                            eConnDoc.RMCashReceiptsType = masterRMCashReceiptsType.ToArray();

                            RMApplyTypeEntry.taRMApply = ApplyItem;
                            masterRMApplyType.Add(RMApplyTypeEntry);
                            eConnDoc.RMApplyType = masterRMApplyType.ToArray();

                            XmlDocument xmlDoc = Serializa(eConnDoc);
                            eConnectMethods.CreateEntity(parametrosCobrosXL.ConnStringTarget, xmlDoc.OuterXml);

                            mensajeOk = dato.NumeroFactura + " - " + dato.NumeroCobro + ": Collection receipt OK" + Environment.NewLine;
                        }
                        else
                        {
                            mensajeError = dato.NumeroFactura + " - " + dato.NumeroCobro + ": Error" + Environment.NewLine;
                        }

                        System.Threading.Thread.Sleep(100);

                    }
                    catch (eConnectException ec)
                    {
                        mensajeError = dato.NumeroFactura + " - " + dato.NumeroCobro + " eConn: " + ec.Message + Environment.NewLine + ec.StackTrace;
                    }
                    catch (Exception ex)
                    {
                        mensajeError = dato.NumeroFactura + " - " + dato.NumeroCobro + ": " + ex.Message + Environment.NewLine+ ex.StackTrace;
                    }
                    finally
                    {
                        eConnectMethods.Dispose();

                        worker.ReportProgress(0, new string[] { mensajeError, mensajeOk });
                    }
                }
            }
            worker.ReportProgress(0, new string[] { "Collection receipt uploading finished.", "Collection receipt uploading finished." });
        }

        /// <summary>
        /// Integra nc de AR y aplica facturas
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        private void IntegraRMNotaCreditoYAplicaciones(BackgroundWorker worker, DoWorkEventArgs e)
        {
            string mensajeOk = "";
            string mensajeError = "";
            Model.RMFactura docGP = null;

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string nroLote = DateTime.Now.ToString("yyyyMMdd.HHmmss");
            worker.ReportProgress(0, new string[] { "Credit memo batch " + nroLote, "Credit memo Batch " + nroLote });
            cantidad = 0;
            foreach (var dato in _cobros.LBoletosBrasil)
            {
                mensajeOk = "";
                mensajeError = "";
                DateTime fechaCobro = dato.FechaTotalLiquidado.AddDays(parametrosCobrosXL.FechaTotalLiquidadoAddDays);
                using (eConnectMethods eConnectMethods = new eConnectMethods())
                {
                    eConnectMethods.RequireProxyService = true;
                    List<RMTransactionType> masterRMTransactionType = new List<RMTransactionType>();
                    List<RMApplyType> masterRMApplyType = new List<RMApplyType>();

                    try
                    {
                        bool error = false;
                        decimal valorBoleto = decimal.Round(dato.ValorBoleto, 2);
                        decimal valorPago = decimal.Round(dato.ValorPago, 2);
                        decimal juros = decimal.Round(dato.Juros, 2);

                        //RMCashReceiptsType RMCashReceiptsTypeEntry = new RMCashReceiptsType();
                        RMTransactionType RMTransactionTypeEntry = new RMTransactionType();

                        taRMTransaction rmTransactionItem = new taRMTransaction();
                        //el número de la planilla puede venir así: B-10201, B-10201., B-10201 01, B-10201. 01
                        //string numFactura = dato.NumeroFactura.Trim().Length > 7 ? Model.Utiles.Izquierda(dato.NumeroFactura, 8) : Model.Utiles.Izquierda( dato.NumeroFactura, 7);
                        docGP = this.getCustnmbrDocnumbr(dato.NumeroFactura.Trim(), dato.FechaVencimientoPago);

                        rmTransactionItem.CUSTNMBR = docGP.Custnmbr;   // custData["custnmbr"].ToString(); //_custnmbr;   
                        rmTransactionItem.DOCNUMBR = "CC" + dato.NumeroCobro;
                        rmTransactionItem.DOCDATE = fechaCobro.ToString(parametrosCobrosXL.FormatoFecha); //System.Configuration.ConfigurationManager.AppSettings[_pre + "_FormatoFecha"]);
                        rmTransactionItem.RMDTYPAL = 7;
                        rmTransactionItem.DOCAMNT = juros;
                        rmTransactionItem.SLSAMNT = juros;

                        rmTransactionItem.BACHNUMB = nroLote;
                        rmTransactionItem.DOCDESCR = dato.NumeroFacturaYCuota;
                        rmTransactionItem.CSTPONBR = dato.NombrePagador;

                        RMApplyType RMApplyTypeEntry = new RMApplyType();

                        taRMApply ApplyItem = new taRMApply();
                        ApplyItem.APTODCNM = docGP.Docnmbr;  // custData["docnumbr"].ToString();   // _docnmbr.Trim(); 
                        ApplyItem.APFRDCNM = "CC" + dato.NumeroCobro;
                        ApplyItem.APPTOAMT = juros - Convert.ToDecimal(docGP.Amount) > 0 ? Convert.ToDecimal(docGP.Amount) : juros;
                        ApplyItem.APFRDCTY = 7;
                        ApplyItem.APTODCTY = 1;
                        ApplyItem.APPLYDATE = fechaCobro.ToString(parametrosCobrosXL.FormatoFecha);     // System.Configuration.ConfigurationManager.AppSettings[_pre + "_FormatoFecha"]);
                        ApplyItem.GLPOSTDT = fechaCobro.ToString(parametrosCobrosXL.FormatoFecha);      // System.Configuration.ConfigurationManager.AppSettings[_pre + "_FormatoFecha"]);

                        cantidad++;

                        if (!error)
                        {
                            eConnectType eConnDoc = new eConnectType();
                            RMTransactionTypeEntry.taRMTransaction = rmTransactionItem;
                            masterRMTransactionType.Add(RMTransactionTypeEntry);
                            eConnDoc.RMTransactionType = masterRMTransactionType.ToArray();

                            RMApplyTypeEntry.taRMApply = ApplyItem;
                            masterRMApplyType.Add(RMApplyTypeEntry);
                            eConnDoc.RMApplyType = masterRMApplyType.ToArray();

                            XmlDocument xmlDoc = Serializa(eConnDoc);
                            eConnectMethods.CreateEntity(parametrosCobrosXL.ConnStringTarget, xmlDoc.OuterXml);

                            mensajeOk = dato.NumeroFactura + " - " + dato.NumeroCobro + ": Credit Memo OK" + Environment.NewLine;
                        }
                        else
                        {
                            mensajeError = dato.NumeroFactura + " - " + dato.NumeroCobro + ": Error" + Environment.NewLine;
                        }

                        System.Threading.Thread.Sleep(100);

                    }
                    catch (eConnectException ec)
                    {
                        mensajeError = dato.NumeroFactura + " - " + dato.NumeroCobro + " eConn: " + ec.Message + Environment.NewLine + ec.StackTrace;
                    }
                    catch (Exception ex)
                    {
                        mensajeError = dato.NumeroFactura + " - " + dato.NumeroCobro + ": " + ex.Message + Environment.NewLine + ex.StackTrace;
                    }
                    finally
                    {
                        eConnectMethods.Dispose();

                        worker.ReportProgress(0, new string[] { mensajeError, mensajeOk });
                    }
                }
            }
            worker.ReportProgress(0, new string[] { "Credit memo uploading finished.", "Credit memo uploading finished." });
        }

    }
}
