using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EOLProject
{
    public partial class Form1 : Form
    {
        //日志文件记录
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        //线程
        Thread thread;    
        //实例化PLC连接
        IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.37"), 3001);
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //实例化网络通讯检测
        Ping ping = new Ping();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.dataGridView1.Rows[0].Cells[0].Value = "制造参数值";
            EOLEntities entities = new EOLEntities();       //保证程序打开后界面不显示总成条码
            var m_data = entities.Only.FirstOrDefault();
            m_data.Assemblycode = "";
            entities.SaveChanges();
            thread = new Thread(Detection);    //执行程序
            thread.Start();
        }


        /// <summary>
        /// 主程序运行
        /// </summary>
        public void Detection()
        {
            PingReply testReply = ping.Send("192.168.0.37");    //将连接PLC放在循环外，如果放在循环内部会重复连接PLC（循环内部在判断与PLC通讯状态异常处会重新连接PLC）
            if (testReply.Status == IPStatus.Success)
            {
                socket.Connect(iPEndPoint);    //连接PLC
            }

            while (true)
            {
                try
                {
                    #region 判断与PLC、PDA网络通讯
                    PingReply pdaReply = ping.Send("192.168.1.2");
                    if (pdaReply.Status == IPStatus.Success)
                    {

                    }
                    else
                    {
                        this.pictureBox1.BackColor = Color.Red;
                    }
                    PingReply plcReply = ping.Send("192.168.0.37");
                    if (plcReply.Status == IPStatus.Success)
                    {

                    }
                    else
                    {
                        this.pictureBox2.BackColor = Color.Red;
                        socket.Connect(iPEndPoint);    //连接PLC
                    }
                    #endregion

                    if (pdaReply.Status == IPStatus.Success)    //如果PDA网络通讯正常才会执行程序
                    {
                        EOLEntities entities = new EOLEntities();
                        var Data = entities.Only.FirstOrDefault();    //获取数据库总成条码信息
                        if (Data.Assemblycode.Length == 23)           //判断总成条码长度是否符合总成条码编码规则
                        {
                            string str = this.textBox2.Text;          //获取系统界面总成条码的信息
                            if (str != (Data.Assemblycode))           //判断系统界面总成条码与数据库总成条码是否一致（不一致系统界面则更换总成条码）
                            {
                                string number = Data.Assemblycode.Substring(0, 4);    //从总成条码中获取产品编号
                                this.textBox1.Invoke(new Action(() => { this.textBox1.Text = number; }));    //界面信息显示产品编号
                                this.textBox2.Invoke(new Action(() => { this.textBox2.Text = Data.Assemblycode; }));    //界面信息显示总成条码
                            }
                            if (plcReply.Status == IPStatus.Success)    //判断与PLC通讯是否正常
                            {
                                List<string> list = new List<string>();
                                var PlcData = entities.PlcDot.ToList();     //从数据库获取需要读取数据的PLC点位
                                for (int i = 0; i < PlcData.Count(); i++)       //for循环需要读取点位的长度，下发报文，接收PLC反馈，解析PLC反馈，添加到本地数组
                                {
                                    byte[] sendBytes = Transform(PlcData[i].Comm, PlcData[i].Subcomm, PlcData[i].Adress, PlcData[i].Length);    //组建报文
                                    socket.Send(sendBytes);    //下发报文

                                    byte[] buffer = new byte[512];
                                    int count = socket.Receive(buffer);    //接收报文
                                    byte[] recv = new byte[count];
                                    Buffer.BlockCopy(buffer, 0, recv, 0, count);    //转换到新数组

                                    string result = AnalysisSingleData(recv, PlcData[i].Type);    //解析PLC反馈
                                    list.Add(result);    //添加到本地数组
                                }
                                this.dataGridView1.Rows[0].Cells[1].Value = list[0];    //界面显示PLC数据
                                this.dataGridView1.Rows[0].Cells[2].Value = list[1];
                                this.dataGridView1.Rows[0].Cells[3].Value = list[2];
                                this.dataGridView1.Rows[0].Cells[4].Value = list[3];
                                this.dataGridView1.Rows[0].Cells[5].Value = list[4];
                                this.button1.Invoke(new Action(() => { this.button1.Enabled = true; }));    //保存数据并打印按钮可以使用
                            }
                            else
                            {
                                this.label5.Invoke(new Action(() => { this.label5.Text = "PLC获取数据失败，请检查PLC通讯"; }));
                            }
                        }
                    }
                    else
                    {
                        this.label5.Invoke(new Action(() => { this.label5.Text = "PDA连接失败无法获取总成条码"; }));
                    }
                }
                catch (Exception ex)
                {
                    logger.Info(ex);
                }
            }
        }

        /// <summary>
        /// 清空界面信息按钮，将系统界面信息清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            this.textBox1.Invoke(new Action(() => { this.textBox1.Text = ""; }));
            this.textBox2.Invoke(new Action(() => { this.textBox2.Text = ""; }));
            this.dataGridView1.Rows.Clear();
            this.button1.Invoke(new Action(() => { this.button1.Enabled = false; }));
            this.button2.Invoke(new Action(() => { this.button2.Enabled = false; }));
            EOLEntities entities = new EOLEntities();    //清空界面信息按钮是程序已经将此条保存到数据库并打印了，界面也不应该显示此条总成条码了，
                                                         //所以对数据库总成条码做出修改，这样程序再次运行就不会显示上次的总成条码了
            var m_data = entities.Only.FirstOrDefault();
            m_data.Assemblycode = "";
            entities.SaveChanges();
            this.label6.Invoke(new Action(() => { this.label6.Text = ""; }));
        }

        /// <summary>
        /// 转换成PLC报文
        /// </summary>
        /// <param name="order">命令：批量读取</param>
        /// <param name="zorder">子命令：按位读取</param>
        /// <param name="data">PLC点位</param>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        public static byte[] Transform(string order, string zorder, string data, string length)
        {
            string address = data.Substring(0, 1);
            switch (address)
            {
                case "M":
                    address = "90";
                    break;
                case "D":
                    address = "A8";
                    break;
                default:
                    break;
            }
            int d = Convert.ToInt32(data.Substring(1));
            data = Convert.ToString(d, 16);
            switch (data.Length)
            {
                case 5:
                    data = "0" + data;
                    break;
                case 4:
                    data = "00" + data;
                    break;
                case 3:
                    data = "000" + data;
                    break;
                case 2:
                    data = "0000" + data;
                    break;
                case 1:
                    data = "00000" + data;
                    break;
                default:
                    break;
            }
            byte[] header = new byte[21];// ={ 0x50,0x00,0x00,0xFF,0xFF,0x03,0x00,0x0C,0x00,001,0x00,0x01,0x14,0x00,0x00,0xA6,0x27,0x00,0xA8,0x01,0x00,0x01,0x02};
            header[0] = 0x50;//固定（命令）:表示发起指令
            header[1] = 0x00;
            header[2] = 0x00;//固定（网路编号）：上位访问下位
            header[3] = 0xFF;//固定（PLC编号）：上位访问下位
            header[4] = 0xFF;//固定（请求目标模块IO编号）：十进制1023
            header[5] = 0x03;
            header[6] = 0x00;//固定（请求目标模块站编号）：上位访问下位
            header[7] = 0x0C;//（应答数据物理长度）：值12，表示后面的报文内容长度是12
            header[8] = 0x00;
            header[9] = 0x10;//（CPU监视定时器）：值4秒，表示等待PLC相应的timeout时间
            header[10] = 0x00;
            header[11] = Convert.ToByte(order.Substring(0, 2), 16);//0104(命令):表示批量读取，1401表示随机读取
            header[12] = Convert.ToByte(order.Substring(2, 2), 16);
            header[13] = Convert.ToByte(zorder.Substring(0, 2), 16);//0000（子命令）：值是0表示按字读取（一个字=16位），如果值是1按位读取
            header[14] = Convert.ToByte(zorder.Substring(2, 2), 16);
            header[15] = Convert.ToByte(data.Substring(4, 2), 16);//258000（首地址）：十进制值600
            header[16] = Convert.ToByte(data.Substring(2, 2), 16);
            header[17] = Convert.ToByte(data.Substring(0, 2), 16);
            header[18] = Convert.ToByte(address.Substring(0, 2), 16);//A8  （软元件）：表示PLC寄存器的类型，A8-D点、90-M点、9C-X点、9D-Y点、B0-ZR外部存储卡
            header[19] = Convert.ToByte(length.Substring(0, 2), 16);//0100（读取长度）：十进制1
            header[20] = Convert.ToByte(length.Substring(2, 2), 16);
            return header;
        }

        /// <summary>
        /// 对PLC反馈的数据进行针对型解析:数据类型
        /// </summary>
        /// <param name="dataIn">PLC反馈</param>
        /// <param name="type">数据类型</param>
        /// <returns></returns>
        public static string AnalysisSingleData(byte[] dataIn, string type)
        {
            byte[] temp = new byte[4];
            int analyIndex = 11;
            string result = null;

            try
            {
                if (dataIn.Length < 13)
                    return "";
                int dataSize = dataIn[8] << 8;
                dataSize += dataIn[7];
                dataSize = (dataSize - 2);
                int error = dataIn[9] << 8;
                error += dataIn[10];
                if (error != 0)
                    return "";
                if (type == "string")
                {
                    temp = new byte[dataSize];

                    Buffer.BlockCopy(dataIn, analyIndex, temp, 0, dataSize);
                    string strCode = System.Text.Encoding.ASCII.GetString(temp);
                    result = strCode.Replace("\r\n", "").Replace("\0", "");
                }
                else if (type == "int")
                {
                    result = (dataIn[11] + (dataIn[12] << 8)).ToString();
                }
                else if (type == "float")
                {
                    temp[0] = dataIn[analyIndex];
                    temp[1] = dataIn[analyIndex + 1];
                    temp[2] = dataIn[analyIndex + 2];
                    temp[3] = dataIn[analyIndex + 3];
                    result = System.BitConverter.ToSingle(temp, 0).ToString();
                }
                else if (type == "bool")
                {
                    int idd = ((dataIn[11] & 0xFF) | (dataIn[12]));
                    bool bTrig = false;
                    if (dataIn[11] % 2 == 0)
                    {
                        bTrig = false;
                    }
                    else
                    {
                        bTrig = true;
                    }

                    result = bTrig.ToString();
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
            return result;
        }

        /// <summary>
        /// 保存数据并打印按钮，将界面中的数据保存到数据库，连接打印机并打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            EOLEntities entities = new EOLEntities();
            Save save = new Save();
            save.Assemblycode = this.textBox2.Text;
            save.Number = this.textBox1.Text;
            save.Data1 = this.dataGridView1.Rows[0].Cells[1].Value.ToString();
            save.Data2 = this.dataGridView1.Rows[0].Cells[2].Value.ToString();
            save.Data3 = this.dataGridView1.Rows[0].Cells[3].Value.ToString();
            save.Data4 = this.dataGridView1.Rows[0].Cells[4].Value.ToString();
            save.Data5 = this.dataGridView1.Rows[0].Cells[5].Value.ToString();
            save.Date = DateTime.Now;
            entities.Save.Add(save);
            entities.SaveChanges();
            this.button2.Invoke(new Action(() => { this.button2.Enabled = true; }));
            this.label6.Invoke(new Action(() => { this.label6.Text = "保存数据并打印成功"; }));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
