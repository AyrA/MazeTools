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
                //"/S",
                //"/W:10000",
                //"/H:10000",
                @"/R:..\Release\solved.png",
                @"/P:..\Release\Stupidly_Large_Maze.png"
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
                        M.CurrentMaze = ParseFile(P.inFile, P.inFormat, P.S);
                    }
                    catch (Exception ex)
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
                        M.Unsolve();
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
                else if (!P.play)
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
        private static void PlayMaze(Maze MazePlayer, string FileName, bool EnableFOW, bool EnableMAP)
        {
            byte[,] maze = MazePlayer.CurrentMaze;

            bool[,] fow = new bool[maze.GetLength(0), maze.GetLength(1)];

            for (int y = 0; y < fow.GetLength(1); y++)
            {
                for (int x = 0; x < fow.GetLength(0); x++)
                {
                    fow[x, y] = !EnableFOW;
                }
            }

            int posX = MazePlayer.Player[0], posY = MazePlayer.Player[1];
            int toX = MazePlayer.End[0], toY = MazePlayer.End[1];

            Console.Clear();
            Console.CursorTop += Console.WindowHeight - 1;
            Console.Write("↑→↓← - Move | [ESC] - Exit | [S] - Save");
            if (string.IsNullOrEmpty(FileName))
            {
                FileName = "game.txt";
            }
            ConsoleKeyInfo KI;
            do
            {
                if (EnableFOW)
                {
                    DoFOW(maze, fow, posX, posY);
                }
                DrawFrame(maze, fow, posX, posY);

                if (EnableFOW && !EnableMAP)
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
                                SetTile(maze, posX, posY, Maze.CellType.VISITED);
                                SetTile(maze, posX, --posY, Maze.CellType.PLAYER);
                            }
                        } while (IsCorridor(maze, posX, posY, Maze.Dir.N));
                        break;
                    case ConsoleKey.DownArrow:
                        do
                        {
                            if (maze[posX, posY + 1] != Maze.CellType.WALL)
                            {
                                SetTile(maze, posX, posY, Maze.CellType.VISITED);
                                SetTile(maze, posX, ++posY, Maze.CellType.PLAYER);
                            }
                        } while (IsCorridor(maze, posX, posY, Maze.Dir.S));
                        break;
                    case ConsoleKey.LeftArrow:
                        do
                        {
                            if (maze[posX - 1, posY] != Maze.CellType.WALL)
                            {
                                SetTile(maze, posX, posY, Maze.CellType.VISITED);
                                SetTile(maze, --posX, posY, Maze.CellType.PLAYER);
                            }
                        } while (IsCorridor(maze, posX, posY, Maze.Dir.W));
                        break;
                    case ConsoleKey.RightArrow:
                        do
                        {
                            if (maze[posX + 1, posY] != Maze.CellType.WALL)
                            {
                                SetTile(maze, posX, posY, Maze.CellType.VISITED);
                                SetTile(maze, ++posX, posY, Maze.CellType.PLAYER);
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
                    DoFOW(maze, fow, posX, posY);
                    DrawFrame(maze, fow, posX, posY);
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
        private static void DoFOW(byte[,] MazeArea, bool[,] FOW, int PosX, int PosY)
        {
            int x = 0;
            int y = 0;

            while (MazeArea[PosX + x, PosY + y] != Maze.CellType.WALL)
            {
                SetTrue(FOW, PosX + x, PosY + y);
                x++;
            }
            x = 0;
            while (MazeArea[PosX + x, PosY + y] != Maze.CellType.WALL)
            {
                SetTrue(FOW, PosX + x, PosY + y);
                x--;
            }
            x = 0;
            while (MazeArea[PosX + x, PosY + y] != Maze.CellType.WALL)
            {
                SetTrue(FOW, PosX + x, PosY + y);
                y++;
            }
            y = 0;
            while (MazeArea[PosX + x, PosY + y] != Maze.CellType.WALL)
            {
                SetTrue(FOW, PosX + x, PosY + y);
                y--;
            }
        }

        /// <summary>
        /// sets a specific FOW location and all surrounding elements to true
        /// </summary>
        /// <param name="fow">fow</param>
        /// <param name="x">x position of center</param>
        /// <param name="y">y position of center</param>
        private static void SetTrue(bool[,] FOW, int X, int Y)
        {
            FOW[X + 0, Y - 1] = true; //N
            FOW[X + 1, Y - 1] = true; //NE
            FOW[X + 1, Y + 0] = true; //E
            FOW[X + 1, Y + 1] = true; //SE
            FOW[X + 0, Y + 1] = true; //S
            FOW[X - 1, Y + 1] = true; //SW
            FOW[X - 1, Y + 0] = true; //W
            FOW[X - 1, Y - 1] = true; //NW
            FOW[X + 0, Y + 0] = true; //Center
        }

        /// <summary>
        /// Sets a tile to a specific value, protecting some tiles from changes
        /// </summary>
        /// <param name="maze">maze</param>
        /// <param name="posX">X position</param>
        /// <param name="posY">Y position</param>
        /// <param name="tile">new tile value</param>
        private static void SetTile(byte[,] MazeArea, int PosX, int PosY, byte Tile)
        {
            if (MazeArea[PosX, PosY] == Maze.CellType.WAY ||
                MazeArea[PosX, PosY] == Maze.CellType.PLAYER ||
                MazeArea[PosX, PosY] == Maze.CellType.VISITED)
            {
                MazeArea[PosX, PosY] = Tile;
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
        private static bool IsCorridor(byte[,] MazeArea, int PosX, int PosY, Maze.Dir Direction)
        {
            if (Direction == Maze.Dir.S || Direction == Maze.Dir.N)
            {
                return
                    MazeArea[PosX - 1, PosY] == Maze.CellType.WALL &&
                    MazeArea[PosX + 1, PosY] == Maze.CellType.WALL &&
                    MazeArea[PosX, PosY - 1] != Maze.CellType.WALL &&
                    MazeArea[PosX, PosY + 1] != Maze.CellType.WALL;
            }
            else
            {
                return
                    MazeArea[PosX - 1, PosY] != Maze.CellType.WALL &&
                    MazeArea[PosX + 1, PosY] != Maze.CellType.WALL &&
                    MazeArea[PosX, PosY - 1] == Maze.CellType.WALL &&
                    MazeArea[PosX, PosY + 1] == Maze.CellType.WALL;
            }
        }

        /// <summary>
        /// draws a part of the maze, fitting the console. Tries to center around posX and posY
        /// </summary>
        /// <param name="maze">maze</param>
        /// <param name="posX">X position of center (player)</param>
        /// <param name="posY">Y position of center (player)</param>
        private static void DrawFrame(byte[,] MazeArea, bool[,] FOW, int PosX, int PosY)
        {
            byte[,] region = GetRegion(MazeArea, PosX, PosY, Console.WindowWidth - 1, Console.WindowHeight - 1);
            bool[,] fowregion = GetRegion(FOW, PosX, PosY, Console.WindowWidth - 1, Console.WindowHeight - 1);
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
                                Console.Write(GetFancyTile(region, x, y, Chars.Corners.Unknown));
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
        private static bool[,] GetRegion(bool[,] FOW, int PosX, int PosY, int W, int H)
        {
            //fix w and h if needed
            if (W > FOW.GetLength(0))
            {
                W = FOW.GetLength(0);
            }
            if (H > FOW.GetLength(1))
            {
                H = FOW.GetLength(1);
            }

            bool[,] region = new bool[W, H];
            int x, y, Mx, My, Mw, Mh;

            Mx = My = 0;
            Mw = W;
            Mh = H;

            for (y = 0; y < H; y++)
            {
                for (x = 0; x < W; x++)
                {
                    region[x, y] = false;
                }
            }

            if (PosX < 0)
            {
                PosX = 0;
            }
            else if (PosX > FOW.GetLength(0) - 1)
            {
                PosX = FOW.GetLength(0) - 1;
            }
            if (PosY < 0)
            {
                PosY = 0;
            }
            else if (PosY > FOW.GetLength(1) - 1)
            {
                PosY = FOW.GetLength(1) - 1;
            }

            //try to center the player
            Mx = PosX - W / 2;
            My = PosY - H / 2;

            if (Mx < 0)
            {
                Mx = 0;
            }
            else if (Mx + W > FOW.GetLength(0))
            {
                Mx = FOW.GetLength(0) - W;
            }
            if (My < 0)
            {
                My = 0;
            }
            else if (My + H > FOW.GetLength(1))
            {
                My = FOW.GetLength(1) - H;
            }

            //Fill maze
            for (y = My; y - My < Mh && y - My < FOW.GetLength(1); y++)
            {
                for (x = Mx; x - Mx < Mw && x - Mx < FOW.GetLength(0); x++)
                {
                    region[x - Mx, y - My] = FOW[x, y];
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
        private static char GetFancyTile(byte[,] MazeArea, int X, int Y, char Unknown)
        {
            byte N, S, E, W;
            N = S = E = W = Maze.CellType.INVALID;

            if (Y == 0)
            {
                N = Maze.CellType.WAY;
                S = MazeArea[X, Y + 1];
            }
            else if (Y > MazeArea.GetLength(1) - 2)
            {
                S = Maze.CellType.WAY;
                N = MazeArea[X, Y - 1];
            }
            else
            {
                N = MazeArea[X, Y - 1];
                S = MazeArea[X, Y + 1];
            }
            if (X == 0)
            {
                W = Maze.CellType.WAY;
                E = MazeArea[X + 1, Y];
            }
            else if (X > MazeArea.GetLength(0) - 2)
            {
                E = Maze.CellType.WAY;
                W = MazeArea[X - 1, Y];
            }
            else
            {
                E = MazeArea[X + 1, Y];
                W = MazeArea[X - 1, Y];
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

            return Unknown;
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
        private static byte[,] GetRegion(byte[,] MazeArea, int PosX, int PosY, int W, int H)
        {
            //fix w and h if needed
            if (W > MazeArea.GetLength(0))
            {
                W = MazeArea.GetLength(0);
            }
            if (H > MazeArea.GetLength(1))
            {
                H = MazeArea.GetLength(1);
            }

            byte[,] region = new byte[W, H];
            int x, y, Mx, My, Mw, Mh;

            Mx = My = 0;
            Mw = W;
            Mh = H;

            for (y = 0; y < H; y++)
            {
                for (x = 0; x < W; x++)
                {
                    region[x, y] = Maze.CellType.WAY;
                }
            }

            if (PosX < 0)
            {
                PosX = 0;
            }
            else if (PosX > MazeArea.GetLength(0) - 1)
            {
                PosX = MazeArea.GetLength(0) - 1;
            }
            if (PosY < 0)
            {
                PosY = 0;
            }
            else if (PosY > MazeArea.GetLength(1) - 1)
            {
                PosY = MazeArea.GetLength(1) - 1;
            }

            //try to center the player
            Mx = PosX - W / 2;
            My = PosY - H / 2;

            if (Mx < 0)
            {
                Mx = 0;
            }
            else if (Mx + W > MazeArea.GetLength(0))
            {
                Mx = MazeArea.GetLength(0) - W;
            }
            if (My < 0)
            {
                My = 0;
            }
            else if (My + H > MazeArea.GetLength(1))
            {
                My = MazeArea.GetLength(1) - H;
            }

            //Fill maze
            for (y = My; y - My < Mh && y - My < MazeArea.GetLength(1); y++)
            {
                for (x = Mx; x - Mx < Mw && x - Mx < MazeArea.GetLength(0); x++)
                {
                    //overdraw start and end with player if he is there.
                    //this is not stored on the file. Loading the maze will set
                    //the player to start if he is not found
                    if (MazeArea[x, y] == Maze.CellType.START && PosX == x && PosY == y)
                    {
                        region[x - Mx, y - My] = Maze.CellType.PLAYER;
                    }
                    else if (MazeArea[x, y] == Maze.CellType.END && PosX == x && PosY == y)
                    {
                        region[x - Mx, y - My] = Maze.CellType.PLAYER;
                    }
                    else
                    {
                        region[x - Mx, y - My] = MazeArea[x, y];
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
        private static void WriteConsole(byte[,] MazeArea, Format FileFormat)
        {
            int w, h, x, y;
            w = MazeArea.GetLength(0);
            h = MazeArea.GetLength(1);

            char WALL, WAY, START, END, VISITED, PLAYER, INVALID;

            switch (FileFormat)
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
                            if (FileFormat == Format.UTF)
                            {
                                Console.Write(GetFancyTile(MazeArea, x, y, Chars.Corners.Unknown));
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
                    if (x < w - 1 && FileFormat == Format.CSV)
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
        private static void WriteFile(string FileName, byte[,] MazeArea, Format FileFormat, int Scale)
        {
            int w, h, x, y;
            w = MazeArea.GetLength(0);
            h = MazeArea.GetLength(1);

            char WALL, WAY, START, END, VISITED, PLAYER, INVALID;
            WALL = WAY = START = END = PLAYER = VISITED = INVALID = '\0';

            switch (FileFormat)
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

            if (FileFormat == Format.ASCII || FileFormat == Format.CSV || FileFormat == Format.Numbered || FileFormat == Format.UTF)
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
                                if (FileFormat == Format.UTF)
                                {
                                    SW.Write(GetFancyTile(MazeArea, x, y, Chars.Corners.Unknown));
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
                        if (x < w - 1 && FileFormat == Format.CSV)
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
            else if (FileFormat == Format.Binary)
            {
                WriteBinary(FileName, MazeArea);
            }
            else if (FileFormat == Format.Image)
            {
                WriteImage(FileName, MazeArea, Scale);
            }
        }

        /// <summary>
        /// writes simplified binary output (only ways and walls)
        /// </summary>
        /// <param name="FileName">file name</param>
        /// <param name="maze">maze</param>
        private static void WriteBinary(string FileName, byte[,] MazeArea)
        {
            int w, h, x, y;
            w = MazeArea.GetLength(0);
            h = MazeArea.GetLength(1);

            bool[] values = new bool[w * h];

            Console.Error.WriteLine("converting maze to binary...");
            for (y = 0; y < h; y++)
            {
                for (x = 0; x < w; x++)
                {
                    values[x + y * w] = MazeArea[x, y] == Maze.CellType.WALL;
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
        private static byte[] ToByteArray(BitArray Bits)
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
            int numBytes = Bits.Count / 8;
            if (Bits.Count % 8 != 0) numBytes++;

            byte[] bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < Bits.Count; i++)
            {
                if (Bits[i])
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
        private static void WriteImage(string FileName, byte[,] MazeArea, int Scale)
        {
            Console.Error.WriteLine("Generating Image");

            Bitmap B = new Bitmap(MazeArea.GetLength(0), MazeArea.GetLength(1), PixelFormat.Format16bppRgb565);

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
                Bitmap Dest = new Bitmap(B.Width * Scale, B.Height * Scale, B.PixelFormat);
                Graphics G = Graphics.FromImage(Dest);

                G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                G.DrawImage(B, 0 + Scale / 2, 0 + Scale / 2, Dest.Width, Dest.Height);

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
                        WriteBinary(FileName + ".bin", MazeArea);
                        Console.Error.WriteLine("File saved as {0}.bin", FileName);
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
                    WriteBinary(FileName + ".bin", MazeArea);
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
        private static byte[,] ParseFile(string FileName, Format FileFormat, int Scale)
        {
            Format f = FileFormat;
            if (f == Format.Autodetect)
            {
                f = DetectFormat(FileName);
                if (f == Format.Invalid)
                {
                    throw new Exception("Autodetect failed: text file of unknown format");
                }
            }

            string s = null;

            switch (f)
            {
                case Format.Binary:
                    s = FromBinary(FileName);
                    break;
                case Format.Image:
                    s = FromImage(FileName, Scale);
                    break;
                case Format.CSV:
                    s = File.ReadAllText(FileName)
                        .Replace(",", "")
                        .Replace(";", "")
                        .Replace("\t", "");
                    break;
                case Format.ASCII:
                    s = File.ReadAllText(FileName)
                        .Replace(Chars.ASCII.WALL, Chars.Numbered.WALL)
                        .Replace(Chars.ASCII.WAY, Chars.Numbered.WAY)
                        .Replace(Chars.ASCII.START, Chars.Numbered.START)
                        .Replace(Chars.ASCII.END, Chars.Numbered.END)
                        .Replace(Chars.ASCII.VISITED, Chars.Numbered.VISITED)
                        .Replace(Chars.ASCII.PLAYER, Chars.Numbered.PLAYER);
                    break;
                case Format.Numbered:
                    s = File.ReadAllText(FileName);
                    break;
                case Format.UTF:
                    s = File.ReadAllText(FileName)
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
        private static string FromImage(string FileName, int Scale)
        {
            Console.Error.WriteLine("Opening image...");
            Bitmap B;
            try
            {
                B = (Bitmap)Image.FromFile(FileName);
            }
            catch
            {
                throw new Exception("Invalid image file: " + FileName);
            }

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
        private static string FromBinary(string FileName)
        {
            FileStream FS;
            int w, h, r;
            try
            {
                FS = File.OpenRead(FileName);
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
        private static char GetFirst(string FileName)
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
        /// Detects the format from the extension and file content
        /// </summary>
        /// <param name="FileName">File name</param>
        /// <returns>Detected Format</returns>
        private static Format DetectFormat(string FileName)
        {
            string Ext = "";
            if (FileName.Contains(".") && FileName.LastIndexOf('.') > FileName.LastIndexOf('\\'))
            {
                Ext = FileName.ToLower().Substring(FileName.LastIndexOf('.') + 1);
            }
            switch (Ext)
            {
                case "bin":
                    return Format.Binary;
                case "png":
                    return Format.Image;
                case "csv":
                    return Format.CSV;
                case "txt":
                    if (File.Exists(FileName))
                    {
                        switch (GetFirst(FileName))
                        {
                            case Chars.Numbered.WALL:
                                return Format.Numbered;
                            case Chars.UTF.WALL:
                                return Format.UTF;
                            case Chars.ASCII.WALL:
                                return Format.ASCII;
                            default:
                                return Format.Invalid;
                        }
                    }
                    else
                    {
                        return Format.UTF;
                    }
                default:
                    Console.Error.WriteLine("Can't detect File format properly");
                    ShowHelp();
                    break;
            }
            return Format.Invalid;
        }

        /// <summary>
        /// parses command line arguments to the CmdParams structure and verifies arguments
        /// </summary>
        /// <param name="args">arguments</param>
        /// <returns>verified arguments</returns>
        private static CmdParams ParseArgs(string[] Args)
        {
            CmdParams P = new CmdParams();
            P.W = Uneven(Console.WindowWidth - 1);
            P.H = Uneven(Console.WindowHeight - 1);
            P.S = 1;
            P.outFormat = Format.Autodetect;
            P.inFormat = Format.Autodetect;
            P.outFile = null;
            P.inFile = null;
            P.solve = false;
            P.play = false;
            P.fow = false;
            P.map = false;
            P.OK = true;

            if (Args.Length > 0)
            {
                if (Args[0] == "/?")
                {
                    ShowHelp();
                    P.OK = false;
                    return P;
                }
                else
                {
                    int temp = 0;
                    foreach (string arg in Args)
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
                                        P.W = Uneven(temp);
                                    }
                                    else
                                    {
                                        Console.Error.WriteLine("unsupported width argument. Must be at least 5");
                                        ShowHelp();
                                        P.OK = false;
                                        return P;
                                    }
                                    break;
                                case "/H:":
                                    if (int.TryParse(arg.Substring(3), out temp) && temp > 4)
                                    {
                                        P.H = Uneven(temp);
                                    }
                                    else
                                    {
                                        Console.Error.WriteLine("unsupported height argument. Must be at least 5");
                                        ShowHelp();
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
                                        Console.Error.WriteLine("unsupported scale argument. Must be at least 1");
                                        ShowHelp();
                                        P.OK = false;
                                        return P;
                                    }
                                    break;
                                case "/O:":
                                    if (GetFormat(arg.Substring(3)) != Format.Invalid)
                                    {
                                        P.outFormat = GetFormat(arg.Substring(3));
                                    }
                                    else
                                    {
                                        Console.Error.WriteLine("Unsupported output format");
                                        ShowHelp();
                                        P.OK = false;
                                        return P;
                                    }
                                    break;
                                case "/I:":
                                    if (GetFormat(arg.Substring(3)) != Format.Invalid)
                                    {
                                        P.inFormat = GetFormat(arg.Substring(3));
                                    }
                                    else
                                    {
                                        Console.Error.WriteLine("Unsupported input format. Ommit argument for auto detect");
                                        ShowHelp();
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
                                        Console.Error.WriteLine("input file not found: " + arg.Substring(3));
                                        ShowHelp();
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
                                        ShowHelp();
                                        P.OK = false;
                                        return P;
                                    }
                                    break;
                                default:
                                    Console.WriteLine("unsupported argument: " + arg);
                                    ShowHelp();
                                    P.OK = false;
                                    return P;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid argument: " + arg);
                            P.OK = false;
                            ShowHelp();
                            return P;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(P.inFile) && P.inFormat == Format.Autodetect)
            {
                P.inFormat = DetectFormat(P.inFile);
                if (P.inFormat == Format.Invalid)
                {
                    Console.Error.WriteLine("Input file format not specified and not detectable");
                    P.OK = false;
                    ShowHelp();
                }
            }
            if (!string.IsNullOrEmpty(P.outFile) && P.outFormat == Format.Autodetect)
            {
                P.outFormat = DetectFormat(P.outFile);
                if (P.outFormat == Format.Invalid)
                {
                    Console.Error.WriteLine("Output file format not specified and not detectable");
                    P.OK = false;
                    ShowHelp();
                }
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
        private static Format GetFormat(string FormatString)
        {
            if (FormatString.ToUpper() != "INVALID")
            {
                try
                {
                    return (Format)Enum.Parse(typeof(Format), FormatString, true);
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
        private static void ShowHelp()
        {
            Console.Error.WriteLine(@"Maze Tools
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
        private static int Uneven(int Number)
        {
            return Number % 2 == 0 ? Number - 1 : Number;
        }
    }
}
