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
            CheckBoxAutoName.IsChecked = Properties.Settings.Default.ImageAutoFileName;
        }

        private void ImageWrite_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not supported yet");
        }

        private void setStatus(string s)
        {
            TextBlockStatus.Text = s;
        }
        private void setStatus(int drive, DiskFormats format, int sur, int trk, int sec)
        {
            TextBlockStatus.Text = string.Format("DRIVE:{0} FORMAT:{1} SURFACE:{2} TRACK:{3:D2} SECTOR:{4:D2}", drive, format, sur, trk, sec);
        }

        private async void ImageRead_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var lockf = new LockForm())
                {
                    string filename;
                    if (Properties.Settings.Default.ImageAutoFileName)
                    {
                        filename = System.IO.Path.Combine(TextBoxDirectory.Text, DateTimeOffset.Now.ToString("yyyyMMddHHmmss") + ".d88");
                    }
                    else
                    {
                        var dialog = new SaveFileDialog();
                        dialog.InitialDirectory = TextBoxDirectory.Text;
                        dialog.Title = "Save Image";
                        dialog.Filter = "D88 Disk Image File(*.d88)|*.d88|All Files(*.*)|*.*";
                        if (dialog.ShowDialog() != true) return;
                        filename = dialog.FileName;
                    }
                    var diskinfo = await Util.GetDiskInf(Util.GetSelectedDrive());
                    var format = Util.GetDiskFormatOverride();
                    ProgressBarDefault.Maximum = (diskinfo.Surfaces + 1) * (diskinfo.MaxTrackNo + 1) * diskinfo.SectorsInTrack;
                    ProgressBarDefault.Value = 0;
                    const int bytesInSector = 256;
                    const int bytesInSectorN = 1;   // 128=0, 256=1, 512=2, 1024=3, 2048=4
                    const int headerOffset = 0x2b0;
                    using (var outputRawStream = File.OpenWrite(filename))
                    {
                        using (var outputStream = new BinaryWriter(outputRawStream))
                        {
                            // Write D88 Header
                            var nameInFile = TextBoxNameInFile.Text;
                            if (string.IsNullOrWhiteSpace(nameInFile)) nameInFile = DateTimeOffset.Now.ToString("yyyyMMddHHmmss");
                            var bytes = Encoding.Default.GetBytes(nameInFile);
                            for (int i = 0; i < 16; i++)
                            {
                                if (bytes.Length > i) outputStream.Write(bytes[i]);
                                else outputStream.Write((byte)0);
                            }
                            // ASCIZ terminater
                            outputStream.Write((byte)0);
                            // RESERVE 9 bytes
                            for (int i = 0; i < 9; i++) outputStream.Write((byte)0);
                            // write protect (Ignore)
                            outputStream.Write((byte)0);
                            // type of disk
                            byte typeOfDisk = 0; // 2D
                            if (diskinfo.MaxTrackNo == 80) typeOfDisk = 0x10; //2DD
                            else if (diskinfo.SectorsInTrack == 26) typeOfDisk = 0x20; //2HD
                            outputStream.Write(typeOfDisk);
                            // size of disk (uint)
                            uint sizeOfDisk = 0x2b0 + (uint)((diskinfo.Surfaces + 1) * (diskinfo.MaxTrackNo + 1) * diskinfo.SectorsInTrack * (0x10 + bytesInSector));
                            outputStream.Write(sizeOfDisk);
                            // Track Table (164 Tracks)
                            int bytesInTrack = diskinfo.SectorsInTrack * (0x10 + bytesInSector);
                            for (int i = 0; i < 164; i++)
                            {
                                uint offset = 0;
                                int surfaceNo = i / (diskinfo.MaxTrackNo+1);
                                if (surfaceNo <= diskinfo.Surfaces) offset = (uint)(headerOffset + i * bytesInTrack);
                                outputStream.Write(offset);
                            }
                            for (int surface = 0; surface < diskinfo.Surfaces + 1; surface++)
                            {
                                for (int track = 0; track < diskinfo.MaxTrackNo + 1; track++)
                                {
                                    //sectors in track
                                    for (int sector = 1; sector < diskinfo.SectorsInTrack + 1; sector++)
                                    {
                                        ProgressBarDefault.Value++;
                                        setStatus(diskinfo.DriveNo, format, surface, track, sector);
                                        var array = await Util.SectorRead(diskinfo.DriveNo, surface, track, sector);
                                        if (array == null) return;
                                        outputStream.Write((byte)track);
                                        outputStream.Write((byte)surface);
                                        outputStream.Write((byte)sector);
                                        outputStream.Write((byte)bytesInSectorN);
                                        outputStream.Write((ushort)diskinfo.SectorsInTrack);
                                        outputStream.Write((byte)0x00); // 0=DS, 0x40=SS
                                        outputStream.Write((byte)0x00); // 0=NORMAL, 0x10=DELETED DATA
                                        outputStream.Write((byte)0x00); // 0=NORMAL END, RETURNED BY DISK BIOS
                                        for (int i = 0; i < 5; i++) outputStream.Write((byte)0x00); // RESERVE
                                        outputStream.Write((ushort)array.Length);
                                        outputStream.Write(array, 0, array.Length);
                                    }
                                }
                            }
                        }
                    }
                    MessageBox.Show("Done Successfully: " + filename);
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

        private void CheckBoxAutoName_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ImageAutoFileName = true;
            Properties.Settings.Default.Save();
        }

        private void CheckBoxAutoName_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ImageAutoFileName = false;
            Properties.Settings.Default.Save();
        }

        private void CheckBoxAutoLabel_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxAutoLabel_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
