using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mazeTools
{
    public static class IO
    {
        private const string CRLF = "\r\n";

        /// <summary>
        /// draws a part of the maze, fitting the console. Tries to center around posX and posY
        /// </summary>
        /// <param name="maze">maze</param>
        /// <param name="posX">X position of center (player)</param>
        /// <param name="posY">Y position of center (player)</param>
        public static void DrawFrame(byte[,] MazeArea, bool[,] FOW, int PosX, int PosY)
        {
            byte[,] region = MazeOperations.GetRegion(MazeArea, PosX, PosY, Console.WindowWidth - 1, Console.WindowHeight - 1);
            bool[,] fowregion = MazeOperations.GetRegion(FOW, PosX, PosY, Console.WindowWidth - 1, Console.WindowHeight - 1);
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.White;

            for (int y = 0; y < region.GetLength(1); y++)
            {
                for (int x = 0; x < region.GetLength(0); x++)
                {
                    if (fowregion[x, y])
                    {
                        switch (region[x, y])
                        {
                            case Maze.CellType.WALL:
                                Console.Write(Chars.UTF.WALL);
                                break;
                            case Maze.CellType.WAY:
                                Console.Write(Chars.UTF.WAY);
                                break;
                            case Maze.CellType.START:
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write(Chars.UTF.START);
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                            case Maze.CellType.END:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write(Chars.UTF.END);
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                            case Maze.CellType.VISITED:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write(MazeOperations.GetFancyTile(region, x, y, Chars.Corners.Unknown));
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                            case Maze.CellType.PLAYER:
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                Console.Write(Chars.UTF.PLAYER);
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.Write(Chars.UTF.INVALID);
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                        }
                    }
                    else
                    {
                        Console.Write(Chars.UTF.WAY);
                    }
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }

        /// <summary>
        /// writes the maze to the console using given format
        /// </summary>
        /// <param name="maze">maze</param>
        /// <param name="f">format</param>
        public static void WriteConsole(byte[,] MazeArea, FileFormat FileFormat)
        {
            int w, h, x, y;
            w = MazeArea.GetLength(0);
            h = MazeArea.GetLength(1);

            char WALL, WAY, START, END, VISITED, PLAYER, INVALID;

            switch (FileFormat)
            {
                case FileFormat.ASCII:
                    WALL = Chars.ASCII.WALL;
                    WAY = Chars.ASCII.WAY;
                    START = Chars.ASCII.START;
                    END = Chars.ASCII.END;
                    VISITED = Chars.ASCII.VISITED;
                    PLAYER = Chars.ASCII.PLAYER;
                    INVALID = Chars.ASCII.INVALID;
                    break;
                case FileFormat.UTF:
                    WALL = Chars.UTF.WALL;
                    WAY = Chars.UTF.WAY;
                    START = Chars.UTF.START;
                    END = Chars.UTF.END;
                    VISITED = Chars.UTF.VISITED;
                    PLAYER = Chars.UTF.PLAYER;
                    INVALID = Chars.UTF.INVALID;
                    break;
                default:
                    throw new Exception("Invalid output format for console: " + FileFormat.ToString());
            }
            for (y = 0; y < h; y++)
            {
                for (x = 0; x < w; x++)
                {
                    switch (MazeArea[x, y])
                    {
                        case Maze.CellType.WALL:
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(WALL);
                            break;
                        case Maze.CellType.WAY:
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write(WAY);
                            break;
                        case Maze.CellType.START:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(START);
                            break;
                        case Maze.CellType.END:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(END);
                            break;
                        case Maze.CellType.VISITED:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            if (FileFormat == FileFormat.UTF)
                            {
                                Console.Write(MazeOperations.GetFancyTile(MazeArea, x, y, Chars.Corners.Unknown));
                            }
                            else
                            {
                                Console.Write(VISITED);
                            }
                            break;
                        case Maze.CellType.PLAYER:
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.Write(PLAYER);
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write(INVALID);
                            break;
                    }
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }

        /// <summary>
        /// writes the given maze to a file
        /// </summary>
        /// <param name="FileName">file name</param>
        /// <param name="maze">maze</param>
        /// <param name="f">format</param>
        /// <param name="scale">scale (for images only)</param>
        public static void WriteFile(string FileName, byte[,] MazeArea, FileFormat FileFormat, int Scale)
        {
            int w, h, x, y;
            w = MazeArea.GetLength(0);
            h = MazeArea.GetLength(1);

            char WALL, WAY, START, END, VISITED, PLAYER, INVALID;
            WALL = WAY = START = END = PLAYER = VISITED = INVALID = '\0';

            switch (FileFormat)
            {
                case FileFormat.ASCII:
                    WALL = Chars.ASCII.WALL;
                    WAY = Chars.ASCII.WAY;
                    START = Chars.ASCII.START;
                    END = Chars.ASCII.END;
                    VISITED = Chars.ASCII.VISITED;
                    PLAYER = Chars.ASCII.PLAYER;
                    INVALID = Chars.ASCII.INVALID;
                    break;
                case FileFormat.UTF:
                    WALL = Chars.UTF.WALL;
                    WAY = Chars.UTF.WAY;
                    START = Chars.UTF.START;
                    END = Chars.UTF.END;
                    VISITED = Chars.UTF.VISITED;
                    PLAYER = Chars.UTF.PLAYER;
                    INVALID = Chars.UTF.INVALID;
                    break;
                default:
                    break;
            }

            if (FileFormat == FileFormat.ASCII || FileFormat == FileFormat.UTF)
            {
                StreamWriter SW;
                try
                {
                    SW = new StreamWriter(File.Create(FileName));
                }
                catch(Exception ex)
                {
                    throw new Exception("cannot create output file: " + FileName, ex);
                }

                for (y = 0; y < h; y++)
                {
                    for (x = 0; x < w; x++)
                    {
                        switch (MazeArea[x, y])
                        {
                            case Maze.CellType.WALL:
                                SW.Write(WALL);
                                break;
                            case Maze.CellType.WAY:
                                SW.Write(WAY);
                                break;
                            case Maze.CellType.START:
                                SW.Write(START);
                                break;
                            case Maze.CellType.END:
                                SW.Write(END);
                                break;
                            case Maze.CellType.VISITED:
                                if (FileFormat == FileFormat.UTF)
                                {
                                    SW.Write(MazeOperations.GetFancyTile(MazeArea, x, y, Chars.Corners.Unknown));
                                }
                                else
                                {
                                    SW.Write(VISITED);
                                }
                                break;
                            case Maze.CellType.PLAYER:
                                SW.Write(PLAYER);
                                break;
                            default:
                                SW.Write(INVALID);
                                break;
                        }
                    }
                    //no line break for last line
                    if (y < h - 1)
                    {
                        SW.WriteLine();
                    }
                }
                SW.Flush();
                SW.Close();
                SW.Dispose();
            }
            else if (FileFormat == FileFormat.Image)
            {
                WriteImage(FileName, MazeArea, Scale);
            }
            else
            {
                throw new ArgumentException($"Invalid file format: {FileFormat}");
            }
        }

        /// <summary>
        /// writes the maze to an image
        /// </summary>
        /// <param name="FileName">file name</param>
        /// <param name="maze">maze</param>
        /// <param name="scale">scale</param>
        public static void WriteImage(string FileName, byte[,] MazeArea, int Scale)
        {
            Console.Error.WriteLine("Generating Image");

            using (var FS = File.Create(FileName))
            {
                using (Bitmap B = new Bitmap(MazeArea.GetLength(0), MazeArea.GetLength(1), PixelFormat.Format16bppRgb565))
                {
                    for (int y = 0; y < MazeArea.GetLength(1); y++)
                    {
                        Console.Error.WriteLine("Drawing Row {0}...", y + 1);
                        for (int x = 0; x < MazeArea.GetLength(0); x++)
                        {
                            switch (MazeArea[x, y])
                            {
                                case Maze.CellType.WALL:
                                    B.SetPixel(x, y, Color.Black);
                                    break;
                                case Maze.CellType.WAY:
                                    B.SetPixel(x, y, Color.White);
                                    break;
                                case Maze.CellType.START:
                                    B.SetPixel(x, y, Color.Lime);
                                    break;
                                case Maze.CellType.END:
                                    B.SetPixel(x, y, Color.Red);
                                    break;
                                case Maze.CellType.VISITED:
                                    B.SetPixel(x, y, Color.Yellow);
                                    break;
                                case Maze.CellType.PLAYER:
                                    B.SetPixel(x, y, Color.Fuchsia);
                                    break;
                                default:
                                    B.SetPixel(x, y, Color.Cyan);
                                    break;
                            }
                        }
                    }

                    if (Scale > 1)
                    {
                        Console.Error.WriteLine("resizing...");
                        using (Bitmap Dest = new Bitmap(B.Width * Scale, B.Height * Scale, B.PixelFormat))
                        {
                            using (Graphics G = Graphics.FromImage(Dest))
                            {
                                G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                                G.DrawImage(B, 0 + Scale / 2, 0 + Scale / 2, Dest.Width, Dest.Height);
                            }

                            try
                            {
                                Dest.Save(FS, ImageFormat.Png);
                            }
                            catch
                            {
                                Console.Error.WriteLine("Cannot save resized image. Probably too big");
                                Console.Error.WriteLine("Try to save unscaled image...");
                                FS.Position = 0;
                                try
                                {
                                    B.Save(FS, ImageFormat.Png);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Unable to save the maze data to a file", ex);
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            B.Save(FS, ImageFormat.Png);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Unable to save the maze data to a file", ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// converts file input to a usable maze
        /// </summary>
        /// <param name="p">readed data from text file</param>
        /// <param name="format">input data format</param>
        /// <param name="scale">scale factor (images only)</param>
        /// <returns>maze</returns>
        /// <remarks>This converts any maze type is can read into the ASCII format</remarks>
        public static byte[,] ParseFile(string FileName, FileFormat FileFormat, int Scale)
        {
            string s = null;

            switch (FileFormat)
            {
                case FileFormat.Image:
                    s = FromImage(FileName, Scale);
                    break;
                case FileFormat.ASCII:
                    s = File.ReadAllText(FileName);
                    break;
                case FileFormat.UTF:
                    s = File.ReadAllText(FileName)
                        .Replace(Chars.UTF.WALL, Chars.ASCII.WALL)
                        .Replace(Chars.UTF.WAY, Chars.ASCII.WAY)
                        .Replace(Chars.UTF.START, Chars.ASCII.START)
                        .Replace(Chars.UTF.END, Chars.ASCII.END)
                        .Replace(Chars.UTF.VISITED, Chars.ASCII.VISITED)
                        .Replace(Chars.UTF.PLAYER, Chars.ASCII.PLAYER)
                        .Replace(Chars.Corners.EW, Chars.ASCII.VISITED)
                        .Replace(Chars.Corners.NE, Chars.ASCII.VISITED)
                        .Replace(Chars.Corners.NES, Chars.ASCII.VISITED)
                        .Replace(Chars.Corners.NESW, Chars.ASCII.VISITED)
                        .Replace(Chars.Corners.NEW, Chars.ASCII.VISITED)
                        .Replace(Chars.Corners.NS, Chars.ASCII.VISITED)
                        .Replace(Chars.Corners.NW, Chars.ASCII.VISITED)
                        .Replace(Chars.Corners.NWS, Chars.ASCII.VISITED)
                        .Replace(Chars.Corners.SE, Chars.ASCII.VISITED)
                        .Replace(Chars.Corners.SEW, Chars.ASCII.VISITED)
                        .Replace(Chars.Corners.SW, Chars.ASCII.VISITED)
                        .Replace(Chars.Corners.Unknown, Chars.ASCII.VISITED);
                    break;
                default:
                    throw new ArgumentException($"Unknown input format: {FileFormat}");
            }

            int x, y, w, h;

            string[] Lines = s.Split(new string[] { CRLF }, StringSplitOptions.None);
            s = null;

            w = Lines[0].Length;
            h = Lines.Length;

            byte[,] retValue = new byte[w, h];

            for (y = 0; y < h; y++)
            {
                if (y + 1 % 10 == 0)
                {
                    Console.Error.WriteLine("Converting row {0}...", y + 1);
                }

                for (x = 0; x < w; x++)
                {
                    switch (Lines[y][x])
                    {
                        case Chars.ASCII.WAY:
                            retValue[x, y] = Maze.CellType.WAY;
                            break;
                        case Chars.ASCII.WALL:
                            retValue[x, y] = Maze.CellType.WALL;
                            break;
                        case Chars.ASCII.START:
                            retValue[x, y] = Maze.CellType.START;
                            break;
                        case Chars.ASCII.END:
                            retValue[x, y] = Maze.CellType.END;
                            break;
                        case Chars.ASCII.VISITED:
                            retValue[x, y] = Maze.CellType.VISITED;
                            break;
                        case Chars.ASCII.PLAYER:
                            retValue[x, y] = Maze.CellType.PLAYER;
                            break;
                        default:
                            throw new Exception(string.Format("Invalid char at {0},{1}: {2}", x, y, Lines[x][y]));
                    }
                }
            }

            return retValue;
        }

        /// <summary>
        /// reads a maze from an image
        /// </summary>
        /// <param name="p">image file</param>
        /// <param name="scale">scale</param>
        /// <returns>fle input string in format UTF</returns>
        public static string FromImage(string FileName, int Scale)
        {
            Console.Error.WriteLine("Opening image...");
            Bitmap B;
            try
            {
                B = (Bitmap)Image.FromFile(FileName);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid image file: " + FileName, ex);
            }
            using (B)
            {
                StringBuilder SB = new StringBuilder((B.Width / Scale) * (B.Height / Scale) + (B.Height / Scale * 2));

                for (int h = 0; h < B.Height; h += Scale)
                {
                    if ((h / Scale + 1) % 10 == 0)
                    {
                        Console.Error.WriteLine("Reading row {0}...", h + 1);
                    }
                    for (int w = 0; w < B.Width; w += Scale)
                    {
                        switch (B.GetPixel(w, h).ToArgb())
                        {
                            case Colors.WALL:
                                SB.Append(Chars.ASCII.WALL);
                                break;
                            case Colors.WAY:
                                SB.Append(Chars.ASCII.WAY);
                                break;
                            case Colors.START:
                                SB.Append(Chars.ASCII.START);
                                break;
                            case Colors.END:
                                SB.Append(Chars.ASCII.END);
                                break;
                            case Colors.VISITED:
                                SB.Append(Chars.ASCII.VISITED);
                                break;
                            case Colors.PLAYER:
                                SB.Append(Chars.ASCII.PLAYER);
                                break;
                            default:
                                throw new Exception(string.Format("Invalid image color at {0},{1}", w, h));
                        }
                    }
                    SB.AppendLine();
                }
                return SB.ToString().Trim();
            }
        }
    }
}
