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

namespace RemotePC8801
{
    /// <summary>
    /// PageSector.xaml の相互作用ロジック
    /// </summary>
    public partial class PageSector : Page
    {
        public PageSector()
        {
            InitializeComponent();
        }

        private const string Sixteen = "0123456789ABCDEF";
        private TextBox[,] textBlocks;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 16; i++)
            {
                var tb = new TextBlock(new Run(Sixteen[i].ToString()));
                Grid.SetColumn(tb, i + 1);
                Grid.SetRow(tb, 0);
                MainGrid.Children.Add(tb);
                var tb2 = new TextBlock(new Run(Sixteen[i].ToString()));
                Grid.SetColumn(tb2, 0);
                Grid.SetRow(tb2, i + 1);
                MainGrid.Children.Add(tb2);
            }

            textBlocks = new TextBox[16, 16];
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    var tb = new TextBox();
                    tb.Text = "XX";
                    tb.IsReadOnly = true;
                    tb.Name = $"table{Sixteen[x]}{Sixteen[y]}";
                    Grid.SetColumn(tb, x + 1);
                    Grid.SetRow(tb, y + 1);
                    MainGrid.Children.Add(tb);
                    textBlocks[x, y] = tb;
                }
            }
        }

        private void setAllXX()
        {
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    textBlocks[x,y].Text = "XX";
                }
            }
        }
        private async void ButtonRead_Click(object sender, RoutedEventArgs e)
        {
            setAllXX();
            var diskinfo = await Util.GetDiskInf(Util.GetSelectedDrive());
            int.TryParse(TextBoxTrack.Text, out int track);
            int.TryParse(TextBoxSector.Text, out int sector);

            var valid = diskinfo.VaridateParameters(ComboBoxDrives.SelectedIndex + 1, ComboBoxSurface.SelectedIndex, track, sector, (message) =>
            {
                Util.ErrorPopup(message);
            });
            if (!valid) return;
            Util.MyMainWindow.BlockReadRequest(256);
            if (await Util.SendCommandAsyncAndErrorHandle($"FIELD #0,128 as a$, 128 as b$:DUMMY$=DSKI$({ComboBoxDrives.SelectedIndex + 1},{ComboBoxSurface.SelectedIndex},{track},{sector})"+ Const.SectorReadStatements, true)) return;
            var bytes = Util.MyMainWindow.blockReadBuffer;
            int x = 0, y = 0;
            foreach (var item in bytes)
            {
                if (y > 15)
                {
                    MessageBox.Show("Sector Data too large: " + bytes.Count().ToString());
                    break;
                }
                textBlocks[x, y].Text = item.ToString("X2");
                x++;
                if( x> 15)
                {
                    x = 0;
                    y++;
                }
            }
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonWrite_Click(object sender, RoutedEventArgs e)
        {

        }

        private void setEnable(bool newEnableState)
        {
            ButtonWrite.IsEnabled = newEnableState;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    textBlocks[x, y].IsEnabled = newEnableState;
                }
            }
        }
                private void CheckBoxEnableEdit_Checked(object sender, RoutedEventArgs e)
        {
            setEnable(true);
        }

        private void CheckBoxEnableEdit_Unchecked(object sender, RoutedEventArgs e)
        {
            setEnable(false);
        }

    }
}
