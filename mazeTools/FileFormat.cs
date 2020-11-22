using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mazeTools
{
    /// <summary>
    /// Lists possible file formats
    /// </summary>
    public enum FileFormat : int
    {
        Autodetect = 0,
        Numbered = 1,
        ASCII = 2,
        UTF = 3,
        CSV = 4,
        Binary = 5,
        Image = 6
    }
}
