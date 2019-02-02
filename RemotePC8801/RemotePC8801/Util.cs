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

        public async Task<int> SendCommandAsync(string statement) => await MyMainWindow?.SendCommandAsync(statement);

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
    }
}
