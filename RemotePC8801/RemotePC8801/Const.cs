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
        public static string SectorReadStatements { get { return ":PRINT \"%%%\";:a=VARPTR(#0)+9:" + CommonReadStatements; } }
        public static string RomReadStatements { get { return ":PRINT \"%%%\";:a=0:" + CommonReadStatements; } }
        //public static string CommonReadStatements { get { return "FOR I=0 to 255:V=PEEK(a+i):T$=chr$(V)+\" \"+CHR$((V+&H20) and &hFF):N=(V<=&h20)*-1:L=N+1:PRINT MID$(T$,N+1,L);:NEXT:PRINT"; } }

        public static string CommonReadStatements { get { return "FOR I=a to a+255:print using \"###\";peek(i);:NEXT: PRINT"; } }
    }
}
