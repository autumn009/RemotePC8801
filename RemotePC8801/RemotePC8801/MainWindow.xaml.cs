                                                        using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.IO;

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
            _navi = this.MainFrame.NavigationService;
            setVersionState(null);
        }

        private Uri pageStart = new Uri("PageStart.xaml", UriKind.Relative);
        private Uri pageDirectCommand = new Uri("PageDirectCommand.xaml", UriKind.Relative);
        private Uri pageSector = new Uri("PageSector.xaml", UriKind.Relative);
        private Uri pageDiskInfo = new Uri("PageDiskInfo.xaml", UriKind.Relative);

        private NavigationService _navi;

        private static readonly string confirmationString = new string(Enumerable.Range(0x20, 256 - 0x20).Where(c => c != '"').Select(c => (char)c).ToArray());

        public enum ResultStatusMarker
        {
            ShowResult = -1,
            CommandEnd = -2,
            Timeout = -3,
            NotOpen = -4,
            Confirmation = -999
        };

        private StringBuilder lastLineBuffer = new StringBuilder();
        private StringBuilder currentLineBuffer = new StringBuilder();
        private string statementReaultString = null;
        public string StatementResultString => statementReaultString;
        private AutoResetEvent waiter = new AutoResetEvent(false);
        private ResultStatusMarker result;

        public void AppendLog(string s)
        {
            TextBoxLog.Text += s;
            if (s.Last() == '\n')
            {
                if (TextBoxLog.LineCount > 50)
                {
                    var index = TextBoxLog.Text.IndexOf("\r\n");
                    if (index >= 0) TextBoxLog.Text = TextBoxLog.Text.Substring(index + 2);
                }
                TextBoxLog.ScrollToEnd();
            }
        }

        private async Task appendLogFromWorkerThread(string s)
        {
            await this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(
                () =>
                {
                    AppendLog(s);
                })
            );
        }

        private async Task appendLogFromWorkerThread(char ch)
        {
            await this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(
                () =>
                {
                    if ((ch >= 32 && ch < 255) || ch == 10 || ch == 13)
                    {
                        AppendLog(ch.ToString());
                    }
                    else
                    {
                        AppendLog($"[{((int)ch).ToString("X2")}]");
                    }
                })
            );
        }

        private SerialPort port;    // current COM port
        private Task portWatcher;    // another task

        public bool IsPortOpen => port != null;

        private async void watcherTask()
        {
            try
            {
                for (; ; )
                {
                    if (port == null) return;
                    int ch = port.ReadChar();
                    //System.Diagnostics.Debug.Write("!");

                    if (ch == -1) return;
                    await appendLogFromWorkerThread((char)ch);
                    if (ch == 13)
                    {
                        // do nothing
                    }
                    else if (ch == 10)
                    {
                        lastLineBuffer = currentLineBuffer;
                        currentLineBuffer = new StringBuilder();
                        if (lastLineBuffer.ToString().StartsWith(":::")) // result marker
                        {
                            result = ResultStatusMarker.ShowResult;
                            waiter.Set();
                        }
                        else if (lastLineBuffer.ToString() == "###")    // statement end marker
                        {
                            result = ResultStatusMarker.CommandEnd;
                            waiter.Set();
                        }
                        else if (lastLineBuffer.ToString().StartsWith("%%%"))    // statement result marker
                        {
                            statementReaultString = lastLineBuffer.ToString().Substring(3);
                        }
                        else if (lastLineBuffer.ToString() == confirmationString)    // confirmationString
                        {
                            result = ResultStatusMarker.Confirmation;
                            waiter.Set();
                        }
                    }
                    else
                    {
                        currentLineBuffer.Append((char)ch);
                    }
                }
            }
            catch (IOException) when (port == null)
            {
                // ignore excepton
            }
            catch (Exception e)
            {
                await appendLogFromWorkerThread(e.ToString());
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
                port.Handshake = Handshake.RequestToSendXOnXOff;
                port.Encoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
                port.WriteTimeout = defaultTimeoutMilliSecond;
                portWatcher = Task.Run((Action)watcherTask);
            }
            catch (Exception e)
            {
                port = null;
                AppendLog(e.ToString());
            }
            updateOpenCloseStatus();
        }

        private void portClose()
        {
            if (port == null) return;
            var p = port;
            port = null;
            p.Close();
            p.Dispose();
            updateOpenCloseStatus();
            setVersionState(null);
        }

        public bool portOutput(string s)
        {
            try
            {
                port.Write(s);
                AppendLog(s + "\n");
            }
            catch (TimeoutException e)
            {
                AppendLog("TIMEOUT");
                return true;
            }
            catch (Exception e)
            {
                AppendLog(e.ToString());
            }
            return false;
        }

        private void setEnables(bool isOpen)
        {
            Util.EnumVisual(this, (visual) =>
            {
                var button = visual as Button;
                if (button == null) return;
                button.IsEnabled = isOpen;
            });
            ButtonPortOpen.IsEnabled = !isOpen;
            ButtonPortClose.IsEnabled = isOpen;
        }

        private void updateOpenCloseStatus()
        {
            bool isOpen = port != null;
            TextBlockOpenClose.Text = isOpen ? "OPEN" : "CLOSE";
            TextBlockOpenClose.Foreground = new SolidColorBrush(isOpen ? Colors.Green : Colors.Red);
            BorderOpenClose.BorderBrush = TextBlockOpenClose.Foreground;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyProgress.Visibility = Visibility.Hidden;
            setEnables(false);
            updateOpenCloseStatus();
            _navi.Navigate(pageStart);
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            portClose();
        }

        private void ButtonPortClose_Click(object sender, RoutedEventArgs e)
        {
            using (var lck = new LockForm())
            {
                portClose();
                setEnables(false);
            }
        }

        private void setVersionState(string text)
        {
            TextBlockVersionStatus.Text = (text!=null)?text:"NOT READY";
            TextBlockVersionStatus.Foreground = new SolidColorBrush((text != null)?Colors.Green:Colors.Red);
            BorderVersionStatus.BorderBrush = TextBlockVersionStatus.Foreground;
        }

        private async void ButtonPortOpen_Click(object sender, RoutedEventArgs e)
        {
            using (var lck = new LockForm())
            {
                portOpen();
                var r = await confirmation();
                if (r == ResultStatusMarker.Confirmation)
                {
                    AppendLog("Communication confirmed. Ready.\r\n");
                    setEnables(true);
                    var r2 = await getN88Version();
                    if (!r2)
                    {
                        AppendLog($"Version {statementReaultString}\r\n");
                        setVersionState("N88-BASIC Version " + statementReaultString + " READY");
                    }
                }
                else
                {
                    AppendLog($"Communication Failed.[{ Util.GetErrorString(r) }] Please verify your environment. Closing Port\r\n");
                    portClose();
                    setEnables(false);
                }
            }
        }
        private int getTargetDrive() => ComboDriveSelect.SelectedIndex + 1;

        public async Task<ResultStatusMarker> SendCommandAsync(string statement, bool forceHandshake = false)
        {
            if (port == null) return ResultStatusMarker.NotOpen;
            var append = "";
            if (forceHandshake) append = ":PRINT \"###\"";
            var r = portOutput("\x1b<" + Const.ClearErrStatement + ":" + statement + append + "\r");
            if (r) return ResultStatusMarker.Timeout;
            if (forceHandshake) if (await waitResult() == ResultStatusMarker.Timeout) return ResultStatusMarker.Timeout;
            r = portOutput("\x1b<print \":::\";ERR\r");
            if (r) return ResultStatusMarker.Timeout;
            return await waitResult();
        }

        private async Task<ResultStatusMarker> confirmation()
        {
            if (port == null) return ResultStatusMarker.NotOpen;
            var r = portOutput("\x1b<print \"" + confirmationString + "\"\r");
            if (r) return ResultStatusMarker.Timeout;
            return await waitResult();
        }

        private async Task<bool> getN88Version()
        {
            if (port == null) return false;
            return await Util.SendCommandAsyncAndErrorHandle("print \"%%%\"+" + Const.GettingVersionExpression + "\r");
        }
        

        private const int defaultTimeoutMilliSecond = 30000;

        private async Task<ResultStatusMarker> waitResult(int? timeout = null)
        {
            return await Task.Run(async () =>
            {
                waiter.Reset();
                if (timeout == null) timeout = defaultTimeoutMilliSecond;
                var sucessed = waiter.WaitOne((int)timeout);
                if (!sucessed)
                {
                    await appendLogFromWorkerThread("Communication Timeout");
                    Dispatcher.Invoke(() =>
                    {
                        portClose();
                        setEnables(false);
                    });
                    await appendLogFromWorkerThread("Port closed");
                    return ResultStatusMarker.Timeout;
                }
                var s = lastLineBuffer.ToString();
                if (s.Length <= 3 || result != ResultStatusMarker.ShowResult) return result;
                int.TryParse(s.Substring(3), out var r);
                // 255 by "error 255". It means "no error".
                //if (r == 255) r = 0;
                return (ResultStatusMarker)r;
            });
        }

        private async void ButtonFiles_Click(object sender, RoutedEventArgs e)
        {
            using (var lck = new LockForm())
            {
                if (port == null) return;
                await SendCommandAsync($"files {getTargetDrive()}");
            }
        }

        private void ButtonSectors_Click(object sender, RoutedEventArgs e)
        {
            using (var lck = new LockForm())
            {
                _navi.Navigate(pageSector);
            }
        }

        private void ButtonDirect_Click(object sender, RoutedEventArgs e)
        {
            using (var lck = new LockForm())
            {
                _navi.Navigate(pageDirectCommand);
            }
        }

        private async void ButtonDebug_Click(object sender, RoutedEventArgs e)
        {
            using (var lck = new LockForm())
            {
                await Util.SendCommandAsyncAndErrorHandle("ABC:", true);

#if false
            await Task.Run(async () =>
            {
                for (int i = 0; i < 20; i++)
                {
                    Dispatcher.Invoke(() =>
                    {
                        portOutput("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

                    });
                    await Task.Delay(1000);
                }
            });
#endif
            }
        }

        private void ButtonDiskInfo_Click(object sender, RoutedEventArgs e)
        {
            using (var lck = new LockForm())
            {
                _navi.Navigate(pageDiskInfo);
            }
        }
    }
}
