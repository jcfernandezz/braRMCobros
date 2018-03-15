using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIBB.WinApp.Model
{
    public class RMFactura
    {
        String _custnmbr;
        String _docnmbr;
        Decimal _amount;

        public string Custnmbr
        {
            get
            {
                return _custnmbr;
            }

            set
            {
                _custnmbr = value;
            }
        }

        public string Docnmbr
        {
            get
            {
                return _docnmbr;
            }

            set
            {
                _docnmbr = value;
            }
        }

        public decimal Amount
        {
            get
            {
                return _amount;
            }

            set
            {
                _amount = value;
            }
        }
    }
}
