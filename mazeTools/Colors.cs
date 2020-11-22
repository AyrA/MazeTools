using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mazeTools
{
    /// <summary>
    /// colors for image input and output
    /// </summary>
    public static class Colors
    {
        public const int WALL = -16777216;
        public const int WAY = -1;
        public const int START = -16711936;
        public const int END = -65536;
        public const int VISITED = -256;
        public const int PLAYER = -65281;
        public const int INVALID = -16711681;
    }
}
