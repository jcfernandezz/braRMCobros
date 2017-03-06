using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIBB.WinApp.Model
{
    public static class Utiles
    {
        public static string getLast(this string @string, int amount)
        {
            if (@string == null)
            {
                return @string;
            }

            if (amount < 0)
            {
                return String.Empty;
            }

            if (amount >= @string.Length)
            {
                return @string;
            }
            else
            {
                return @string.Substring(@string.Length - amount);
            }
        }

    }
}
