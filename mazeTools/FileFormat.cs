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
        ASCII = 1,
        UTF = 2,
        Image = 3
    }
}
