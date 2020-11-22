using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace mazeTools
{
    class Program
    {
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
                "/W", "1000",
                "/H", "1000",
                //"/R", @"..\Release\solved.png",
                "/O", @"R:\Stupidly_Large_Maze.png"
            };
#endif
            if (args.Length == 0 || args.Contains("/?"))
            {
                ShowHelp();
                return 0;
            }
            CmdParams P;
            try
            {
                P = new CmdParams(args);
            }
            catch (Exception ex)
            {
#if DEBUG
                throw;
#else
                Console.Error.WriteLine("Invalid arguments: {0}", ex.Message);
                return 1;
#endif
            }

            Maze M = new Maze();
            if (P.InFile != null)
            {
                Console.Error.WriteLine("Reading input File...");
                try
                {
                    M.CurrentMaze = IO.ParseFile(P.InFile, P.InFormat, P.Scale);
                }
                catch (Exception ex)
                {
#if DEBUG
                    throw;
#else
                    Console.Error.WriteLine("Error reading file: {0}", ex.Message);
                    return 1;
#endif
                }
            }
            else
            {
                Console.Error.WriteLine("Generating maze...");
                M.generate2(P.Width, P.Height);
            }
            if (!P.Play)
            {
                if (P.Solve)
                {
                    Console.Error.WriteLine("solving...");
                    M.Unsolve();
                    M.Solve();
                }
                else if(P.Clear)
                {
                    Console.Error.WriteLine("unsolving...");
                    M.Unsolve();
                }
            }

            if (P.OutFile != null)
            {
                Console.Error.WriteLine("generating File...");
                IO.WriteFile(P.OutFile, M.CurrentMaze, P.OutFormat, P.Scale);
            }
            else if (!P.Play)
            {
                Console.Clear();
                IO.WriteConsole(M.CurrentMaze, P.OutFormat);
            }

            if (P.Play)
            {
                PlayMaze(M, P);
            }
#if DEBUG
            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
#endif
            return 0;
        }

        /// <summary>
        /// allows the user to play a maze
        /// </summary>
        /// <param name="m">Maze</param>
        /// <param name="FileName">file name for progress save function</param>
        private static void PlayMaze(Maze MazePlayer, CmdParams Arguments)
        {
            byte[,] maze = MazePlayer.CurrentMaze;

            bool[,] fow = new bool[maze.GetLength(0), maze.GetLength(1)];

            for (int y = 0; y < fow.GetLength(1); y++)
            {
                for (int x = 0; x < fow.GetLength(0); x++)
                {
                    fow[x, y] = !Arguments.Fow;
                }
            }

            int posX = MazePlayer.Player[0], posY = MazePlayer.Player[1];
            int toX = MazePlayer.End[0], toY = MazePlayer.End[1];
            string FileName = string.IsNullOrEmpty(Arguments.OutFile) ? "game.txt" : Arguments.OutFile;
            
            Console.Clear();
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write("↑→↓← - Move | [ESC] - Exit | [S] - Save");
            ConsoleKey KI;
            do
            {
                if (Arguments.Fow)
                {
                    MazeOperations.DoFOW(maze, fow, posX, posY);
                }
                IO.DrawFrame(maze, fow, posX, posY);

                if (Arguments.Fow && !Arguments.Map)
                {
                    for (int y = 0; y < fow.GetLength(1); y++)
                    {
                        for (int x = 0; x < fow.GetLength(0); x++)
                        {
                            fow[x, y] = false;
                        }
                    }
                }

                KI = Console.ReadKey(true).Key;
                switch (KI)
                {
                    case ConsoleKey.UpArrow:
                        do
                        {
                            if (maze[posX, posY - 1] != Maze.CellType.WALL)
                            {
                                MazeOperations.SetTile(maze, posX, posY, Maze.CellType.VISITED);
                                MazeOperations.SetTile(maze, posX, --posY, Maze.CellType.PLAYER);
                            }
                        } while (MazeOperations.IsCorridor(maze, posX, posY, Maze.Dir.N));
                        break;
                    case ConsoleKey.DownArrow:
                        do
                        {
                            if (maze[posX, posY + 1] != Maze.CellType.WALL)
                            {
                                MazeOperations.SetTile(maze, posX, posY, Maze.CellType.VISITED);
                                MazeOperations.SetTile(maze, posX, ++posY, Maze.CellType.PLAYER);
                            }
                        } while (MazeOperations.IsCorridor(maze, posX, posY, Maze.Dir.S));
                        break;
                    case ConsoleKey.LeftArrow:
                        do
                        {
                            if (maze[posX - 1, posY] != Maze.CellType.WALL)
                            {
                                MazeOperations.SetTile(maze, posX, posY, Maze.CellType.VISITED);
                                MazeOperations.SetTile(maze, --posX, posY, Maze.CellType.PLAYER);
                            }
                        } while (MazeOperations.IsCorridor(maze, posX, posY, Maze.Dir.W));
                        break;
                    case ConsoleKey.RightArrow:
                        do
                        {
                            if (maze[posX + 1, posY] != Maze.CellType.WALL)
                            {
                                MazeOperations.SetTile(maze, posX, posY, Maze.CellType.VISITED);
                                MazeOperations.SetTile(maze, ++posX, posY, Maze.CellType.PLAYER);
                            }
                        } while (MazeOperations.IsCorridor(maze, posX, posY, Maze.Dir.E));
                        break;
                    case ConsoleKey.S:
                        IO.WriteFile(FileName, maze, FileFormat.UTF, 1);
                        break;
                    default:
                        break;
                }
                if (posX == toX && posY == toY)
                {
                    MazeOperations.DoFOW(maze, fow, posX, posY);
                    IO.DrawFrame(maze, fow, posX, posY);
                    return;
                }
            } while (KI != ConsoleKey.Escape);
        }

        /// <summary>
        /// prints extended help
        /// </summary>
        private static void ShowHelp()
        {
            Console.Error.WriteLine(@"Maze Tools
mazeTools.exe [/W <number>] [/H <number>] [/M <number>]
              [/S] [/U] [/G] [/FOW] [/MAP]
              [/IF <format>] [/OF <format>] [/I <file>] [/O <file>]

/W <number>    Width of the maze, if not specified, window width is used
/H <number>    Height of the maze, if not specified, window height is used
/M <number>    Output multiplication factor, 1 or bigger. Default: 1
               this only affects image inputs and outputs
/OF <format>   Output format (see below), defaults to UTF-8
/IF <format>   Input format (see below), defaults to UTF-8
/S             Solves the Maze
/C             Clears a maze
/G             Game: Allow the user to solve manually
/FOW           Fog of war: maze is invisible except for current player view
/MAP           Map: once uncovered tiles (using /FOW) will stay visible
/I <file>      input file, if not specified, a maze is generated using
               W and H arguments.
/O <file>      output file, if not specified, the console is used (stdout)

Mazes always start at the top left corner and end at the bottom right corner,
but they don't have to. The generator that is in use will create mazes
where any two points will always have exactly one path that connects them.
Loading a maze from file will find start and end.
Mazes always have an uneven rows and columns. Supplied values are adjusted
in case they are even. (subtraction only)

/IF and /OF
Input and output file can be identical.
This can be used to (un)solve a maze, or to load and save when using /G.

Formats:      ASCII: Output in ascii printable chars. Each char is 1 byte.
                  (space)=way
                        #=wall
                        S=start
                        E=end
                        .=solution
                        !=Player
              UTF: similar to ASCII but with UTF-8 characters
                  (space)=way
                        █=wall
                        S=start
                        E=end
                          solution: Line drawing ASCII art
                        ☺=player
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
Image   =png

Detecting multiple txt formats is done by looking at the first
char of a file, since there is a border around the maze, the
first char is always a wall.

The /G switch:
The /G switch lets the user play the maze in the console after
all other switches are processed. Console output is disabled.
Specifying an output file saves the progress.
Specifying an input file loads a maze.
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
    }
}
