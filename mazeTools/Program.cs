using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace mazeTools
{
    class Program
    {
        /// <summary>
        /// Lists possible file formats
        /// </summary>
        private enum Format
        {
            Invalid,
            Autodetect,
            Numbered,
            ASCII,
            UTF,
            CSV,
            Binary,
            Image
        }

        /// <summary>
        /// struct to hold all command line arguments
        /// </summary>
        private struct CmdParams
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
            public Format outFormat;
            /// <summary>
            /// Input Format
            /// </summary>
            public Format inFormat;
        }

        /// <summary>
        /// struct to hold chars for output and input
        /// </summary>
        private struct Chars
        {
            /// <summary>
            /// UTF-8 corner pieces
            /// </summary>
            public struct Corners
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
            public struct UTF
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
            public struct ASCII
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
            public struct Numbered
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
        /// <summary>
        /// colors for image input and output
        /// </summary>
        private struct Colors
        {
            public const int WALL = -16777216;
            public const int WAY = -1;
            public const int START = -16711936;
            public const int END = -65536;
            public const int VISITED = -256;
            public const int PLAYER = -65281;
            public const int INVALID = -16711681;
        }

        /// <summary>
        /// line break
        /// </summary>
        private const string CRLF = "\r\n";
        /// <summary>
        /// file read and write buffer
        /// </summary>
        private const int BUFFER = 10240; //10 KB buffer

        /// <summary>
        /// main function. Calls functions depending on arguments
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static int Main(string[] args)
        {
#if DEBUG
            args = new string[]
            {
                "/G",
                "/FOW",
                "/MAP",
                "/H:100"
            };
#endif

            CmdParams P = ParseArgs(args);

            if (P.OK)
            {
                Maze M = new Maze();
                if (P.inFile != null)
                {
                    Console.Error.WriteLine("Reading input File...");
                    try
                    {
                        M.CurrentMaze = parseFile(P.inFile, P.inFormat, P.S);
                    }
                    catch(Exception ex)
                    {
                        Console.Error.WriteLine("Error reading file: " + ex.Message);
                        return 1;
                    }
                }
                else
                {
                    Console.Error.WriteLine("Generating maze...");
                    M.generate2(P.W, P.H);
                }
                if (P.outFormat != Format.Binary && !P.play)
                {
                    if (P.solve)
                    {
                        Console.Error.WriteLine("solving...");
                        M.Solve();
                    }
                    else
                    {
                        Console.Error.WriteLine("unsolving...");
                        M.Unsolve();
                    }
                }

                if (P.outFile != null)
                {
                    Console.Error.WriteLine("generating File...");
                    WriteFile(P.outFile, M.CurrentMaze, P.outFormat, P.S);
                }
                else if(!P.play)
                {
                    Console.Clear();
                    WriteConsole(M.CurrentMaze, P.outFormat);
                }

                if (P.play)
                {
                    PlayMaze(M, P.outFile, P.fow, P.map);
                }
            }
            else
            {
                Console.ReadKey(true);
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// allows the user to play a maze
        /// </summary>
        /// <param name="m">Maze</param>
        /// <param name="FileName">file name for progress save function</param>
        private static void PlayMaze(Maze m, string FileName,bool enableFOW, bool enableMAP)
        {
            byte[,] maze = m.CurrentMaze;
            
            bool[,] fow = new bool[maze.GetLength(0), maze.GetLength(1)];

            for (int y = 0; y < fow.GetLength(1); y++)
            {
                for (int x = 0; x < fow.GetLength(0); x++)
                {
                    fow[x, y] = !enableFOW;
                }
            }

            int posX = m.Player[0], posY = m.Player[1];
            int toX = m.End[0], toY = m.End[1];

            Console.Clear();
            Console.CursorTop += Console.WindowHeight - 1;
            Console.Write("↑→↓← - Move | [ESC] - Exit | [S] - Save");
            if(string.IsNullOrEmpty(FileName))
            {
                FileName = "game.txt";
            }
            ConsoleKeyInfo KI;
            do
            {
                if (enableFOW)
                {
                    doFOW(maze, fow, posX, posY);
                }
                drawFrame(maze, fow, posX, posY);

                if (enableFOW && !enableMAP)
                {
                    for (int y = 0; y < fow.GetLength(1); y++)
                    {
                        for (int x = 0; x < fow.GetLength(0); x++)
                        {
                            fow[x, y] = false;
                        }
                    }
                }

                KI = Console.ReadKey(true);
                switch (KI.Key)
                {
                    case ConsoleKey.UpArrow:
                        do
                        {
                            if (maze[posX, posY - 1] != Maze.CellType.WALL)
                            {
                                setTile(maze, posX, posY, Maze.CellType.VISITED);
                                setTile(maze, posX, --posY, Maze.CellType.PLAYER);
                            }
                        } while (IsCorridor(maze, posX, posY, Maze.Dir.N));
                        break;
                    case ConsoleKey.DownArrow:
                        do
                        {
                            if (maze[posX, posY + 1] != Maze.CellType.WALL)
                            {
                                setTile(maze, posX, posY, Maze.CellType.VISITED);
                                setTile(maze, posX, ++posY, Maze.CellType.PLAYER);
                            }
                        } while (IsCorridor(maze, posX, posY, Maze.Dir.S));
                        break;
                    case ConsoleKey.LeftArrow:
                        do
                        {
                            if (maze[posX - 1, posY] != Maze.CellType.WALL)
                            {
                                setTile(maze, posX, posY, Maze.CellType.VISITED);
                                setTile(maze, --posX, posY, Maze.CellType.PLAYER);
                            }
                        } while (IsCorridor(maze, posX, posY, Maze.Dir.W));
                        break;
                    case ConsoleKey.RightArrow:
                        do
                        {
                            if (maze[posX + 1, posY] != Maze.CellType.WALL)
                            {
                                setTile(maze, posX, posY, Maze.CellType.VISITED);
                                setTile(maze, ++posX, posY, Maze.CellType.PLAYER);
                            }
                        } while (IsCorridor(maze, posX, posY, Maze.Dir.E));
                        break;
                    case ConsoleKey.S:
                        WriteFile(FileName, maze, Format.UTF, 1);
                        break;
                    default:
                        break;
                }
                if (posX == toX && posY == toY)
                {
                    doFOW(maze, fow, posX, posY);
                    drawFrame(maze, fow, posX, posY);
                    return;
                }
            } while (KI.Key != ConsoleKey.Escape);
        }

        /// <summary>
        /// updates the FOW field
        /// </summary>
        /// <param name="maze">maze</param>
        /// <param name="fow">fof of war</param>
        /// <param name="posX">Player X</param>
        /// <param name="posY">Player Y</param>
        private static void doFOW(byte[,] maze, bool[,] fow, int posX, int posY)
        {
            int x = 0;
            int y = 0;

            while (maze[posX + x, posY + y] != Maze.CellType.WALL)
            {
                setTrue(fow, posX + x, posY + y);
                x++;
            }
            x = 0;
            while (maze[posX + x, posY + y] != Maze.CellType.WALL)
            {
                setTrue(fow, posX + x, posY + y);
                x--;
            }
            x = 0;
            while (maze[posX + x, posY + y] != Maze.CellType.WALL)
            {
                setTrue(fow, posX + x, posY + y);
                y++;
            }
            y = 0;
            while (maze[posX + x, posY + y] != Maze.CellType.WALL)
            {
                setTrue(fow, posX + x, posY + y);
                y--;
            }
        }

        /// <summary>
        /// sets a specific FOW location and all surrounding elements to true
        /// </summary>
        /// <param name="fow">fow</param>
        /// <param name="x">x position of center</param>
        /// <param name="y">y position of center</param>
        private static void setTrue(bool[,] fow, int x, int y)
        {
            fow[x + 0, y - 1] = true; //N
            fow[x + 1, y - 1] = true; //NE
            fow[x + 1, y + 0] = true; //E
            fow[x + 1, y + 1] = true; //SE
            fow[x + 0, y + 1] = true; //S
            fow[x - 1, y + 1] = true; //SW
            fow[x - 1, y + 0] = true; //W
            fow[x - 1, y - 1] = true; //NW
            fow[x + 0, y + 0] = true; //Center
        }

        /// <summary>
        /// Sets a tile to a specific value, protecting some tiles from changes
        /// </summary>
        /// <param name="maze">maze</param>
        /// <param name="posX">X position</param>
        /// <param name="posY">Y position</param>
        /// <param name="tile">new tile value</param>
        private static void setTile(byte[,] maze, int posX, int posY, byte tile)
        {
            if (maze[posX, posY] == Maze.CellType.WAY ||
                maze[posX, posY] == Maze.CellType.PLAYER ||
                maze[posX, posY] == Maze.CellType.VISITED)
            {
                maze[posX, posY] = tile;
            }
        }

        /// <summary>
        /// checks if a given tile is a straight corridor
        /// </summary>
        /// <param name="maze">maze</param>
        /// <param name="posX">X position</param>
        /// <param name="posY">Y position</param>
        /// <param name="dir">direction of player</param>
        /// <returns>true, if corridor</returns>
        private static bool IsCorridor(byte[,] maze, int posX, int posY, Maze.Dir dir)
        {
            if (dir == Maze.Dir.S || dir == Maze.Dir.N)
            {
                return
                    maze[posX - 1, posY] == Maze.CellType.WALL &&
                    maze[posX + 1, posY] == Maze.CellType.WALL &&
                    maze[posX, posY - 1] != Maze.CellType.WALL &&
                    maze[posX, posY + 1] != Maze.CellType.WALL;
            }
            else
            {
                return
                    maze[posX - 1, posY] != Maze.CellType.WALL &&
                    maze[posX + 1, posY] != Maze.CellType.WALL &&
                    maze[posX, posY - 1] == Maze.CellType.WALL &&
                    maze[posX, posY + 1] == Maze.CellType.WALL;
            }
        }

        /// <summary>
        /// draws a part of the maze, fitting the console. Tries to center around posX and posY
        /// </summary>
        /// <param name="maze">maze</param>
        /// <param name="posX">X position of center (player)</param>
        /// <param name="posY">Y position of center (player)</param>
        private static void drawFrame(byte[,] maze, bool[,] fow, int posX, int posY)
        {
            byte[,] region = getRegion(maze, posX, posY, Console.WindowWidth - 1, Console.WindowHeight - 1);
            bool[,] fowregion = getRegion(fow, posX, posY, Console.WindowWidth - 1, Console.WindowHeight - 1);
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
                                //Console.Write(Chars.UTF.VISITED);
                                Console.Write(getFancyTile(region, x, y, Chars.Corners.Unknown));
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
        /// Gets the currently visible fow region
        /// </summary>
        /// <param name="fow">fow</param>
        /// <param name="posX">center X</param>
        /// <param name="posY">center Y</param>
        /// <param name="w">maximum width</param>
        /// <param name="h">maximum height</param>
        /// <returns>fow subregion</returns>
        private static bool[,] getRegion(bool[,] fow, int posX, int posY, int w, int h)
        {
            //fix w and h if needed
            if (w > fow.GetLength(0))
            {
                w = fow.GetLength(0);
            }
            if (h > fow.GetLength(1))
            {
                h = fow.GetLength(1);
            }

            bool[,] region = new bool[w, h];
            int x, y, Mx, My, Mw, Mh;

            Mx = My = 0;
            Mw = w;
            Mh = h;

            for (y = 0; y < h; y++)
            {
                for (x = 0; x < w; x++)
                {
                    region[x, y] = false;
                }
            }

            if (posX < 0)
            {
                posX = 0;
            }
            else if (posX > fow.GetLength(0) - 1)
            {
                posX = fow.GetLength(0) - 1;
            }
            if (posY < 0)
            {
                posY = 0;
            }
            else if (posY > fow.GetLength(1) - 1)
            {
                posY = fow.GetLength(1) - 1;
            }

            //try to center the player
            Mx = posX - w / 2;
            My = posY - h / 2;

            if (Mx < 0)
            {
                Mx = 0;
            }
            else if (Mx + w > fow.GetLength(0))
            {
                Mx = fow.GetLength(0) - w;
            }
            if (My < 0)
            {
                My = 0;
            }
            else if (My + h > fow.GetLength(1))
            {
                My = fow.GetLength(1) - h;
            }

            //Fill maze
            for (y = My; y - My < Mh && y - My < fow.GetLength(1); y++)
            {
                for (x = Mx; x - Mx < Mw && x - Mx < fow.GetLength(0); x++)
                {
                    region[x - Mx, y - My] = fow[x, y];
                }
            }

            return region;
        }

        /// <summary>
        /// gets a "fancy" tile for visited areas to draw lines using box drawing chars
        /// </summary>
        /// <param name="maze">maze</param>
        /// <param name="x">position of tile (X)</param>
        /// <param name="y">position of tile (Y)</param>
        /// <param name="unknown">return value for unknown state</param>
        /// <returns>corner piece (from Chars.Corners)</returns>
        private static char getFancyTile(byte[,] maze, int x, int y, char unknown)
        {
            byte N, S, E, W;
            N = S = E = W = Maze.CellType.INVALID;

            if (y == 0)
            {
                N = Maze.CellType.WAY;
                S = maze[x, y + 1];
            }
            else if (y > maze.GetLength(1) - 2)
            {
                S = Maze.CellType.WAY;
                N = maze[x, y - 1];
            }
            else
            {
                N = maze[x, y - 1];
                S = maze[x, y + 1];
            }
            if (x == 0)
            {
                W = Maze.CellType.WAY;
                E = maze[x + 1, y];
            }
            else if (x > maze.GetLength(0) - 2)
            {
                E = Maze.CellType.WAY;
                W = maze[x - 1, y];
            }
            else
            {
                E = maze[x + 1, y];
                W = maze[x - 1, y];
            }

            if (N == Maze.CellType.VISITED &&
                S == Maze.CellType.VISITED &&
                E == Maze.CellType.VISITED &&
                W == Maze.CellType.VISITED)
            {
                return Chars.Corners.NESW;
            }
            if (N == Maze.CellType.VISITED &&
                S != Maze.CellType.VISITED &&
                E == Maze.CellType.VISITED &&
                W != Maze.CellType.VISITED)
            {
                return Chars.Corners.NE;
            }
            if (N == Maze.CellType.VISITED &&
                S != Maze.CellType.VISITED &&
                E != Maze.CellType.VISITED &&
                W == Maze.CellType.VISITED)
            {
                return Chars.Corners.NW;
            }
            if (N != Maze.CellType.VISITED &&
                S == Maze.CellType.VISITED &&
                E == Maze.CellType.VISITED &&
                W != Maze.CellType.VISITED)
            {
                return Chars.Corners.SE;
            }
            if (N != Maze.CellType.VISITED &&
                S == Maze.CellType.VISITED &&
                E != Maze.CellType.VISITED &&
                W == Maze.CellType.VISITED)
            {
                return Chars.Corners.SW;
            }
            if (N == Maze.CellType.VISITED &&
                S == Maze.CellType.VISITED &&
                E != Maze.CellType.VISITED &&
                W != Maze.CellType.VISITED)
            {
                return Chars.Corners.NS;
            }
            if (N != Maze.CellType.VISITED &&
                S != Maze.CellType.VISITED &&
                E == Maze.CellType.VISITED &&
                W == Maze.CellType.VISITED)
            {
                return Chars.Corners.EW;
            }
            if (N == Maze.CellType.VISITED &&
                S == Maze.CellType.VISITED &&
                E == Maze.CellType.VISITED &&
                W != Maze.CellType.VISITED)
            {
                return Chars.Corners.NES;
            }
            if (N == Maze.CellType.VISITED &&
                S == Maze.CellType.VISITED &&
                E != Maze.CellType.VISITED &&
                W == Maze.CellType.VISITED)
            {
                return Chars.Corners.NWS;
            }
            if (N != Maze.CellType.VISITED &&
                S == Maze.CellType.VISITED &&
                E == Maze.CellType.VISITED &&
                W == Maze.CellType.VISITED)
            {
                return Chars.Corners.SEW;
            }
            if (N == Maze.CellType.VISITED &&
                S != Maze.CellType.VISITED &&
                E == Maze.CellType.VISITED &&
                W == Maze.CellType.VISITED)
            {
                return Chars.Corners.NEW;
            }
            
            return unknown;
        }

        /// <summary>
        /// copies part of the maze to a new array. Tries to center around x and y if possible.
        /// Shrinks w and h if needed.
        /// </summary>
        /// <param name="maze">maze</param>
        /// <param name="posX">Player position X</param>
        /// <param name="posY">player position Y</param>
        /// <param name="w">Width of portion</param>
        /// <param name="h">Height of portion</param>
        /// <returns>copied region</returns>
        private static byte[,] getRegion(byte[,] maze, int posX, int posY, int w, int h)
        {
            //fix w and h if needed
            if (w > maze.GetLength(0))
            {
                w = maze.GetLength(0);
            }
            if (h > maze.GetLength(1))
            {
                h = maze.GetLength(1);
            }

            byte[,] region = new byte[w, h];
            int x, y, Mx, My, Mw, Mh;

            Mx = My = 0;
            Mw = w;
            Mh = h;

            for (y = 0; y < h; y++)
            {
                for (x = 0; x < w; x++)
                {
                    region[x, y] = Maze.CellType.WAY;
                }
            }

            if (posX < 0)
            {
                posX = 0;
            }
            else if (posX > maze.GetLength(0) - 1)
            {
                posX = maze.GetLength(0) - 1;
            }
            if (posY < 0)
            {
                posY = 0;
            }
            else if (posY > maze.GetLength(1) - 1)
            {
                posY = maze.GetLength(1) - 1;
            }

            //try to center the player
            Mx = posX - w / 2;
            My = posY - h / 2;

            if (Mx < 0)
            {
                Mx = 0;
            }
            else if (Mx + w > maze.GetLength(0))
            {
                Mx = maze.GetLength(0) - w;
            }
            if (My < 0)
            {
                My = 0;
            }
            else if (My + h > maze.GetLength(1))
            {
                My = maze.GetLength(1) - h;
            }

            //Fill maze
            for (y = My; y - My < Mh && y - My < maze.GetLength(1); y++)
            {
                for (x = Mx; x - Mx < Mw && x - Mx < maze.GetLength(0); x++)
                {
                    //overdraw start and end with player if he is there.
                    //this is not stored on the file. Loading the maze will set
                    //the player to start if he is not found
                    if (maze[x, y] == Maze.CellType.START && posX == x && posY == y)
                    {
                        region[x - Mx, y - My] = Maze.CellType.PLAYER;
                    }
                    else if (maze[x, y] == Maze.CellType.END && posX == x && posY == y)
                    {
                        region[x - Mx, y - My] = Maze.CellType.PLAYER;
                    }
                    else
                    {
                        region[x - Mx, y - My] = maze[x, y];
                    }

                }
            }

            return region;
        }

        /// <summary>
        /// writes the maze to the console using given format
        /// </summary>
        /// <param name="maze">maze</param>
        /// <param name="f">format</param>
        private static void WriteConsole(byte[,] maze, Format f)
        {
            int w, h, x, y;
            w = maze.GetLength(0);
            h = maze.GetLength(1);

            char WALL, WAY, START, END, VISITED, PLAYER, INVALID;

            switch (f)
            {
                case Format.ASCII:
                    WALL = Chars.ASCII.WALL;
                    WAY = Chars.ASCII.WAY;
                    START = Chars.ASCII.START;
                    END = Chars.ASCII.END;
                    VISITED = Chars.ASCII.VISITED;
                    PLAYER = Chars.ASCII.PLAYER;
                    INVALID = Chars.ASCII.INVALID;
                    break;
                case Format.Numbered:
                case Format.CSV:
                    WALL = Chars.Numbered.WALL;
                    WAY = Chars.Numbered.WAY;
                    START = Chars.Numbered.START;
                    END = Chars.Numbered.END;
                    VISITED = Chars.Numbered.VISITED;
                    PLAYER = Chars.Numbered.PLAYER;
                    INVALID = Chars.Numbered.INVALID;
                    break;
                case Format.UTF:
                    WALL = Chars.UTF.WALL;
                    WAY = Chars.UTF.WAY;
                    START = Chars.UTF.START;
                    END = Chars.UTF.END;
                    VISITED = Chars.UTF.VISITED;
                    PLAYER = Chars.UTF.PLAYER;
                    INVALID = Chars.UTF.INVALID;
                    break;
                default:
                    throw new Exception("Invalid output format for console: " + f.ToString());
            }
            for (y = 0; y < h; y++)
            {
                for (x = 0; x < w; x++)
                {
                    switch (maze[x, y])
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
                            if (f == Format.UTF)
                            {
                                Console.Write(getFancyTile(maze, x, y, Chars.Corners.Unknown));
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
                    if (x < w - 1 && f == Format.CSV)
                    {
                        Console.Write(',');
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
        private static void WriteFile(string FileName, byte[,] maze, Format f, int scale)
        {
            int w, h, x, y;
            w = maze.GetLength(0);
            h = maze.GetLength(1);

            char WALL, WAY, START, END, VISITED, PLAYER, INVALID;
            WALL = WAY = START = END = PLAYER = VISITED = INVALID = '\0';

            switch (f)
            {
                case Format.ASCII:
                    WALL = Chars.ASCII.WALL;
                    WAY = Chars.ASCII.WAY;
                    START = Chars.ASCII.START;
                    END = Chars.ASCII.END;
                    VISITED = Chars.ASCII.VISITED;
                    PLAYER = Chars.ASCII.PLAYER;
                    INVALID = Chars.ASCII.INVALID;
                    break;
                case Format.Numbered:
                case Format.CSV:
                    WALL = Chars.Numbered.WALL;
                    WAY = Chars.Numbered.WAY;
                    START = Chars.Numbered.START;
                    END = Chars.Numbered.END;
                    VISITED = Chars.Numbered.VISITED;
                    PLAYER = Chars.Numbered.PLAYER;
                    INVALID = Chars.Numbered.INVALID;
                    break;
                case Format.UTF:
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

            try
            {
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
            }
            catch
            {
                throw new Exception("Cannot delete existing file");
            }

            if (f == Format.ASCII || f == Format.CSV || f == Format.Numbered || f == Format.UTF)
            {
                StreamWriter SW;
                try
                {
                    SW = new StreamWriter(File.OpenWrite(FileName));
                }
                catch
                {
                    throw new Exception("cannot create output file: " + FileName);
                }

                for (y = 0; y < h; y++)
                {
                    for (x = 0; x < w; x++)
                    {
                        switch (maze[x, y])
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
                                if (f == Format.UTF)
                                {
                                    SW.Write(getFancyTile(maze, x, y, Chars.Corners.Unknown));
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
                        if (x < w - 1 && f == Format.CSV)
                        {
                            SW.Write(',');
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
            else if (f == Format.Binary)
            {
                WriteBinary(FileName, maze);
            }
            else if (f == Format.Image)
            {
                WriteImage(FileName, maze, scale);
            }
        }

        /// <summary>
        /// writes simplified binary output (only ways and walls)
        /// </summary>
        /// <param name="FileName">file name</param>
        /// <param name="maze">maze</param>
        private static void WriteBinary(string FileName, byte[,] maze)
        {
            int w, h, x, y;
            w = maze.GetLength(0);
            h = maze.GetLength(1);

            bool[] values = new bool[w * h];

            Console.Error.WriteLine("converting maze to binary...");
            for (y = 0; y < h; y++)
            {
                for (x = 0; x < w; x++)
                {
                    values[x + y * w] = maze[x, y] == Maze.CellType.WALL;
                }
            }

            byte[] v = ToByteArray(new BitArray(values));

            Console.Error.WriteLine("Writing binary file...");
            FileStream FS;
            try
            {
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
                FS = File.OpenWrite(FileName);
            }
            catch
            {
                throw new Exception("cannot create binary file");
            }
            FS.Write(BitConverter.GetBytes(w), 0, 4);
            FS.Write(BitConverter.GetBytes(h), 0, 4);
            FS.Write(v, 0, v.Length);
            FS.Close();
        }

        /// <summary>
        /// Converts bits to bytes
        /// </summary>
        /// <param name="bits">bits</param>
        /// <returns>byte array</returns>
        private static byte[] ToByteArray(BitArray bits)
        {
            /*
            byte[] retValue = new byte[bits.Count];
            for (int i = 0; i < bits.Count; i++)
            {
                retValue[i] = bits[i] ? Maze.CellType.WALL : Maze.CellType.WAY;
            }

            return retValue;
            //*/
            //*
            int numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0) numBytes++;

            byte[] bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << bitIndex);

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
            //*/
        }

        /// <summary>
        /// writes the maze to an image
        /// </summary>
        /// <param name="FileName">file name</param>
        /// <param name="maze">maze</param>
        /// <param name="scale">scale</param>
        private static void WriteImage(string FileName, byte[,] maze, int scale)
        {
            Console.Error.WriteLine("Generating Image");

            Bitmap B = new Bitmap(maze.GetLength(0), maze.GetLength(1), PixelFormat.Format16bppRgb565);

            for (int y = 0; y < maze.GetLength(1); y++)
            {
                Console.Error.WriteLine("Drawing Row {0}...", y + 1);
                for (int x = 0; x < maze.GetLength(0); x++)
                {
                    switch (maze[x, y])
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

            if (scale > 1)
            {
                Console.Error.WriteLine("resizing...");
                Bitmap Dest = new Bitmap(B.Width * scale, B.Height * scale, B.PixelFormat);
                Graphics G = Graphics.FromImage(Dest);

                G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                G.DrawImage(B, 0+scale/2, 0+scale/2, Dest.Width, Dest.Height);

                G.Dispose();

                try
                {
                    Dest.Save(FileName, ImageFormat.Png);
                }
                catch
                {
                    Console.Error.WriteLine("Cannot save resized image. Probably too big");
                    Console.Error.WriteLine("Try to save unscaled image...");
                    try
                    {
                        B.Save(FileName, ImageFormat.Png);
                    }
                    catch
                    {
                        Console.Error.WriteLine("cannot save original image. Save binary output...");
                        WriteBinary(FileName + ".bin", maze);
                        Console.Error.WriteLine("File saved as {0}.bin",FileName);
                    }
                }

                B.Dispose();
                Dest.Dispose();
            }
            else
            {
                try
                {
                    B.Save(FileName, ImageFormat.Png);
                }
                catch
                {
                    Console.Error.WriteLine("cannot save original image. Save binary output...");
                    WriteBinary(FileName + ".bin", maze);
                    Console.Error.WriteLine("File saved as {0}.bin", FileName);
                }
                B.Dispose();
            }
            Console.WriteLine("Done");
        }

        /// <summary>
        /// converts file input to a usable maze
        /// </summary>
        /// <param name="p">readed data from text file</param>
        /// <param name="format">input data format</param>
        /// <param name="scale">scale factor (images only)</param>
        /// <returns>maze</returns>
        private static byte[,] parseFile(string p, Format format,int scale)
        {
            Format f = format;
            if (f == Format.Autodetect)
            {
                switch (p.Substring(p.LastIndexOf('.') + 1).ToLower())
                {
                    case "txt":
                        switch (getFirst(p))
                        {
                            case Chars.Numbered.WALL:
                                f = Format.Numbered;
                                break;
                            case Chars.UTF.WALL:
                                f = Format.UTF;
                                break;
                            case Chars.ASCII.WALL:
                                f = Format.ASCII;
                                break;
                            default:
                                throw new Exception("Autodetect failed: text file of unknown format");
                        }
                        break;
                    case "csv":
                        f = Format.CSV;
                        break;
                    case "bin":
                        f = Format.Binary;
                        break;
                    case "png":
                        f = Format.Image;
                        break;
                    default:
                        throw new Exception("unidentified file type: " + p.Substring(p.LastIndexOf('.') + 1).ToLower());
                }
            }

            string s = null;

            switch (f)
            {
                case Format.Binary:
                    s = FromBinary(p);
                    break;
                case Format.Image:
                    s = FromImage(p,scale);
                    break;
                case Format.CSV:
                    s = File.ReadAllText(p)
                        .Replace(",", "")
                        .Replace(";", "")
                        .Replace("\t", "");
                    break;
                case Format.ASCII:
                    s = File.ReadAllText(p)
                        .Replace(Chars.ASCII.WALL,Chars.Numbered.WALL)
                        .Replace(Chars.ASCII.WAY, Chars.Numbered.WAY)
                        .Replace(Chars.ASCII.START, Chars.Numbered.START)
                        .Replace(Chars.ASCII.END, Chars.Numbered.END)
                        .Replace(Chars.ASCII.VISITED, Chars.Numbered.VISITED)
                        .Replace(Chars.ASCII.PLAYER, Chars.Numbered.PLAYER);
                    break;
                case Format.Numbered:
                    s = File.ReadAllText(p);
                    break;
                case Format.UTF:
                    s = File.ReadAllText(p)
                        .Replace(Chars.UTF.WALL, Chars.Numbered.WALL)
                        .Replace(Chars.UTF.WAY, Chars.Numbered.WAY)
                        .Replace(Chars.UTF.START, Chars.Numbered.START)
                        .Replace(Chars.UTF.END, Chars.Numbered.END)
                        .Replace(Chars.UTF.VISITED, Chars.Numbered.VISITED)
                        .Replace(Chars.UTF.PLAYER, Chars.Numbered.PLAYER)
                        .Replace(Chars.Corners.EW, Chars.Numbered.VISITED)
                        .Replace(Chars.Corners.NE, Chars.Numbered.VISITED)
                        .Replace(Chars.Corners.NES, Chars.Numbered.VISITED)
                        .Replace(Chars.Corners.NESW, Chars.Numbered.VISITED)
                        .Replace(Chars.Corners.NEW, Chars.Numbered.VISITED)
                        .Replace(Chars.Corners.NS, Chars.Numbered.VISITED)
                        .Replace(Chars.Corners.NW, Chars.Numbered.VISITED)
                        .Replace(Chars.Corners.NWS, Chars.Numbered.VISITED)
                        .Replace(Chars.Corners.SE, Chars.Numbered.VISITED)
                        .Replace(Chars.Corners.SEW, Chars.Numbered.VISITED)
                        .Replace(Chars.Corners.SW, Chars.Numbered.VISITED)
                        .Replace(Chars.Corners.Unknown, Chars.Numbered.VISITED);
                    break;
                default:
                    throw new Exception("unknown input format");
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
                        case Chars.Numbered.WAY:
                            retValue[x, y] = Maze.CellType.WAY;
                            break;
                        case Chars.Numbered.WALL:
                            retValue[x, y] = Maze.CellType.WALL;
                            break;
                        case Chars.Numbered.START:
                            retValue[x, y] = Maze.CellType.START;
                            break;
                        case Chars.Numbered.END:
                            retValue[x, y] = Maze.CellType.END;
                            break;
                        case Chars.Numbered.VISITED:
                            retValue[x, y] = Maze.CellType.VISITED;
                            break;
                        case Chars.Numbered.PLAYER:
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
        /// <returns>fle input string</returns>
        private static string FromImage(string p,int scale)
        {
            Console.Error.WriteLine("Opening image...");
            Bitmap B;
            try
            {
                B = (Bitmap)Image.FromFile(p);
            }
            catch
            {
                throw new Exception("Invalid image file: " + p);
            }

            StringBuilder SB = new StringBuilder((B.Width / scale) * (B.Height / scale) + (B.Height / scale * 2));

            for (int h = 0; h < B.Height; h += scale)
            {
                if ((h / scale + 1) % 10 == 0)
                {
                    Console.Error.WriteLine("Reading row {0}...", h + 1);
                }
                for (int w = 0; w < B.Width; w += scale)
                {
                    switch (B.GetPixel(w, h).ToArgb())
                    {
                        case Colors.WALL:
                            SB.Append(Chars.Numbered.WALL);
                            break;
                        case Colors.WAY:
                            SB.Append(Chars.Numbered.WAY);
                            break;
                        case Colors.START:
                            SB.Append(Chars.Numbered.START);
                            break;
                        case Colors.END:
                            SB.Append(Chars.Numbered.END);
                            break;
                        case Colors.VISITED:
                            SB.Append(Chars.Numbered.VISITED);
                            break;
                        case Colors.PLAYER:
                            SB.Append(Chars.Numbered.PLAYER);
                            break;
                        default:
                            throw new Exception(string.Format("Invalid image color at {0},{1}", w, h));
                    }
                }
                SB.Append(CRLF);
            }
            B.Dispose();
            return SB.ToString().Trim();
        }

        /// <summary>
        /// reads a binary input file to a maze
        /// </summary>
        /// <param name="p">file name</param>
        /// <returns>file input string</returns>
        private static string FromBinary(string p)
        {
            FileStream FS;
            int w, h, r;
            try
            {
                FS = File.OpenRead(p);
            }
            catch
            {
                throw new Exception("shit happened when opening binary file");
            }

            if (FS.Length <= 8)
            {
                FS.Close();
                FS.Dispose();
                throw new Exception("invalid binary file header");
            }



            byte[] buffer = new byte[BUFFER];
            FS.Read(buffer, 0, 8);
            w = BitConverter.ToInt32(buffer, 0);
            h = BitConverter.ToInt32(buffer, 4);

            StringBuilder SB = new StringBuilder(w * h);

            //convert binary input into string
            do
            {
                r = FS.Read(buffer, 0, buffer.Length);
                if (r > 0)
                {
                    if (r < BUFFER)
                    {
                        Array.Resize<byte>(ref buffer, r);
                    }
                    BitArray B = new BitArray(buffer);
                    for (int i = 0; i < w * h; i++)
                    {
                        SB.Append(B[i] ? Chars.Numbered.WALL : Chars.Numbered.WAY);
                    }
                }
            } while (r > 0);


            //add line breaks for every row
            string s = SB.ToString();

            string[] parts = new string[h];
            for (int j = 0; j < h; j++)
            {
                parts[j] = s.Substring(j * w, w);
            }
            return string.Join(CRLF, parts);
        }

        /// <summary>
        /// reads the first char from a file
        /// </summary>
        /// <param name="FileName">file name</param>
        /// <returns>first char</returns>
        private static char getFirst(string FileName)
        {
            char INVALID = '\0';
            char[] buf = new char[] { INVALID };

            try
            {
                StreamReader SR = File.OpenText(FileName);
                SR.Read(buf, 0, 1);
                SR.Close();
                SR.Dispose();
                return buf[0];
            }
            catch
            {
            }
            return INVALID;
        }

        /// <summary>
        /// parses command line arguments to the CmdParams structure and verifies arguments
        /// </summary>
        /// <param name="args">arguments</param>
        /// <returns>verified arguments</returns>
        private static CmdParams ParseArgs(string[] args)
        {
            CmdParams P = new CmdParams();
            P.W = uneven(Console.WindowWidth - 1);
            P.H = uneven(Console.WindowHeight - 1);
            P.S = 1;
            P.outFormat = Format.UTF;
            P.inFormat = Format.Autodetect;
            P.outFile = null;
            P.inFile = null;
            P.solve = false;
            P.play = false;
            P.fow = false;
            P.map = false;
            P.OK = true;

            if (args.Length > 0)
            {
                if (args[0] == "/?")
                {
                    help();
                    P.OK = false;
                    return P;
                }
                else
                {
                    int temp = 0;
                    foreach (string arg in args)
                    {
                        if (arg.ToUpper() == "/S")
                        {
                            P.solve = true;
                        }
                        else if (arg.ToUpper() == "/G")
                        {
                            P.play = true;
                        }
                        else if (arg.ToUpper() == "/FOW")
                        {
                            P.fow = true;
                        }
                        else if (arg.ToUpper() == "/MAP")
                        {
                            P.map = true;
                        }
                        else if (arg.Length > 3)
                        {
                            switch (arg.ToUpper().Substring(0, 3))
                            {
                                case "/W:":
                                    if (int.TryParse(arg.Substring(3), out temp) && temp > 4)
                                    {
                                        P.W = uneven(temp);
                                    }
                                    else
                                    {
                                        Console.WriteLine("unsupported width argument. Must be at least 5");
                                        help();
                                        P.OK = false;
                                        return P;
                                    }
                                    break;
                                case "/H:":
                                    if (int.TryParse(arg.Substring(3), out temp) && temp > 4)
                                    {
                                        P.H = uneven(temp);
                                    }
                                    else
                                    {
                                        Console.WriteLine("unsupported height argument. Must be at least 5");
                                        help();
                                        P.OK = false;
                                        return P;
                                    }
                                    break;
                                case "/M:":
                                    if (int.TryParse(arg.Substring(3), out temp) && temp > 0)
                                    {
                                        P.S = temp;
                                    }
                                    else
                                    {
                                        Console.WriteLine("unsupported scale argument. Must be at least 1");
                                        help();
                                        P.OK = false;
                                        return P;
                                    }
                                    break;
                                case "/O:":
                                    if (getFormat(arg.Substring(3)) != Format.Invalid)
                                    {
                                        P.outFormat = getFormat(arg.Substring(3));
                                    }
                                    else
                                    {
                                        Console.WriteLine("Unsupported output format");
                                        help();
                                        P.OK = false;
                                        return P;
                                    }
                                    break;
                                case "/I:":
                                    if (getFormat(arg.Substring(3)) != Format.Invalid)
                                    {
                                        P.inFormat = getFormat(arg.Substring(3));
                                    }
                                    else
                                    {
                                        Console.WriteLine("Unsupported input format. Ommit argument for auto detect");
                                        help();
                                        P.OK = false;
                                        return P;
                                    }
                                    break;
                                case "/R:":
                                    if (File.Exists(arg.Substring(3)))
                                    {
                                        P.inFile = arg.Substring(3);
                                    }
                                    else
                                    {
                                        Console.WriteLine("input file not found: " + arg.Substring(3));
                                        help();
                                        P.OK = false;
                                        return P;
                                    }
                                    break;
                                case "/P:":
                                    if (IsValidFileName(arg.Substring(3)))
                                    {
                                        P.outFile = arg.Substring(3);
                                    }
                                    else
                                    {
                                        Console.WriteLine("cannot create output file: " + arg);
                                        help();
                                        P.OK = false;
                                        return P;
                                    }
                                    break;
                                default:
                                    Console.WriteLine("unsupported argument: " + arg);
                                    help();
                                    P.OK = false;
                                    return P;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid argument: " + arg);
                            P.OK = false;
                            help();
                            return P;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(P.outFile) && P.outFormat == Format.Autodetect)
            {
                Console.WriteLine("output file format not specified");
                P.OK = false;
                help();
                return P;
            }

            return P;
        }

        /// <summary>
        /// checks if a given file name is valid for writing
        /// </summary>
        /// <param name="FileName">file name</param>
        /// <returns>true, if valid</returns>
        private static bool IsValidFileName(string FileName)
        {
            if (File.Exists(FileName))
            {
                return true;
            }
            else
            {
                try
                {
                    File.Create(FileName).Close();
                    File.Delete(FileName);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        /// <summary>
        /// converts a string input to its format
        /// </summary>
        /// <param name="p">string input</param>
        /// <returns>format (Format.Invalid on errors)</returns>
        private static Format getFormat(string p)
        {
            if (p.ToUpper() != "INVALID")
            {
                try
                {
                    return (Format)Enum.Parse(typeof(Format), p, true);
                }
                catch
                {
                }
            }
            return Format.Invalid;
        }

        /// <summary>
        /// prints extended help
        /// </summary>
        private static void help()
        {
            Console.WriteLine(@"Maze Tools
mazeTools.exe [/W:<number>] [/H:<number>] [/M:<number>] [/S] [/O:<format>]
              [/I:<format>] [/G] [/FOW] [/R:<infile>] [/P:<outfile>] [/MAP]

/W:<number>   Width of the maze, if not specified, window width is used
/H:<number>   Height of the maze, if not specified, window height is used
/M:<number>   Output multiplication factor, 1 or bigger. Default: 1
              this only affects image inputs and outputs
/O:<format>   Output format (see below), defaults to UTF
/I:<format>   Input format (see below), defaults to UTF
/S            Solves the Maze
/G            Game: Allow the user to solve manually
/FOW          Fog of war: maze is invisible except for current player view
/MAP          Map: once uncovered tiles (using /FOW) will stay visible
/R:<infile>   input file, if not specified, a maze is generated using
              W and H arguments. If specified, W and H are ignored
/P:<outfile>  output file, if not specified, the console is used (stdout)

Mazes always start at the top left corner and end at the bottom right corner.
Loading a maze from file (except binary) will find start and end.
Mazes always have an uneven rows and columns. Supplied values are adjusted
in case they are even. (subtraction only)

Formats:      Numbered: Output consists of Numbers only (with line breaks):
                        0=way
                        1=wall
                        2=start
                        3=end
                        4=solution
                        5=player
              ASCII: Output in ascii printable chars. Each char is 1 byte.
                  (space)=way
                        #=wall
                        S=start
                        E=end
                        .=solution
                        !=Player
              UTF: similar to ASCII but with multibyte chars
                  (space)=way
                        █=wall
                        S=start
                        E=end
                          solution: Line drawing ASCII art
                        ☺=player
              CSV: Similar to numbered format, but comma separated
              Binary: Compressed binary, only ways and walls
              Image: PNG image of maze using Colors
                    black=wall
                    white=way
                    green=start
                      red=end
                   yellow=solution
                  magenta=player

if a format is not specified, it is detected by file extension:
UTF     =txt
ASCII   =txt
CSV     =csv
BINARY  =bin
Numbered=txt
Image   =png

Detecting multiple txt formats is done by looking at the first
char of a file, since there is a border around the maze, the
first char is always a wall. The Wall differs from numbered to
UTF to ASCII.

The /G switch:
The /G switch lets the user play the maze in the console after
all other switches are processed. Console output is disabled,
if /G is specified, /P can to be used to save the maze progress.
/FOW only works together with /G. /S does not works together with /G

The /FOW switch:
When specified, the maze region is invisible except for the players
field of view. This makes solving mazes a lot more complicated.
The drawn path you took is still saved and visible again, if you
backtrack a little. Does not affects save file.

The /MAP switch:
This switch only works together with /FOW. If specified, once
uncovered tiles stay visible, as if the player drew a map.
Does not affects save file.");
        }

        /// <summary>
        /// makes a number uneven by subtracting 1 if needed.
        /// </summary>
        /// <param name="p">possibly even number</param>
        /// <returns>uneven number</returns>
        private static int uneven(int p)
        {
            return p % 2 == 0 ? p - 1 : p;
        }
    }
}
