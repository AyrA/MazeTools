using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mazeTools
{
    /// <summary>
    /// struct to hold chars for output and input
    /// </summary>
    public static class Chars
    {
        /// <summary>
        /// UTF-8 corner pieces
        /// </summary>
        public static class Corners
        {
            public const char NS = '│';
            public const char EW = '─';
            public const char NE = '└';
            public const char NW = '┘';
            public const char SE = '┌';
            public const char SW = '┐';
            public const char NESW = '┼';
            public const char NES = '├';
            public const char NWS = '┤';
            public const char NEW = '┴';
            public const char SEW = '┬';
            public const char Unknown = '■';
        }
        /// <summary>
        /// UTF-8 chars
        /// </summary>
        public static class UTF
        {
            public const char WALL = '█';
            public const char WAY = ' ';
            public const char START = 'S';
            public const char END = 'E';
            public const char VISITED = '░';
            public const char PLAYER = '☺';
            public const char INVALID = '?';
        };
        /// <summary>
        /// 7-bit ASCII chars
        /// </summary>
        public static class ASCII
        {
            public const char WALL = '#';
            public const char WAY = ' ';
            public const char START = 'S';
            public const char END = 'E';
            public const char VISITED = '.';
            public const char PLAYER = '!';
            public const char INVALID = '?';
        };
        /// <summary>
        /// Numbered file format
        /// </summary>
        public static class Numbered
        {
            public const char WAY = '0';
            public const char WALL = '1';
            public const char START = '2';
            public const char END = '3';
            public const char VISITED = '4';
            public const char PLAYER = '5';
            public const char INVALID = '6';
        }
    }
}
