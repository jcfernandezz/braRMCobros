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

        public void ProcesaBandeja(Model.BoletosBrasil cobros, TargetGP target)
        {
            switch (target)
            {
                case TargetGP.RMCobro:
                    _cobros = cobros;
                    InitializeBackgroundWorker();
                    backgroundWorker.RunWorkerAsync();
                    break;
                case TargetGP.RMNotaCredito:
                    throw new InvalidOperationException("No se puede integrar a GP porque no está implementado el destino: " + target.ToString());
                //_cobros = cobros;
                //InitializeBackgroundWorker();
                //backgroundWorker.RunWorkerAsync();
                //break;
                default:
                    throw new InvalidOperationException("No se puede integrar a GP porque no está implementado el destino: "+target.ToString() + " [BandejaXLIntegraCobrosBoletos.ProcesaBandeja]");
            }
        }


        private void InitializeBackgroundWorker()
        {
            backgroundWorker = new BackgroundWorker();
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
        /// <param name="beneficiario">Número de RPS</param>
        /// <param name="dueDate">Fecha de vencimiento del boleto bancario</param>
        /// <returns></returns>
        private DataRow getCustnmbrDocnumbr(string beneficiario, DateTime dueDate)
        {
            int intDueDate = dueDate.Year * 10000 + dueDate.Month * 100 + dueDate.Day;
            //_custnmbr = String.Empty;
            //_docnmbr = String.Empty;
            //_amount = 0;

            //string sql = "select top 1 custnmbr, docnumbr from vwLocBraBoletosBancarios where inv_no = @beneficiario and intDueDate = @intDueDate";
            string sql = "select top 1 custnmbr, docnumbr, Amount from dbo.vwLocBraRmFacturasYBolBancarios where inv_no = @beneficiario and Amount!=0 order by Payment_Date";
            using (SqlConnection conn = new SqlConnection(parametrosCobrosXL.ConnStringTarget))
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
                        return dt.Rows[0];

                        //_custnmbr = dt.Rows[0]["custnmbr"].ToString();
                        //_docnmbr = dt.Rows[0]["docnumbr"].ToString();
                        //_amount = Convert.ToDecimal(dt.Rows[0]["Amount"]);
                    }
                    else
                        return null;
                }
            }
        }

        private void ProcesarDatos(BackgroundWorker worker, DoWorkEventArgs e)
        {
            string mensajeOk = "";
            string mensajeError = "";
            DataRow custData = null;

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string nroLote = DateTime.Now.ToString("yyyyMMdd.HHmmss");
            worker.ReportProgress(0, new string[] { "Batch " + nroLote, "Batch " + nroLote });
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
                        string numFactura = dato.NumeroFactura.Trim().Length > 7 ? dato.NumeroFactura.Substring(0, 8) : dato.NumeroFactura.Substring(0, 7);
                        custData = this.getCustnmbrDocnumbr(numFactura.Trim(), dato.FechaVencimientoPago);

                        CashReceiptItem.CUSTNMBR = custData["custnmbr"].ToString(); //_custnmbr;   
                        CashReceiptItem.DOCNUMBR = "RB" + dato.NumeroCobro;
                        CashReceiptItem.DOCDATE = fechaCobro.ToString(parametrosCobrosXL.FormatoFecha); //System.Configuration.ConfigurationManager.AppSettings[_pre + "_FormatoFecha"]);
                        CashReceiptItem.ORTRXAMT = dato.ValorPago;
                        CashReceiptItem.GLPOSTDT = CashReceiptItem.DOCDATE;
                        CashReceiptItem.BACHNUMB = nroLote;
                        CashReceiptItem.CSHRCTYP = 0;
                        CashReceiptItem.CHEKBKID = parametrosCobrosXL.ChekbkidDefault;                         //System.Configuration.ConfigurationManager.AppSettings[_pre + "_CHEKBKID"];
                        CashReceiptItem.CHEKNMBR = dato.NumeroCobro.ToString();
                        CashReceiptItem.TRXDSCRN = dato.NumeroFactura;
                        //CashReceiptItem.CURNCYID = "BRL";
                        RMApplyType RMApplyTypeEntry = new RMApplyType();

                        taRMApply ApplyItem = new taRMApply();
                        ApplyItem.APTODCNM = custData["docnumbr"].ToString();   // _docnmbr.Trim(); 
                        ApplyItem.APFRDCNM = "RB" + dato.NumeroCobro;
                        ApplyItem.APPTOAMT = dato.ValorPago - Convert.ToDecimal(custData["Amount"]) > 0 ? Convert.ToDecimal(custData["Amount"]) : dato.ValorPago;
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
                        String msj = custData == null ? String.Empty : "Invoice or Bank slip doesn't exist in GP.";
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

    }
}
