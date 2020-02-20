using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Bt;
using System.Threading;
using Bt.SysLib;

namespace PDACode
{
    public partial class Form1 : Form
    {
        MsgWindow MsgWin;								// 消息窗口
        public static Int32 ScanMode = 0;				// 扫描模式(1:逐个 2:全体)
        public static String szReadin = "";             //接受PDA扫码获取的条码数据
        Thread thread;
        WebReference.WebService1 webService = new PDACode.WebReference.WebService1();

        public Form1()
        {
            InitializeComponent(); 
            this.MsgWin = new MsgWindow();		// 生成消息窗口
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width == 240)
            {
                resolution.VGAtoQVGA(this);
            }
            thread = new Thread(Test);//执行上传数据方法
            thread.Start();
        }

        #region PDA扫码获取条码数据
        private System.Windows.Forms.Control GetFocusedControl(System.Windows.Forms.Control parent)
        {
            if (parent.Focused)
            {
                return parent;
            }

            foreach (System.Windows.Forms.Control ctrl in parent.Controls)
            {
                System.Windows.Forms.Control focusedControl = GetFocusedControl(ctrl);
                if (focusedControl != null)
                {
                    return focusedControl;
                }
            }
            return null;
        }

        public static bool ScanDisposable()
        {
            Int32 ret = 0;
            //String disp = "";

            try
            {
                // 扫描模式＝设定为「一次性」
                ScanMode = 2;

                ret = Bt.ScanLib.Control.btScanEnable();
                if (ret != LibDef.BT_OK)
                {
                    //disp = "btScanEnable error ret[" + ret + "]";
                    //MessageBox.Show(disp, "错误");
                    return false;
                }

                ret = Bt.ScanLib.Control.btScanSoftTrigger(1);
                if (ret != LibDef.BT_OK)
                {
                    //disp = "btScanSoftTrigger error ret[" + ret + "]";
                    //MessageBox.Show(disp, "错误");
                    return false;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("错误9");
            }
            return true;
        }


        //---------------------------------------------------------------------------------
        // MessageWindow类
        //---------------------------------------------------------------------------------
        public class MsgWindow : Microsoft.WindowsCE.Forms.MessageWindow
        {
            public MsgWindow()
            {
            }

            protected override void WndProc(ref Microsoft.WindowsCE.Forms.Message msg)
            {
                switch (msg.Msg)
                {
                    case (Int32)LibDef.WM_BT_SCAN:
                        // 读取成功的场合
                        if (msg.WParam.ToInt32() == (Int32)LibDef.BTMSG_WPARAM.WP_SCN_SUCCESS)
                        {
                            if (ScanMode == 2)
                            {
                                // 读取(一次性)
                                ScanData_ikkatu();
                            }
                        }
                        break;
                }
                base.WndProc(ref msg);
            }



            /********************************************************************************
             * 功能 ：一次性取得读取到的条码数据。
             * API  ：btScanGetStringSize, btScanGetString
            ********************************************************************************/
            public static void ScanData_ikkatu()
            {

                Int32 ret = 0;
                String disp = "";

                Byte[] codedataGet;
                String strCodedata = "";
                Int32 codeLen = 0;
                UInt16 symbolGet = 0;

                try
                {
                    //-----------------------------------------------------------
                    // 读取（一次性）
                    //-----------------------------------------------------------
                    codeLen = Bt.ScanLib.Control.btScanGetStringSize();
                    if (codeLen <= 0)
                    {
                        disp = "扫描获取字符长度错误[" + codeLen + "]";
                        ShowMessageBoxTimeout(disp, "错误", MessageBoxButtons.OK, 3000);
                        goto L_END;
                    }
                    codedataGet = new Byte[codeLen];

                    ret = Bt.ScanLib.Control.btScanGetString(codedataGet, ref symbolGet);
                    if (ret != LibDef.BT_OK)
                    {
                        disp = "扫描获取字符[" + ret + "]";
                        ShowMessageBoxTimeout(disp, "错误", MessageBoxButtons.OK, 3000);
                        goto L_END;
                    }
                    strCodedata = System.Text.Encoding.UTF8.GetString(codedataGet, 0, codeLen);
                    szReadin = strCodedata;

                L_END:

                    ret = Bt.ScanLib.Control.btScanDisable();
                    if (ret != LibDef.BT_OK)
                    {
                        disp = "扫描丢失[" + ret + "]";
                        ShowMessageBoxTimeout(disp, "错误", MessageBoxButtons.OK, 3000);
                    }


                }
                catch (Exception)
                {
                    MessageBox.Show("错误10");
                }
            }

            public static void ShowMessageBoxTimeout(string text, string caption, MessageBoxButtons buttons, int timeout)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(CloseMessageBox), new CloseState(caption, timeout));
                MessageBox.Show(text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
            }
            private static void CloseMessageBox(object state)
            {
                CloseState closeState = state as CloseState;
                Thread.Sleep(closeState.Timeout);
                IntPtr dlg = DataBaseStruct.FindWindow(null, closeState.Caption);
                if (dlg != IntPtr.Zero)
                {
                    IntPtr result;
                    DataBaseStruct.EndDialog(dlg, out result);
                }
            }

            private class CloseState
            {
                private int _Timeout;

                public int Timeout
                {
                    get
                    {
                        return _Timeout;
                    }
                }

                private string _Caption;

                public string Caption
                {
                    get
                    {
                        return _Caption;
                    }
                }

                public CloseState(string caption, int timeout)
                {
                    _Timeout = timeout;
                    _Caption = caption;
                }
            }

            public static void ScanErrorBuzzer()
            {
                Int32 ret = 0;
                String disp = "";

                LibDef.BT_BUZZER_PARAM stBuzzerSet = new LibDef.BT_BUZZER_PARAM();			// 蜂鸣器的各类控制参数(Set)
                stBuzzerSet.dwOn = 100;		//发声时间[ms] （1～5000）
                stBuzzerSet.dwOff = 100;		//停止时间[ms] （0～5000）
                stBuzzerSet.dwCount = 2;	// 发声次数[回] （0～100）
                stBuzzerSet.bTone = 1;		// 音阶 （1～16）
                stBuzzerSet.bVolume = 3;	// 蜂鸣器音量 （1～3）

                try
                {
                    // btBuzzer 发声
                    ret = Device.btBuzzer(1, stBuzzerSet);
                    if (ret != LibDef.BT_OK)
                    {
                        disp = "蜂鸣器错误[" + ret + "]";
                        MessageBox.Show(disp, "错误");
                        return;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("错误11");
                }
            }

        }
        #endregion

        #region 键盘事件
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                System.Windows.Forms.Control control = sender as System.Windows.Forms.Control;					// Form1
                System.Windows.Forms.Control focusedControl = GetFocusedControl(control);	// 焦点控制
                int iTabIndex = focusedControl.TabIndex;//按键的索引
                Int32 ret = 0;
                String disp = "";
                UInt32 nowkeyGet = 0;
                UInt32 refkeySet = 0;
                refkeySet = LibDef.BT_KEY_CTRG;
                ret = Device.btKeySense(refkeySet, ref nowkeyGet);

                if (ret != LibDef.BT_OK)
                {
                    disp = "SCAN键取值错误[" + ret + "]";
                    MessageBox.Show(disp, "错误");
                    return;
                }
                if (nowkeyGet == LibDef.BT_KEY_CTRG)//当键盘SCAN按键被点击会进行扫码
                {
                    ScanDisposable();//获取条码数据
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }
        #endregion

        #region 循环判断接受条码数据字段的长度是否为0，不为0则将条码数据通过调用WebService接口上传至数据库，条码数据字段赋值为空
        public void Test()
        {
            while (true)
            {
                try
                {
                    if (szReadin.Length != 0)//判断条码数据的字段长度是否为0
                    {
                        webService.GetCode(szReadin);//调用webService将条码数据上传至数据库
                        szReadin = "";//条码数据字段赋值为空
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
        }
        #endregion
    }
}
