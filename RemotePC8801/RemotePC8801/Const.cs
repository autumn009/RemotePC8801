using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePC8801
{
    static class Const
    {
        public static string ClearErrStatement { get { return "poke &he649,0"; } }
        public static string GettingVersionExpression { get { return "chr$(peek(&h79d5))+chr$(peek(&h79d6))+chr$(peek(&h79d7))"; } }
        public static string SectorReadStatements { get { return CommonReadStatements; } }
        public static string RomReadStatements { get { return ":PRINT \"%%%\";:a$=space$(128):b$=space$(128):" + CommonReadStatements; } }

        public static string CommonReadStatements { get { return ":PRINT A$;B$"; } }
    }
}
