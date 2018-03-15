using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIBB.WinApp.Model
{
    public class BoletoBrasil
    {
        public string NumeroCobro { get; set; }
        public string NumeroFactura { get; set; }
        public string NumeroFacturaYCuota { get; set; }
        public string CodigoLiquidacion { get; set; }
        public DateTime FechaVencimientoPago { get; set; }
        public decimal ValorBoleto { get; set; }
        public decimal Juros { get; set; }
        public decimal Abatimento { get; set; }
        public decimal ValorPago { get; set; }
        public string NombrePagador { get; set; }
        public DateTime FechaTotalLiquidado { get; set; }
    }

    public class BoletosBrasil
    {
        public List<BoletoBrasil> LBoletosBrasil { get; set; }
    }
}
