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
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var topRow = new TableRow();
            topRow.Cells.Add(new TableCell(new Paragraph(new Run("⌐"))));
            MyTableRowGroup.Rows.Add(topRow);
            foreach (var item in Sixteen) topRow.Cells.Add(new TableCell(new Paragraph(new Run(item.ToString()))));
            foreach (var item in Sixteen)
            {
                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.ToString()))));
                for (int i = 0; i < 16; i++)
                {
                    var run = new Run("XY");
                    //run.Name = $"table{i.ToString()}{item}";
                    row.Cells.Add(new TableCell(new Paragraph(run)));
                }
                MyTableRowGroup.Rows.Add(row);
            }
        }
    }
}
