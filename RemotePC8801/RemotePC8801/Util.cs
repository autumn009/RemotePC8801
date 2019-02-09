using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RemotePC8801
{
    class DiskInfo
    {
        public int DriveNo; // 読み出したドライブ番号
        public int FreeClustors; //省略時 ： ディスクの 残り 容量 （クラスタ 単位） 
        public int MaxTrackNo; //0 ： 最大 トラック 番号 （= 片面 当りの トラック 数 一 1) 
        public int SectorsInTrack; //1 ： 1 トラック 当りの セクタ 数 
        public int Surfaces; //2 ： ディスクの サーフ ヱ イス （面) 数 一 1 
        public int ClustorsInTrack; //3 ： 1 トラック 当りの クラスタ 数 または 1 クラスタ 当りの トラック 数 
                      //フロッピーディスク （サー フェイス 数一 1 = 1) の 場合 ： 1 トラック 当りの クラ スタ数 
                      //固定 ディスク （サー フェイス 数一 1> 1) の 場合： 1 クラスタ 当りの トラック 数 
        public int ClustorsInVolume; //4 ： ボリューム 当りの クラスタ 数 
        public int DirectoryTrack; //5 ： ディレクトリ トラック 番号 
        public int SectorsInCluster; //6 ： 1 クラスタ 当りの セクタ 数 
        public int FATStartSector; //7 ： FAT の 開始 セクタ 番号 
        public int FATEndSector; //8 ： FAT の 終了 セクタ 番号 
        public int NumverOfFATs; //9 ： FAT の 数 
        public int SectorInDiskAttr; //10 ： ディスク 属性の 入って いる セクタ 番号 

        internal bool VaridateParameters(int drive, int surface, int track, int sector, Action<string> errorReporter)
        {
            if( DriveNo != drive)
            {
                errorReporter($"Drive Not Match {DriveNo}/{drive}");
                return false;
            }
            if (surface < 0)
            {
                errorReporter($"Minimum surface is 0 but {surface}");
                return false;
            }
            if (track < 0)
            {
                errorReporter($"Minimum track is 0 but {track}");
                return false;
            }
            if (sector < 1)
            {
                errorReporter($"Minimum sector is 1 but {sector}");
                return false;
            }
            if (surface > Surfaces)
            {
                errorReporter($"Maximum surface is {Surfaces} but {surface}");
                return false;
            }
            if (track > MaxTrackNo)
            {
                errorReporter($"Maximum track is {MaxTrackNo} but {track}");
                return false;
            }
            if (sector > SectorsInTrack)
            {
                errorReporter($"Maximum sector is {SectorsInTrack} but {sector}");
                return false;
            }
            return true;
        }
    }

    class Util
    {
        public static MainWindow MyMainWindow => ((MainWindow)App.Current.MainWindow);

        public static bool IsPortOpen => MyMainWindow.IsPortOpen;

        public static void AppendLog(string msg) => MyMainWindow?.AppendLog(msg);

        internal static IEnumerable<byte> DecodeBinaryString(string encodedString)
        {
            bool escaped = false;
            foreach (var item in encodedString)
            {
                byte n = (byte)item;
                if (n == 0x20)
                {
                    escaped = true;
                    continue;
                }
                if (escaped) n -= 0x20;
                yield return n;
                escaped = false;
            }
        }

        //public static async Task<MainWindow.ResultStatusMarker> SendCommandAsync(string statement) => await MyMainWindow?.SendCommandAsync(statement);

        public static async Task<bool> SendCommandAsyncAndErrorHandle(string statement, bool forceHandshake = false)
        {
            var errorCode = await MyMainWindow?.SendCommandAsync(statement, forceHandshake);
            if (errorCode == 0) return false;
            AppendLog($"\"{statement}\" FAILED, ERROR CODE={errorCode}\r\n");
            return true;
        }

        static public void EnumVisual(Visual myVisual, Action<Visual> act)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(myVisual); i++)
            {
                // Retrieve child visual at specified index value.
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(myVisual, i);

                // Do processing of the child visual object.
                act(childVisual);

                // Enumerate children of the child visual object.
                EnumVisual(childVisual, act);
            }
        }

        internal static string GetErrorString(MainWindow.ResultStatusMarker r)
        {
            if (Enum.IsDefined(r.GetType(), r)) return Enum.GetName(r.GetType(), r);
            else return $"[UNDEFINED ERROR={((int)r).ToString()}]";
        }

        internal static void ErrorPopup(string message)
        {
            MessageBox.Show(MyMainWindow,message);
        }


        public async static Task<DiskInfo> GetDiskInf( int drive)
        {
            var info = new DiskInfo();
            info.DriveNo = drive;
            var ten = string.Join(",",Enumerable.Range(0, 11).Select(c => $"DSKF({drive},{c})").ToArray());
            if (await Util.SendCommandAsyncAndErrorHandle($"print \"%%%\";:WRITE DSKF({drive}),"+ten)) return null;
            var ar = Util.MyMainWindow.StatementReaultString.Split(',').Select(c=>
            {
                int.TryParse(c.Trim(), out var r);
                return r;
            }).ToArray();
            info.FreeClustors = ar[0];
            info.MaxTrackNo = ar[1];
            info.SectorsInTrack = ar[2];
            info.Surfaces = ar[3];
            info.ClustorsInTrack = ar[4];
            info.ClustorsInVolume = ar[5];
            info.DirectoryTrack = ar[6];
            info.SectorsInCluster = ar[7];
            info.FATStartSector = ar[8];
            info.FATEndSector = ar[9];
            info.NumverOfFATs = ar[10];
            info.SectorInDiskAttr = ar[11];
            return info;
        }

        internal static int GetSelectedDrive()
        {
            return MyMainWindow.ComboDriveSelect.SelectedIndex+1;
        }
    }

    public class LockForm : IDisposable
    {
        private bool oldState;

        public LockForm()
        {
            oldState = Util.MyMainWindow.IsEnabled;
            Util.MyMainWindow.IsEnabled = false;
        }

        public void Dispose()
        {
            Util.MyMainWindow.IsEnabled = oldState;
        }
    }

}
