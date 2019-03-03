using Microsoft.Win32;
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
using System.IO;

namespace RemotePC8801
{
    /// <summary>
    /// PageImage.xaml の相互作用ロジック
    /// </summary>
    public partial class PageImage : Page
    {
        public PageImage()
        {
            InitializeComponent();
            ProgressBarDefault.Value = 0;
            setStatus("");
            TextBoxDirectory.Text = Properties.Settings.Default.ImageDir;
        }

        private void ImageWrite_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not supported yet");
        }

        private void setStatus(string s)
        {
            TextBlockStatus.Text = s;
        }
        private void setStatus(int drive, DiskFormats format, int sur,int trk, int sec)
        {
            TextBlockStatus.Text = string.Format("DRIVE:{0} FORMAT:{1} SURFACE:{2} TRACK:{3:D2} SECTOR:{4:D2}", drive, format, sur, trk, sec);
        }

        private async void ImageRead_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var lockf = new LockForm())
                {
                    var dialog = new SaveFileDialog();
                    dialog.InitialDirectory = TextBoxDirectory.Text;
                    dialog.Title = "Save Image";
                    dialog.Filter = "Image File(*.img)|*.img|All Files(*.*)|*.*";
                    if (dialog.ShowDialog() != true) return;
                    var diskinfo = await Util.GetDiskInf(Util.GetSelectedDrive());
                    var format = Util.GetDiskFormatOverride();
                    ProgressBarDefault.Maximum = (diskinfo.Surfaces + 1) * (diskinfo.MaxTrackNo + 1) * diskinfo.SectorsInTrack;
                    ProgressBarDefault.Value = 0;
                    using (var outputStream = File.OpenWrite(dialog.FileName))
                    {
                        for (int surface = 0; surface < diskinfo.Surfaces + 1; surface++)
                        {
                            for (int track = 0; track < diskinfo.MaxTrackNo + 1; track++)
                            {
                                for (int sector = 1; sector < diskinfo.SectorsInTrack + 1; sector++)
                                {
                                    ProgressBarDefault.Value++;
                                    setStatus(diskinfo.DriveNo, format, surface, track, sector);
                                    var array = await Util.SectorRead(diskinfo.DriveNo, surface, track, sector);
                                    if (array == null) return;
                                    outputStream.Write(array, 0, array.Length);
                                }
                            }
                        }
                    }
                    MessageBox.Show("Done Successfully: " + dialog.FileName);
                }
            }
            finally
            {
                setStatus("");
                ProgressBarDefault.Value = 0;
            }
        }
        private void DirectorySelect_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "フォルダーを選択してください。";
            dlg.SelectedPath = TextBoxDirectory.Text;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBoxDirectory.Text = dlg.SelectedPath;
                Properties.Settings.Default.ImageDir = TextBoxDirectory.Text;
                Properties.Settings.Default.Save();
            }
        }
    }
}
