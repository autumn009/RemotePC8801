using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;

namespace RemotePC8801
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyProgress.Visibility = Visibility.Hidden;
        }

        private void appendLog(string s)
        {
            TextBoxLog.Text += s;
        }

        private async void appendLog(char ch)
        {
            await this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(
                () =>
                {
                    if (ch >= 32 && ch < 255)
                    {
                        TextBoxLog.Text += new string(ch, 1);
                    }
                    else
                    {
                        TextBoxLog.Text += $"[{((int)ch).ToString("X2")}]";
                    }
                })
            );
        }

        private SerialPort port;    // current COM port
        private Task portWatcher;    // another task

        private void watcherTask()
        {
            try
            {
                for (; ; )
                {
                    if (port == null) return;
                    int ch = port.ReadChar();
                    if (ch == -1) return;
                    appendLog((char)ch);
                }
            }
            catch (Exception e)
            {
                //appendLog(e.ToString());
            }
        }

        private void portOpen()
        {
            try
            {
                //　TBW
                port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
                port.Open();
                port.DtrEnable = true;
                port.RtsEnable = true;
                portWatcher = Task.Run((Action)watcherTask);
            }
            catch (Exception e)
            {
                port = null;
                appendLog(e.ToString());
            }
        }

        private void portClose()
        {
            if (port == null) return;
            port.Close();
            port.Dispose();
        }

        private void portOutput(string s)
        {
            try
            {
                port.Write(s);
            }
            catch (Exception e)
            {
                appendLog(e.ToString());
            }
        }

        private void ButtonManualSend_Click(object sender, RoutedEventArgs e)
        {
            portOutput("\x1b<" + TextBoxManualCommand.Text + "\r");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            portClose();
        }

        private void ButtonPortClose_Click(object sender, RoutedEventArgs e)
        {
            portClose();
        }

        private void ButtonPortOpen_Click(object sender, RoutedEventArgs e)
        {
            portOpen();
        }
    }
}
