using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIBB.WinApp.Business
{
    public interface IBandejaXLCobros : IBandejaXL
    {
        void ProcesaBandeja(Model.BoletosBrasil cobros);

        event EventHandler<ErrorIntegracionEventArgs> EventoErrorIntegracion;

        void OnErrorIntegracion(ErrorIntegracionEventArgs e);

        event EventHandler<AlertaIntegracionEventArgs> EventoAlertaIntegracion;

        void OnAlertaIntegracion(AlertaIntegracionEventArgs e);
    }
}
