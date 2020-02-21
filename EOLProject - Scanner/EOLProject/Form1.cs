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
        Socket socket;
        //实例化网络通讯检测
        Ping ping = new Ping();

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// load事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            #region 窗体启动时界面设置
            this.groupBox1.Paint += GroupBox_Paint;
            this.groupBox2.Paint += GroupBox_Paint;
            this.groupBox3.Paint += GroupBox_Paint;
            this.groupBox7.Paint += GroupBox_Paint;
            this.groupBox5.Paint += GroupBox_Paint;
            this.groupBox6.Paint += GroupBox_Paint;
            this.dataGridView1.Rows.Add();
            this.dataGridView1.Rows.Add();
            this.dataGridView1.Rows.Add();
            this.dataGridView1.Rows.Add();
            this.dataGridView1.Rows[0].Cells[0].Value = "理论值";
            this.dataGridView1.Rows[0].Cells[1].Value = "5.00-6.00";
            this.dataGridView1.Rows[0].Cells[2].Value = "5.00-6.00";
            this.dataGridView1.Rows[0].Cells[3].Value = "5.00-6.00";
            this.dataGridView1.Rows[0].Cells[4].Value = "5.00-6.00";
            this.dataGridView1.Rows[1].Cells[0].Value = "实际值";
            this.dataGridView1.Rows[2].Cells[0].Value = "";
            this.dataGridView1.Rows[2].Cells[1].Value = "数据5";
            this.dataGridView1.Rows[2].Cells[2].Value = "数据6";
            this.dataGridView1.Rows[2].Cells[3].Value = "数据7";
            this.dataGridView1.Rows[2].Cells[4].Value = "数据8";
            this.dataGridView1.Rows[3].Cells[0].Value = "理论值";
            this.dataGridView1.Rows[3].Cells[1].Value = "5.00-6.00";
            this.dataGridView1.Rows[3].Cells[2].Value = "5.00-6.00";
            this.dataGridView1.Rows[3].Cells[3].Value = "5.00-6.00";
            this.dataGridView1.Rows[3].Cells[4].Value = "5.00-6.00";
            this.dataGridView1.Rows[4].Cells[0].Value = "实际值";
            this.dataGridView1.Rows[1].Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView1.Rows[4].Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView1.Rows[1].Cells[1].Style.Font = new Font("微软雅黑", 72);
            this.dataGridView1.Rows[1].Cells[1].Style.ForeColor = Color.Blue;
            this.dataGridView1.Rows[1].Cells[2].Style.Font = new Font("微软雅黑", 72);
            this.dataGridView1.Rows[1].Cells[2].Style.ForeColor = Color.Blue;
            this.dataGridView1.Rows[1].Cells[3].Style.Font = new Font("微软雅黑", 72);
            this.dataGridView1.Rows[1].Cells[3].Style.ForeColor = Color.Blue;
            this.dataGridView1.Rows[1].Cells[4].Style.Font = new Font("微软雅黑", 72);
            this.dataGridView1.Rows[1].Cells[4].Style.ForeColor = Color.Blue;
            this.dataGridView1.Rows[4].Cells[1].Style.Font = new Font("微软雅黑", 72);
            this.dataGridView1.Rows[4].Cells[1].Style.ForeColor = Color.Blue;
            this.dataGridView1.Rows[4].Cells[2].Style.Font = new Font("微软雅黑", 72);
            this.dataGridView1.Rows[4].Cells[2].Style.ForeColor = Color.Blue;
            this.dataGridView1.Rows[4].Cells[3].Style.Font = new Font("微软雅黑", 72);
            this.dataGridView1.Rows[4].Cells[3].Style.ForeColor = Color.Blue;
            this.dataGridView1.Rows[4].Cells[4].Style.Font = new Font("微软雅黑", 72);
            this.dataGridView1.Rows[4].Cells[4].Style.ForeColor = Color.Blue;
            this.dataGridView1.GridColor = Color.Black;
            this.dataGridView1.EnableHeadersVisualStyles = false;
            this.dataGridView1.Rows[0].Selected = false;
            #endregion

            thread = new Thread(Detection);    //执行程序
            thread.Start();
        }


        /// <summary>
        /// 主程序运行
        /// </summary>
        public void Detection()
        {
            //实例化串行端口
            SerialPort serialPort = new SerialPort();
            //端口名  注:因为使用的是USB转RS232 所以去设备管理器中查看一下虚拟com口的名字
            serialPort.PortName = "COM3";
            //波特率
            serialPort.BaudRate = 9600;
            //奇偶校验
            serialPort.Parity = Parity.None;
            //停止位
            serialPort.StopBits = StopBits.One;
            //数据位
            serialPort.DataBits = 8;

            PingReply testReply = ping.Send("192.168.0.37");    //将连接PLC放在循环外，如果放在循环内部会重复连接PLC（循环内部在判断与PLC通讯状态异常处会重新连接PLC）
            if (testReply.Status == IPStatus.Success)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(iPEndPoint);    //连接PLC
            }

            while (true)
            {
                try
                {
                    #region 判断与扫码枪、PLC的连接是否正常显示不同信息
                    string[] gCOM = SerialPort.GetPortNames();    // 获取设备的所有可用串口
                    bool judge = false;
                    for (int i = 0; i < gCOM.Length; i++)
                    {
                        if (gCOM[i] == serialPort.PortName)    //判断可用的串口中是否有扫码枪的串口
                        {
                            judge = true;
                            break;
                        }
                    }
                    PingReply plcReply = ping.Send("192.168.0.37");    //获取与PLC网络连接状态
                    if (judge == true && plcReply.Status == IPStatus.Success)    //根据判断是否存在扫码枪串口与PLC网络连接状态在界面显示不同的信息
                    {
                        this.pictureBox1.BackColor = Color.Green;
                        this.pictureBox2.BackColor = Color.Green;

                        if (serialPort.IsOpen == false)
                        {
                            serialPort.Open();
                        }
                        int s = this.label15.Text.Length;
                        if (s == 0 || s == 18 || s == 21)
                        {
                            this.label15.Invoke(new Action(() => { this.label15.Text = "请扫取总成条码！"; }));
                        }
                    }
                    else if (judge == false && plcReply.Status == IPStatus.Success)
                    {
                        this.label15.Invoke(new Action(() => { this.label15.Text = "扫码枪连接异常,请检查扫码枪串口连接"; }));
                        this.pictureBox1.BackColor = Color.Red;
                        this.pictureBox2.BackColor = Color.Green;
                    }
                    else if (judge == true && plcReply.Status == IPStatus.TimedOut)
                    {
                        this.label15.Invoke(new Action(() => { this.label15.Text = "PLC连接异常,请检查PLC网络连接"; }));
                        this.pictureBox1.BackColor = Color.Green;
                        this.pictureBox2.BackColor = Color.Red;
                        this.button1.Invoke(new Action(() => { this.button1.Enabled = false; }));
                        this.button4.Invoke(new Action(() => { this.button4.Enabled = true; }));
                        this.textBox1.Invoke(new Action(() => { this.textBox1.Text = ""; }));
                        this.textBox2.Invoke(new Action(() => { this.textBox2.Text = ""; }));
                    }
                    else if (judge == false && plcReply.Status == IPStatus.TimedOut)
                    {
                        this.label15.Invoke(new Action(() => { this.label15.Text = "请检查扫码枪串口连接\n请检查PLC网络连接"; }));
                        this.button1.Invoke(new Action(() => { this.button1.Enabled = false; }));
                        this.pictureBox1.BackColor = Color.Red;
                        this.pictureBox2.BackColor = Color.Red;
                        this.button4.Invoke(new Action(() => { this.button4.Enabled = true; }));
                        this.textBox1.Invoke(new Action(() => { this.textBox1.Text = ""; }));
                        this.textBox2.Invoke(new Action(() => { this.textBox2.Text = ""; }));
                    }
                    #endregion

                    if (judge == true && plcReply.Status == IPStatus.Success)    //判断扫码枪与PLC连接正常
                    {
                        Thread.Sleep(1000);
                        string str = serialPort.ReadExisting().Replace("\r", "").Replace("\n", "");//接收扫码枪获取的总成条码
                        //int extent = str.Length;
                        if (str.Length == 23)    //判断总成条码的长度是否正常
                        {
                            #region 清空界面信息
                            this.button1.Invoke(new Action(() => { this.button1.Enabled = false; }));
                            this.textBox3.Invoke(new Action(() => { this.textBox3.BackColor = Color.White; }));
                            this.textBox4.Invoke(new Action(() => { this.textBox4.BackColor = Color.White; }));
                            this.textBox5.Invoke(new Action(() => { this.textBox5.BackColor = Color.White; }));
                            this.dataGridView1.Rows[1].Cells[1].Value = "";
                            this.dataGridView1.Rows[1].Cells[2].Value = "";
                            this.dataGridView1.Rows[1].Cells[3].Value = "";
                            this.dataGridView1.Rows[1].Cells[4].Value = "";
                            this.dataGridView1.Rows[4].Cells[1].Value = "";
                            this.dataGridView1.Rows[4].Cells[2].Value = "";
                            this.dataGridView1.Rows[4].Cells[3].Value = "";
                            this.dataGridView1.Rows[4].Cells[4].Value = "";
                            #endregion

                            this.textBox1.Invoke(new Action(() => { this.textBox1.Text = str.Substring(0, 4); }));    //界面显示产品编号
                            this.textBox2.Invoke(new Action(() => { this.textBox2.Text = str; }));    //界面显示总成条码
                            str = "";    //将接收总成条码字段清空
                            this.label15.Invoke(new Action(() => { this.label15.Text = "获取总成条码成功，正在获取制造参数信息！"; }));    //界面显示总成条码获取成功信息
                            this.textBox3.Invoke(new Action(() => { this.textBox3.BackColor = Color.Green; }));    //程序运行状态显示
                            if (plcReply.Status == IPStatus.Success)    //判断与PLC通讯是否正常
                            {
                                EOLEntities entities = new EOLEntities();
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
                                this.dataGridView1.Rows[1].Cells[1].Value = list[0];    //界面显示PLC数据
                                this.dataGridView1.Rows[1].Cells[2].Value = list[1];
                                this.dataGridView1.Rows[1].Cells[3].Value = list[2];
                                this.dataGridView1.Rows[1].Cells[4].Value = list[3];
                                this.dataGridView1.Rows[4].Cells[1].Value = list[4];
                                this.button1.Invoke(new Action(() => { this.button1.Enabled = true; }));    //保存数据并打印按钮可以使用
                                this.label15.Invoke(new Action(() => { this.label15.Text = "制造参数获取完成，请保存数据并打印！"; }));    //界面显示制造参数获取成功信息
                                this.textBox4.Invoke(new Action(() => { this.textBox4.BackColor = Color.Green; }));    //程序运行状态显示
                            }
                        }
                        else if (str.Length != 0 && str.Length != 23)    //判断总成条码长度异常
                        {
                            #region 清空界面信息
                            this.textBox1.Invoke(new Action(() => { this.textBox1.Text = ""; }));
                            this.button1.Invoke(new Action(() => { this.button1.Enabled = false; }));
                            this.textBox3.Invoke(new Action(() => { this.textBox3.BackColor = Color.White; }));
                            this.textBox4.Invoke(new Action(() => { this.textBox4.BackColor = Color.White; }));
                            this.textBox5.Invoke(new Action(() => { this.textBox5.BackColor = Color.White; }));
                            this.dataGridView1.Rows[1].Cells[1].Value = "";
                            this.dataGridView1.Rows[1].Cells[2].Value = "";
                            this.dataGridView1.Rows[1].Cells[3].Value = "";
                            this.dataGridView1.Rows[1].Cells[4].Value = "";
                            this.dataGridView1.Rows[4].Cells[1].Value = "";
                            this.dataGridView1.Rows[4].Cells[2].Value = "";
                            this.dataGridView1.Rows[4].Cells[3].Value = "";
                            this.dataGridView1.Rows[4].Cells[4].Value = "";
                            #endregion
                            this.label15.Invoke(new Action(() => { this.label15.Text = "总成条码有误，请重新扫码！"; }));    //界面显示总成条码有误信息
                            this.textBox2.Invoke(new Action(() => { this.textBox2.Text = str; }));    //界面显示异常总成条码
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Info(ex);
                    socket.Close();
                    Thread.Sleep(3000);
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(iPEndPoint);    //连接PLC
                }
            }
        }



        /// <summary>
        /// 查询总成条码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            Inquire inquire = new Inquire();
            inquire.Show();
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
            save.Data1 = this.dataGridView1.Rows[1].Cells[1].Value.ToString();
            save.Data2 = this.dataGridView1.Rows[1].Cells[2].Value.ToString();
            save.Data3 = this.dataGridView1.Rows[1].Cells[3].Value.ToString();
            save.Data4 = this.dataGridView1.Rows[1].Cells[4].Value.ToString();
            save.Data5 = this.dataGridView1.Rows[3].Cells[1].Value.ToString();
            save.Data6 = this.dataGridView1.Rows[3].Cells[2].Value.ToString();
            save.Data7 = this.dataGridView1.Rows[3].Cells[3].Value.ToString();
            save.Data8 = this.dataGridView1.Rows[3].Cells[4].Value.ToString();
            save.Date = DateTime.Now;
            entities.Save.Add(save);
            entities.SaveChanges();
            this.label15.Invoke(new Action(() => { this.label15.Text = "数据保存完成，正在打印条码！"; }));
            this.textBox5.Invoke(new Action(() => { this.textBox5.BackColor = Color.Green; }));
        }

        /// <summary>
        /// 退出按钮，退出程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        /// <summary>
        /// 复位按钮，重新启动程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            this.textBox1.Invoke(new Action(() => { this.textBox1.Text = ""; }));
            this.textBox2.Invoke(new Action(() => { this.textBox2.Text = ""; }));
            this.button1.Invoke(new Action(() => { this.button1.Enabled = false; }));
            this.textBox3.Invoke(new Action(() => { this.textBox3.BackColor = Color.White; }));
            this.textBox4.Invoke(new Action(() => { this.textBox4.BackColor = Color.White; }));
            this.textBox5.Invoke(new Action(() => { this.textBox5.BackColor = Color.White; }));
            this.dataGridView1.Rows[1].Cells[1].Value = "";
            this.dataGridView1.Rows[1].Cells[2].Value = "";
            this.dataGridView1.Rows[1].Cells[3].Value = "";
            this.dataGridView1.Rows[1].Cells[4].Value = "";
            this.dataGridView1.Rows[4].Cells[1].Value = "";
            this.dataGridView1.Rows[4].Cells[2].Value = "";
            this.dataGridView1.Rows[4].Cells[3].Value = "";
            this.dataGridView1.Rows[4].Cells[4].Value = "";
            this.label15.Invoke(new Action(() => { this.label15.Text = "复位成功！"; }));
        }

        #region GroupBox边框重绘
        void GroupBox_Paint(object sender, PaintEventArgs e)
        {
            GroupBox gBox = (GroupBox)sender;

            e.Graphics.Clear(gBox.BackColor);
            e.Graphics.DrawString(gBox.Text, gBox.Font, Brushes.Black, 10, 1);
            var vSize = e.Graphics.MeasureString(gBox.Text, gBox.Font);
            e.Graphics.DrawLine(Pens.Gray, 1, vSize.Height / 2, 8, vSize.Height / 2);
            e.Graphics.DrawLine(Pens.Gray, vSize.Width + 8, vSize.Height / 2, gBox.Width - 2, vSize.Height / 2);
            e.Graphics.DrawLine(Pens.Gray, 1, vSize.Height / 2, 1, gBox.Height - 2);
            e.Graphics.DrawLine(Pens.Gray, 1, gBox.Height - 2, gBox.Width - 2, gBox.Height - 2);
            e.Graphics.DrawLine(Pens.Gray, gBox.Width - 2, vSize.Height / 2, gBox.Width - 2, gBox.Height - 2);
        }

        #endregion

    }
}
