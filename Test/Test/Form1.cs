using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TechTalk.SpecFlow;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ZebraPrintHelper.PrinterProgrammingLanguage = ProgrammingLanguage.EPL;
            ZebraPrintHelper.PrinterName = "ZDesigner GK888t (EPL)";
            ZebraPrintHelper.PrinterType = DeviceType.DRV;
            string s = this.textBox1.Text; 
            string cmd = ZPLOrder(this.textBox1.Text);

            ZebraPrintHelper.PrintCommand(cmd.ToString());
        }
        public string ZPLOrder(string text)
        {
            //string s = "^XA\r\n\r\n^JMA^LL450^PW700^MD0^PR3^PON^LRN^LH20,70\r\n\r\n^CWJ,E:MSUNG.FNT^FS\r\n\r\n^CI26\r\n\r\n^FO200,0\r\n\r\n^BQN,2,4\r\n\r\n^FDHM,B0200 " + text  + "^FS\r\n\r\n^FO300,10^AFN,0,0,10^FD 生产时间："+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "^FS\r\n\r\n^XZ";

            string s="^XA\r\n\r\n^JMA^LL450^PW700^MD0^PR3^PON^LRN^LH0,0\r\n\r\n^CI26\r\n\r\n^CWJ,E:MSUNG.FNT^FS\r\n\r\n^FO200,0\r\n\r\n^BQN,2,6\r\n\r\n^FDHM,B0200 "+text+"^FS\r\n\r\n^FO500,200^AFN,0,20,10^FD总成条码^FS\r\n\r\n^FO500,200^AFN,0,20,10^FD时间^FS\r\n\r\n^FO500,200^AFN,0,20,10^FD生产时间^FS\r\n\r\n^FO500,200^AFN,0,20,10^FD"+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "^FS\r\n\r\n^XZ\r\n"







            return s;
        }
    }
}
