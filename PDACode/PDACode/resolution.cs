using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace PDACode
{
    public class resolution
    {  //// 将VGA画面转变成QVGA
        ////
        ////    CALL方法：向各Form的Load处理追加以下的状态
        ////
        ////    if (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width == 240)
        ////    {
        ////        resolution.ScreenSize.VGAtoQVGA(this);
        ////    }
        public static void VGAtoQVGA(Control top)
        {
            //// 调整窗体内部件的尺寸
            foreach (Control item in top.Controls)
            {
                //// 纵横坐标，宽度&高度
                item.Left = (int)(item.Left * 0.5);
                item.Top = (int)(item.Top * 0.5);
                item.Width = (int)(item.Width * 0.5);
                item.Height = (int)(item.Height * 0.5);

                //// 字体尺寸
                try
                {
                    item.Font = new Font(item.Font.Name, (float)(item.Font.Size * 0.5), item.Font.Style);
                }
                catch (NotSupportedException)
                {
                    /* Font not implemented */
                }

            }

            //// // 调整窗体尺寸
            top.Left = (int)(top.Left * 0.5);
            top.Top = (int)(top.Top * 0.5);
            top.Width = (int)(top.Width * 0.5);
            top.Height = (int)(top.Height * 0.5);

            top.Font = new Font(top.Font.Name, (float)(top.Font.Size * 0.5), top.Font.Style);
        }

        //// 将QVGA画面转变成VGA
        ////
        ////    CALL方法：向各Form的Load处理追加以下的状态
        ////
        ////    if (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width == 480)
        ////    {
        ////        resolution.ScreenSize.VGAtoQVGA(this);
        ////    }
        public static void QVGAtoVGA(Control top)
        {
            //// 调整窗体内部件的尺寸
            foreach (Control item in top.Controls)
            {
                //// 纵横坐标，宽度&高度
                item.Left = item.Left * 2;
                item.Top = item.Top * 2;
                item.Width = item.Width * 2;
                item.Height = item.Height * 2;

                //// 字体尺寸
                try
                {
                    item.Font = new Font(item.Font.Name, item.Font.Size * 2, item.Font.Style);
                }
                catch (NotSupportedException)
                {
                    /* Font not implemented */
                }

            }

            //// 调整窗体尺寸
            top.Left = top.Left * 2;
            top.Top = top.Top * 2;
            top.Width = top.Width * 2;
            top.Height = top.Height * 2;

            top.Font = new Font(top.Font.Name, top.Font.Size * 2, top.Font.Style);
        }
    }

}
