using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIBB.WinApp.Business
{
    public class ErrorIntegracionEventArgs : EventArgs
    {
        public string Archivo { get; set; }
        public string Error { get; set; }
    }
}
