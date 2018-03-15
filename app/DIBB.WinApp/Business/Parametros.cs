using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIBB.WinApp.Business
{
    public class Parametros : Model.IParametrosCobrosBoletosXL
    {
        string prefijoGeneral = String.Empty;
        string prefijoTipoArchivo = String.Empty;
        int _NumeroCobroCol;
        int _NumeroFacturaCol;
        int _CodigoLiquidacionCol;
        int _FechaVencimientoPagoCol;
        int _ValorBoletoCol;
        int _JurosCol;
        int _AbatimentoCol;
        int _ValorPagoCol;
        int _NombrePagadorCol;
        int _FechaTotalLiquidadoCol;
        int _FechaFila;
        int _FechaCol;
        int _IniciaDatosFila;
        int _FechaTotalLiquidadoAddDays;
        string _formatoFecha;
        string _chekbkid;
        string _connStringTarget;
        string _connStringTargetEF;
        string _rutaLog;

        public int NumeroCobroCol
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_NumeroCobroCol"]);
            }

            set
            {
                _NumeroCobroCol = value;
            }
        }

        public int NumeroFacturaCol
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_NumeroFacturaCol"]); ;
            }

            set
            {
                _NumeroFacturaCol = value;
            }
        }

        public int CodigoLiquidacionCol
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_CodigoLiquidacionCol"]);
            }

            set
            {
                _CodigoLiquidacionCol = value;
            }
        }

        public int FechaVencimientoPagoCol
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_FechaVencimientoPagoCol"]);
            }

            set
            {
                _FechaVencimientoPagoCol = value;
            }
        }

        public int ValorBoletoCol
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_ValorBoletoCol"]);
            }

            set
            {
                _ValorBoletoCol = value;
            }
        }

        public int JurosCol
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_JurosCol"]);
            }

            set
            {
                _JurosCol = value;
            }
        }

        public int AbatimentoCol
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_AbatimentoCol"]);
            }

            set
            {
                _AbatimentoCol = value;
            }
        }

        public int ValorPagoCol
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_ValorPagoCol"]);
            }

            set
            {
                _ValorPagoCol = value;
            }
        }

        public int NombrePagadorCol
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_NombrePagadorCol"]);
            }

            set
            {
                _NombrePagadorCol = value;
            }
        }

        public int FechaTotalLiquidadoCol
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_FechaTotalLiquidadoCol"]);
            }

            set
            {
                _FechaTotalLiquidadoCol = value;
            }
        }

        public int FechaFila
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_FechaFila"]);
            }

            set
            {
                _FechaFila = value;
            }
        }

        public int FechaCol
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_FechaCol"]);
            }

            set
            {
                _FechaCol = value;
            }
        }

        public int IniciaDatosFila
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_IniciaDatosFila"]);
            }

            set
            {
                _IniciaDatosFila = value;
            }
        }

        public int FechaTotalLiquidadoAddDays
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_FechaTotalLiquidadoAddDays"]);
            }

            set
            {
                _FechaTotalLiquidadoAddDays = value;
            }
        }

        public string ChekbkidDefault
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + prefijoTipoArchivo + "_CHEKBKID"];
            }

            set
            {
                _chekbkid = value;
            }
        }

        public string FormatoFecha
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings[prefijoGeneral + "_FormatoFecha"];
            }

            set
            {
                _formatoFecha = value;
            }
        }

        public string ConnStringTarget
        {
            get
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings[prefijoGeneral]?.ConnectionString; 
            }

            set
            {
                _connStringTarget = value;
            }
        }

        public string ConnectionStringTargetEF
        {
            get
            {
                return _connStringTargetEF;
            }

            set
            {
                _connStringTargetEF = value;
            }
        }

        public string RutaLog
        {
            get
            {
                return _rutaLog;
            }

            set
            {
                _rutaLog = value;
            }
        }

        public Parametros(String prefijo, String prefTipoArchivo)
        {
            prefijoGeneral = prefijo;
            prefijoTipoArchivo = prefTipoArchivo;
        }
    }
}
