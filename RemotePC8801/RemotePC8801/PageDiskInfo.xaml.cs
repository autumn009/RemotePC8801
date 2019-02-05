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
            var driveNo = Util.GetSelectedDrive();
            var info = await Util.GetDiskInf(driveNo);
            foreach (var item in info.GetType().GetFields())
            {
                var v = item.GetValue(info);
                var s = item.Name + "=" + v.ToString();
                ListBoxDiskInfos.Items.Add(s);
            }
        }
    }
}
