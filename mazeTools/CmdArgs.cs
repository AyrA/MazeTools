using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mazeTools
{
    /// <summary>
    /// struct to hold all command line arguments
    /// </summary>
    public class CmdParams
    {
        /// <summary>
        /// Width
        /// </summary>
        public int W;
        /// <summary>
        /// Height
        /// </summary>
        public int H;
        /// <summary>
        /// Scale
        /// </summary>
        public int S;
        /// <summary>
        /// Solve maze?
        /// </summary>
        public bool solve;
        /// <summary>
        /// true, if arguments passed validation
        /// </summary>
        public bool OK;
        /// <summary>
        /// allow the user to play the maze
        /// </summary>
        public bool play;
        /// <summary>
        /// fog of war
        /// </summary>
        public bool fow;
        /// <summary>
        /// Enables the map
        /// </summary>
        public bool map;
        /// <summary>
        /// output File
        /// </summary>
        public string outFile;
        /// <summary>
        /// input File
        /// </summary>
        public string inFile;
        /// <summary>
        /// Output Format
        /// </summary>
        public FileFormat outFormat;
        /// <summary>
        /// Input Format
        /// </summary>
        public FileFormat inFormat;

        public CmdParams(string[] Arguments)
        {

        }
    }
}
