using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RemotePC8801
{
    class Util
    {
        public static MainWindow MyMainWindow => ((MainWindow)App.Current.MainWindow);

        public static bool IsPortOpen => MyMainWindow.IsPortOpen;

        public static void AppendLog(string msg) => MyMainWindow?.AppendLog(msg);

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
