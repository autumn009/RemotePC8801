using System;
using System.Collections.Generic;
using System.IO;
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

namespace RemotePC8801
{
    /// <summary>
    /// PageStart.xaml の相互作用ロジック
    /// </summary>
    public partial class PageStart : Page
    {
        public PageStart()
        {
            InitializeComponent();
        }

        private async void AutoSave_Click(object sender, RoutedEventArgs e)
        {
            const string data = @"10 rem auto start program for Remove PC-8801 client
20 console 0,25,1,1
30 cls 1
40 print ""TERM MODE""
50 color 2:print ""SHIFT+STOP"";:color 7:print ""to quit TERM mode.""
60 term ""N81NNF"",,8192
";

            using (var diable = new LockForm())
            {
                if (await Util.SendCommandAsyncAndErrorHandle("open \"" + Util.GetSelectedDrive() + ":rem88\" for output as #1 ")) return;
                var reader = new StringReader(data);
                for (; ; )
                {
                    var s = reader.ReadLine();
                    if (s == null) break;
                    if (await Util.SendCommandAsyncAndErrorHandle("print #1,\"" + s.Replace("\"", "\"+chr$(34)+\"") + "\"")) return;
                }
                if (await Util.SendCommandAsyncAndErrorHandle("close #1")) return;
            }
        }
    }
}
