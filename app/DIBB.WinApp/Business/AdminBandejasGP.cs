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
using DIBB.WinApp.Model;

namespace DIBB.WinApp.Business
{
    public class AdminBandejasGP
    {
        Model.IParametrosCobrosBoletosXL paramCobros;
        IBandejaXLCobros integraCobrosXL;

        public IBandejaXLCobros IntegraCobrosXL
        {
            get
            {
                return integraCobrosXL;
            }

            set
            {
                integraCobrosXL = value;
            }
        }

        public IParametrosCobrosBoletosXL ParamCobros
        {
            get
            {
                return paramCobros;
            }

            set
            {
                paramCobros = value;
            }
        }

        public AdminBandejasGP(Model.IParametrosCobrosBoletosXL param)
        {
            paramCobros = param;
            integraCobrosXL = new BandejaXLIntegraCobrosBoletos(param);

            //connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[pre].ToString();
            //_pre = pre;
            //_param = new Parametros(_pre);
        }

        public string GetCompany()
        {
            string sql = "select CMPNYNAM from dynamics..sy01500 where INTERID = DB_NAME()";
            using (SqlConnection conn = new SqlConnection(paramCobros.ConnStringTarget))
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
            using (SqlConnection conn = new SqlConnection(paramCobros.ConnStringTarget))
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

        public void ProcesaBandejaXL(Model.BoletosBrasil cobros, Bandeja.TargetGP destinoGP)
        {
            integraCobrosXL.ProcesaBandeja(cobros, destinoGP);
        }
    }
}