using System;
using System.Collections.Generic;
using System.IO;
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
        /// <remarks>This will always be odd, unless it's zero</remarks>
        public int Width { get; private set; }
        /// <summary>
        /// Height
        /// </summary>
        /// <remarks>This will always be odd, unless it's zero</remarks>
        public int Height { get; private set; }
        /// <summary>
        /// Scale
        /// </summary>
        public int Scale { get; private set; }
        /// <summary>
        /// Solve maze
        /// </summary>
        public bool Solve { get; private set; }
        /// <summary>
        /// Clear maze
        /// </summary>
        public bool Clear { get; private set; }
        /// <summary>
        /// allow the user to play the maze
        /// </summary>
        public bool Play { get; private set; }
        /// <summary>
        /// fog of war
        /// </summary>
        public bool Fow { get; private set; }
        /// <summary>
        /// Enables the map
        /// </summary>
        public bool Map { get; private set; }
        /// <summary>
        /// output File
        /// </summary>
        public string OutFile { get; private set; }
        /// <summary>
        /// input File
        /// </summary>
        public string InFile { get; private set; }
        /// <summary>
        /// Output Format
        /// </summary>
        public FileFormat OutFormat { get; private set; }
        /// <summary>
        /// Input Format
        /// </summary>
        public FileFormat InFormat { get; private set; }

        /// <summary>
        /// Creates a new instance from the given parameters and validates the parameters
        /// </summary>
        /// <param name="Arguments">Command line arguments</param>
        public CmdParams(string[] Arguments)
        {
            for (var i = 0; i < Arguments.Length; i++)
            {
                var Arg = Arguments[i];
                var Next = i < Arguments.Length - 1 ? Arguments[i + 1] : null;
                int tempNum = 0;
                FileFormat tempFormat;
                #region Parsing
                switch (Arg.ToUpper())
                {
                    case "/W":
                        if (Width > 0)
                        {
                            throw new ArgumentException($"Duplicate argument: {Arg}");
                        }
                        if (Next == null)
                        {
                            throw new ArgumentException($"{Arg} requires a value but none given");
                        }
                        if (int.TryParse(Next, out tempNum) && tempNum > 0)
                        {
                            Width = Uneven(tempNum);
                            ++i;
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid value for {Arg}. Expecting a number above zero");
                        }
                        break;
                    case "/H":
                        if (Height > 0)
                        {
                            throw new ArgumentException($"Duplicate argument: {Arg}");
                        }
                        if (Next == null)
                        {
                            throw new ArgumentException($"{Arg} requires a value but none given");
                        }
                        if (int.TryParse(Next, out tempNum) && tempNum > 0)
                        {
                            Height = Uneven(tempNum);
                            ++i;
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid value for {Arg}. Expecting a number above zero");
                        }
                        break;
                    case "/M":
                        if (Scale > 0)
                        {
                            throw new ArgumentException($"Duplicate argument: {Arg}");
                        }
                        if (Next == null)
                        {
                            throw new ArgumentException($"{Arg} requires a value but none given");
                        }
                        if (int.TryParse(Next, out tempNum) && tempNum > 0)
                        {
                            Scale = tempNum;
                            ++i;
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid value for {Arg}. Expecting a number above zero");
                        }
                        break;
                    case "/IF":
                        if (InFormat != FileFormat.Autodetect)
                        {
                            throw new ArgumentException($"Duplicate argument: {Arg}");
                        }
                        if (Next == null)
                        {
                            throw new ArgumentException($"{Arg} requires a value but none given");
                        }
                        if (Enum.TryParse(Next, true, out tempFormat))
                        {
                            InFormat = tempFormat;
                            ++i;
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid value for {Arg}. See help for list of values");
                        }
                        break;
                    case "/OF":
                        if (OutFormat != FileFormat.Autodetect)
                        {
                            throw new ArgumentException($"Duplicate argument: {Arg}");
                        }
                        if (Next == null)
                        {
                            throw new ArgumentException($"{Arg} requires a value but none given");
                        }
                        if (Enum.TryParse(Next, true, out tempFormat))
                        {
                            OutFormat = tempFormat;
                            ++i;
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid value for {Arg}. See help for list of values");
                        }
                        break;
                    case "/S":
                        if (Solve)
                        {
                            throw new ArgumentException($"Duplicate argument: {Arg}");
                        }
                        Solve = true;
                        break;
                    case "/C":
                        if (Clear)
                        {
                            throw new ArgumentException($"Duplicate argument: {Arg}");
                        }
                        Clear = true;
                        break;
                    case "/G":
                        if (Play)
                        {
                            throw new ArgumentException($"Duplicate argument: {Arg}");
                        }
                        Play = true;
                        break;
                    case "/FOW":
                        if (Fow)
                        {
                            throw new ArgumentException($"Duplicate argument: {Arg}");
                        }
                        Fow = true;
                        break;
                    case "/MAP":
                        if (Map)
                        {
                            throw new ArgumentException($"Duplicate argument: {Arg}");
                        }
                        Map = true;
                        break;
                    case "/I":
                        if (InFile != null)
                        {
                            throw new ArgumentException($"Duplicate argument: {Arg}");
                        }
                        if (Next == null)
                        {
                            throw new ArgumentException($"{Arg} requires a value but none given");
                        }
                        InFile = Next;
                        ++i;
                        break;
                    case "/O":
                        if (OutFile != null)
                        {
                            throw new ArgumentException($"Duplicate argument: {Arg}");
                        }
                        if (Next == null)
                        {
                            throw new ArgumentException($"{Arg} requires a value but none given");
                        }
                        OutFile = Next;
                        ++i;
                        break;
                    default:
                        throw new ArgumentException($"Invalid argument: {Arg}");
                }
                #endregion
            }

            #region General

            //Solve and clear are mutually exclusive
            if (Solve && Clear)
            {
                throw new ArgumentException("/S and /C can't be used together");
            }
            #endregion

            #region I/O

            //Input file and sizes cant be used together
            if (InFile != null && (Width > 0 || Height > 0))
            {
                throw new ArgumentException("/W and /H can't be used if an input file is specified");
            }
            if (InFormat == FileFormat.Autodetect && InFile != null)
            {
                InFormat = DetectFormatFromFile(InFile);
            }
            if (OutFormat == FileFormat.Autodetect)
            {
                if (OutFile != null)
                {
                    OutFormat = DetectFormatFromExtension(Path.GetExtension(OutFile));
                }
                else
                {
                    OutFormat = FileFormat.UTF;
                }
            }
            if (InFormat != FileFormat.Image && OutFormat != FileFormat.Image && Scale > 0)
            {
                throw new ArgumentException("/S only applies to images but neither input nor output is an image");
            }
            if (Width == 0)
            {
                Width = Uneven(Console.WindowWidth);
            }
            if (Height == 0)
            {
                Height = Uneven(Console.WindowHeight);
            }

            #endregion

            #region Game related

            if (Play && (Solve || Clear))
            {
                throw new ArgumentException("/S and /C can't be used with /G");
            }
            if (!Play && (Map || Fow))
            {
                throw new ArgumentException("/MAP and /FOW must be used with /G");
            }
            if (Map && !Fow)
            {
                throw new ArgumentException("/MAP implies /FOW");
            }

            #endregion
        }

        /// <summary>
        /// Gets the preferred format for a file extension
        /// </summary>
        /// <param name="ext">File extension</param>
        /// <returns>File format</returns>
        private static FileFormat DetectFormatFromExtension(string ext)
        {
            if (string.IsNullOrEmpty(ext))
            {
                return FileFormat.UTF;
            }
            if (ext.ToLower().TrimStart('.') == "png")
            {
                return FileFormat.Image;

            }
            return FileFormat.UTF;
        }

        /// <summary>
        /// Detects the file format by looking at the contents of the input file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Detected file format</returns>
        private static FileFormat DetectFormatFromFile(string fileName)
        {
            using (var FS = File.OpenRead(fileName))
            {
                byte[] data = new byte[4];
                var readed = FS.Read(data, 0, data.Length);
                if (readed == 0)
                {
                    throw new InvalidDataException($"The file '{fileName}' is empty and the format can't be detected.");
                }
                //ASCII format
                if (data[0] == Chars.ASCII.WALL)
                {
                    return FileFormat.ASCII;
                }
                //ëPNG
                if (data.SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }))
                {
                    return FileFormat.Image;
                }
                //UTF-8 format
                if (Encoding.UTF8.GetString(data).First() == Chars.UTF.WALL)
                {
                    return FileFormat.UTF;
                }
                throw new InvalidDataException("File format is not known");
            }
        }

        /// <summary>
        /// makes a number uneven by subtracting 1 if needed.
        /// </summary>
        /// <param name="Number">possibly even number</param>
        /// <returns>uneven number</returns>
        private static int Uneven(int Number)
        {
            return (Number & 1) == 0 ? Number - 1 : Number;
        }
    }
}
