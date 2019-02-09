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
using System.Reflection;

namespace RemotePC8801
{
    /// <summary>
    /// PageDiskInfo.xaml の相互作用ロジック
    /// </summary>
    public partial class PageDiskInfo : Page
    {
        public PageDiskInfo()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await update();
        }

        private DiskInfo info = null;

        private async Task update()
        {
            using (var lck = new LockForm())
            {
                ListBoxDiskInfos.Items.Clear();
                var driveNo = Util.GetSelectedDrive();
                info = await Util.GetDiskInf(driveNo);
                if (info == null) return;
                foreach (var item in info.GetType().GetFields())
                {
                    var v = item.GetValue(info);
                    var s = item.Name + "=" + v.ToString();
                    ListBoxDiskInfos.Items.Add(s);
                }
            }
        }

        private async void ButtonReload_Click(object sender, RoutedEventArgs e)
        {
            await update();
        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            using (var lck = new LockForm())
            {
                if (info == null) return;
                var sb = new StringBuilder();
                foreach (var item in info.GetType().GetFields())
                {
                    sb.AppendFormat("{0}={1}\r\n", item.Name, item.GetValue(info));
                }
                Clipboard.Clear();
                Clipboard.SetText(sb.ToString());
            }
        }
    }
}
