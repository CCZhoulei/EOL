using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace PDACode
{
    class DataBaseStruct
    {
        [DllImport("Coredll.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("Coredll.dll")]
        public static extern bool EndDialog(IntPtr hDlg, out IntPtr nResult);
    }
}
