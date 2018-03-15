using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIBB.WinApp.Model
{
    public static class Utiles
    {
        public static string getLast(this string @string, int largo)
        {
            if (@string == null)
            {
                return @string;
            }

            if (largo < 0)
            {
                return String.Empty;
            }

            if (largo >= @string.Length)
            {
                return @string;
            }
            else
            {
                return @string.Substring(@string.Length - largo);
            }
        }

        public static string Izquierda(this string @string, int largo)
        {
            if (@string == null)
            {
                return @string;
            }

            if (largo < 0)
            {
                return String.Empty;
            }

            if (largo >= @string.Length)
            {
                return @string;
            }
            else
            {
                return @string.Substring(0, largo);
            }
        }
    }
}
