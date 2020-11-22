using System;
using System.Collections.Generic;
using System.Text;

namespace mazeTools
{
    /// <summary>
    /// Provides maze generation and solving functionalities
    /// </summary>
    public class Maze
    {
        /// <summary>
        /// Possible values for cells
        /// </summary>
        public struct CellType
        {
            /// <summary>
            /// way
            /// </summary>
            public const byte WAY = 0;
            /// <summary>
            /// wall
            /// </summary>
            public const byte WALL = 1;
            /// <summary>
            /// starting point
            /// </summary>
            public const byte START = 2;
            /// <summary>
            /// ending point
            /// </summary>
            public const byte END = 3;
            /// <summary>
            /// visited cell
            /// </summary>
            public const byte VISITED = 4;
            /// <summary>
            /// player cell
            /// </summary>
            public const byte PLAYER = 5;
            /// <summary>
            /// invalid cell
            /// </summary>
            public const byte INVALID = 6;
        }

        private class Cell
        {
            public int X, Y;
            public byte Z;

            public Cell(int x, int y, byte z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        /// <summary>
        /// Directions for player to walk
        /// </summary>
        public enum Dir : int
        {
            N = 0,
            S = 1,
            E = 2,
            W = 3
        }

        private byte[,] Z;
        private Cell[] NSEW;
        private Random R;
        private Cell StartPos;
        private Cell EndPos;
        private Cell PlayerPos;

        /// <summary>
        /// gets the seed used to generate the maze
        /// </summary>
        public int Seed
        { get; private set; }

        /// <summary>
        /// Start position
        /// </summary>
        public int[] Start
        {
            get
            {
                return new int[] { StartPos.X, StartPos.Y };
            }
            set
            {
                if (value != null &&
                    value.Length == 2 &&
                    value[0] > 0 && value[0] < Z.GetLength(0) - 1 &&
                    value[1] > 0 && value[1] < Z.GetLength(1) - 1)
                {
                    StartPos = new Cell(value[0], value[1], CellType.START);
                }
                else
                {
                    throw new ArgumentException("Expecting array with 2 values which are inside the maze");
                }
            }
        }

        /// <summary>
        /// End position
        /// </summary>
        public int[] End
        {
            get
            {
                return new int[] { EndPos.X, EndPos.Y };
            }
            set
            {
                if (value != null &&
                    value.Length == 2 &&
                    value[0] > 0 && value[0] < Z.GetLength(0) - 1 &&
                    value[1] > 0 && value[1] < Z.GetLength(1) - 1)
                {
                    EndPos = new Cell(value[0], value[1], CellType.END);
                }
                else
                {
                    throw new ArgumentException("Expecting array with 2 values which are inside the maze");
                }
            }
        }

        /// <summary>
        /// player position
        /// </summary>
        public int[] Player
        {
            get
            {
                return new int[] { PlayerPos.X, PlayerPos.Y };
            }
            set
            {
                if (value != null &&
                    value.Length == 2 &&
                    value[0] > 0 && value[0] < Z.GetLength(0) - 1 &&
                    value[1] > 0 && value[1] < Z.GetLength(1) - 1)
                {
                    PlayerPos = new Cell(value[0], value[1], CellType.PLAYER);
                }
                else
                {
                    throw new ArgumentException("Expecting array with 2 values which are inside the maze");
                }
            }
        }

        /// <summary>
        /// initializes the maze class using current time as seed
        /// </summary>
        public Maze()
        {
            init((int)DateTime.Now.ToFileTimeUtc());
        }

        /// <summary>
        /// initializes the maze class using given seed
        /// </summary>
        /// <param name="seed">random generator seed</param>
        public Maze(int seed)
        {
            init(seed);
        }

        private void init(int seed)
        {
            Seed = seed;
            Z = new byte[0, 0];
            NSEW = new Cell[]
            {
                new Cell(0,1,CellType.WAY),
                new Cell(0,-1,CellType.WAY),
                new Cell(1,0,CellType.WAY),
                new Cell(-1,0,CellType.WAY)
            };
            R = new Random(seed);
        }

        /// <summary>
        /// Generates a maze
        /// </summary>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        /// <param name="compl">complexity</param>
        /// <param name="dens">density</param>
        public void generate1(int w, int h,double compl,double dens)
        {
            int i, j, x, y;

            int width = w / 2 * 2 + 1;
            int height = h / 2 * 2 + 1;
            double density = dens * (w / 2d) * (h / 2d);
            double complexity = compl * 5d * (double)(w + h);

            Z = new byte[w, h];

            R = new Random(Seed);

            //clear Cells
            for (i = 0; i < w; i++)
            {
                Z[i, 0] = Z[i, h - 1] = CellType.WALL;
            }
            for (i = 0; i < h; i++)
            {
                Z[0, i] = Z[w - 1, i] = CellType.WALL;
            }

            for (i = 0; i < density; i++)
            {
                x = R.Next(w / 2) * 2;
                y = R.Next(h / 2) * 2;

                Z[x, y] = CellType.WALL;


                for (j = 0; j < complexity; j++)
                {
                    List<Cell> Cells = new List<Cell>();
                    if (x > 1)
                    {
                        Cells.Add(new Cell(x - 2, y, CellType.WAY));
                    }
                    if (x < w - 2)
                    {
                        Cells.Add(new Cell(x + 2, y, CellType.WAY));
                    }
                    if (y > 1)
                    {
                        Cells.Add(new Cell(x, y - 2, CellType.WAY));
                    }
                    if (y < h - 2)
                    {
                        Cells.Add(new Cell(x, y + 2, CellType.WAY));
                    }
                    if (Cells.Count > 0)
                    {
                        Cell next = Cells[R.Next(Cells.Count)];
                        if (Z[next.X, next.Y] == CellType.WAY)
                        {
                            Z[next.X, next.Y] = CellType.WALL;
                            Z[next.X + (x - next.X) / 2, next.Y + (y - next.Y) / 2] = CellType.WALL;
                            x = next.X;
                            y = next.Y;
                        }
                    }
                    Cells.Clear();
                }
            }
            Z[1, 1] = CellType.START;
            Z[w - 2, h - 2] = CellType.END;
            StartPos = new Cell(1, 1, CellType.START);
            EndPos = new Cell(w - 2, h - 2, CellType.END);
            PlayerPos = new Cell(1, 1, CellType.PLAYER);
        }

        /// <summary>
        /// Generates a maze
        /// </summary>
        /// <param name="w">Width of maze</param>
        /// <param name="h">Height of maze</param>
        public void generate2(int w, int h)
        {
            Stack<Cell> Cells = new Stack<Cell>();
            List<Cell> neighbors = new List<Cell>();
            Cell current = new Cell(1, 1, CellType.WAY);
            Cells.Push(current);

            int i, j;
            ulong k, l;
            k = 0;
            l = 0;
            Z = new byte[w, h];

            R = new Random(Seed);

            //mark as taken
            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Z[i, j] = CellType.WALL;
                }
            }

            while (Cells.Count > 0)
            {
                Z[current.X, current.Y] = CellType.WAY;

                neighbors = GetValidNeighbors(current, w, h);
                if (neighbors.Count > 0)
                {
                    if (++k % 10000 == 0)
                    {

                        if (l != k / (ulong)w * 200 / (ulong)h)
                        {
                            l = k / (ulong)w * 200 / (ulong)h;
                            Console.Error.WriteLine("generating... {0}%", l);
                        }
                    }
                    //remember this tile, by putting it on the stack
                    Cells.Push(current);
                    //move on to a random of the neighboring tiles
                    current = neighbors[R.Next(neighbors.Count)];
                }
                else
                {
                    //if there were no neighbors to try, we are at a dead-CellType.END
                    //test to see if this dead CellType.END will be the furthest point in the maze
                    //UpdateFurthestPoint();
                    //toss this tile out 
                    //(thereby returning to a previous tile in the list to check).
                    current = Cells.Pop();
                }
            }
            Z[1, 1] = CellType.START;
            Z[w - 2, h - 2] = CellType.END;
            StartPos = new Cell(1, 1, CellType.START);
            EndPos = new Cell(w - 2, h - 2, CellType.END);
            PlayerPos = new Cell(1, 1, CellType.PLAYER);
            Console.Error.WriteLine("generating... 100%");
        }

        /// <summary>
        /// Clears the maze and its properties, except for the seed
        /// </summary>
        public void Clear()
        {
            Z = null;
            StartPos = null;
            EndPos = null;
            PlayerPos = null;
        }

        /// <summary>
        /// Solves the maze using the Start and End properties
        /// </summary>
        public void Solve()
        {
            Solve(StartPos.X, StartPos.Y, EndPos.X, EndPos.Y);
        }

        /// <summary>
        /// Solves the maze using given start and end points
        /// </summary>
        /// <param name="FromX">Start X</param>
        /// <param name="FromY">Start Y</param>
        /// <param name="ToX">End X</param>
        /// <param name="ToY">End Y</param>
        public void Solve(int FromX, int FromY, int ToX, int ToY)
        {
            int x, y, X, Y;
            ulong i, j;
            bool turn = false;

            i = 0;
            j = 0;
            X = ToX;
            Y = ToY;
            x = FromX;
            y = FromY;

            //Solve(x, y, X, Y);

            Dir D = Z[2, 1] == CellType.WAY ? Dir.E : Dir.S;

            while (x != X || y != Y)
            {
                if (!turn)
                {
                    if (Z[x, y] == CellType.WAY)
                    {
                        if (++i % 1000 == 0)
                        {
                            if (j != i / (ulong)X * 200 / (ulong)Y)
                            {
                                j = i / (ulong)X * 200 / (ulong)Y;
                                Console.Error.WriteLine("Solving... {0}%", j);
                            }
                        }
                        Z[x, y] = CellType.VISITED;
                    }
                    else if (Z[x, y] == CellType.VISITED)
                    {
                        switch (D)
                        {
                            case Dir.N:
                                if (Z[x, y - 1] == CellType.VISITED)
                                {
                                    Z[x, y] = CellType.WAY;
                                }
                                break;
                            case Dir.S:
                                if (Z[x, y + 1] == CellType.VISITED)
                                {
                                    Z[x, y] = CellType.WAY;
                                }
                                break;
                            case Dir.E:
                                if (Z[x + 1, y] == CellType.VISITED)
                                {
                                    Z[x, y] = CellType.WAY;
                                }
                                break;
                            case Dir.W:
                                if (Z[x - 1, y] == CellType.VISITED)
                                {
                                    Z[x, y] = CellType.WAY;
                                }
                                break;
                        }
                    }
                }
                turn = false;

                switch (D)
                {
                    case Dir.E:
                        x++;
                        if (Z[x, y + 1] != CellType.WALL)
                        {
                            D = Dir.S;
                        }
                        else if (Z[x + 1, y] == CellType.WALL && Z[x, y - 1] != CellType.WALL)
                        {
                            D = Dir.N;
                        }
                        else if (Z[x + 1, y] == CellType.WALL)
                        {
                            D = Dir.W;
                            turn = true;
                        }
                        break;
                    case Dir.N:
                        y--;
                        if (Z[x + 1, y] != CellType.WALL)
                        {
                            D = Dir.E;
                        }
                        else if (Z[x, y - 1] == CellType.WALL && Z[x - 1, y] != CellType.WALL)
                        {
                            D = Dir.W;
                        }
                        else if (Z[x, y - 1] == CellType.WALL)
                        {
                            D = Dir.S;
                            turn = true;
                        }
                        break;
                    case Dir.S:
                        y++;
                        if (Z[x - 1, y] != CellType.WALL)
                        {
                            D = Dir.W;
                        }
                        else if (Z[x, y + 1] == CellType.WALL && Z[x + 1, y] != CellType.WALL)
                        {
                            D = Dir.E;
                        }
                        else if (Z[x, y + 1] == CellType.WALL)
                        {
                            D = Dir.N;
                            turn = true;
                        }
                        break;
                    case Dir.W:
                        x--;
                        if (Z[x, y - 1] != CellType.WALL)
                        {
                            D = Dir.N;
                        }
                        else if (Z[x - 1, y] == CellType.WALL && Z[x, y + 1] != CellType.WALL)
                        {
                            D = Dir.S;
                        }
                        else if (Z[x - 1, y] == CellType.WALL)
                        {
                            D = Dir.E;
                            turn = true;
                        }
                        break;
                }
            }
            Console.Error.WriteLine("Solving... 100%");
        }

        /// <summary>
        /// Clears solving state. Converts visited cells to ways
        /// </summary>
        public void Unsolve()
        {
            for (int y = 0; y < Z.GetLength(1); y++)
            {
                for (int x = 0; x < Z.GetLength(0); x++)
                {
                    if (Z[x, y] == CellType.VISITED)
                    {
                        Z[x, y] = CellType.WAY;
                    }
                }
            }
        }

        private List<Cell> GetValidNeighbors(Cell centerTile, int w, int h)
        {
            List<Cell> validNeighbors = new List<Cell>();

            //Check all four directions around the tile
            foreach (var offset in NSEW)
            {
                //find the neighbor's position
                Cell toCheck = new Cell(centerTile.X + offset.X, centerTile.Y + offset.Y, CellType.WAY);

                //make sure the tile is not on both an even X-axis and an even Y-axis
                //to ensure we can get CellType.WALLs around all tunnels
                if (toCheck.X % 2 == CellType.WALL || toCheck.Y % 2 == CellType.WALL)
                {
                    //if the potential neighbor is unexcavated (==1)
                    //and still has three CellType.WALLs intact (new territory)
                    if (Z[toCheck.X, toCheck.Y] == CellType.WALL && HasThreeWallsIntact(toCheck, w, h))
                    {
                        //add the neighbor
                        validNeighbors.Add(toCheck);
                    }
                }
            }
            return validNeighbors;
        }

        private bool HasThreeWallsIntact(Cell toCheck, int w, int h)
        {
            int count = 0;
            foreach (var offset in NSEW)
            {
                //find the neighbor's position
                Cell neighborToCheck = new Cell(toCheck.X + offset.X, toCheck.Y + offset.Y, CellType.WAY);

                //make sure it is inside the maze, and it hasn't been dug out yet
                if (IsInside(neighborToCheck, w, h) && Z[neighborToCheck.X, neighborToCheck.Y] == CellType.WALL)
                {
                    count++;
                }
            }
            return count == 3;
        }

        private bool IsInside(Cell c, int Width, int Height)
        {
            return c.X >= 0 && c.Y >= 0 && c.X < Width && c.Y < Height;
        }

        /// <summary>
        /// Gets or sets the current maze. Setting clears the seed
        /// </summary>
        public byte[,] CurrentMaze
        {
            get
            {
                return (byte[,])Z.Clone();
            }
            set
            {
                if (value != null)
                {
                    StartPos = null;
                    EndPos = null;
                    PlayerPos = null;
                    Seed = 0;
                    for (int x = 0; x < value.GetLength(0); x++)
                    {
                        for (int y = 0; y < value.GetLength(1); y++)
                        {
                            if (value[x, y] < 0 || value[x, y] >= CellType.INVALID)
                            {
                                throw new ArgumentException(string.Format("Position {0},{1} contains invalid number {2}", x, y, value[x, y]));
                            }
                            else if (value[x, y] == CellType.START)
                            {
                                if (StartPos != null)
                                {
                                    throw new ArgumentException(string.Format("Secondary start position found at {0},{1}", x, y));
                                }
                                StartPos = new Cell(x, y, CellType.START);
                            }
                            else if (value[x, y] == CellType.END)
                            {
                                if (EndPos != null)
                                {
                                    throw new ArgumentException(string.Format("Secondary end position found at {0},{1}", x, y));
                                }
                                EndPos = new Cell(x, y, CellType.END);
                            }
                            else if (value[x, y] == CellType.PLAYER)
                            {
                                if (PlayerPos != null)
                                {
                                    throw new ArgumentException(string.Format("Secondary player position found at {0},{1}", x, y));
                                }
                                PlayerPos = new Cell(x, y, CellType.PLAYER);
                            }
                        }
                    }
                    Z = value;
                    if (StartPos == null)
                    {
                        StartPos = new Cell(1, 1, CellType.START);
                    }
                    if (EndPos == null)
                    {
                        EndPos = new Cell(value.GetLength(0) - 2, value.GetLength(1) - 2, CellType.END);
                    }
                    if (PlayerPos == null)
                    {
                        PlayerPos = new Cell(StartPos.X, StartPos.Y, CellType.PLAYER);
                    }
                }
                else
                {
                    throw new ArgumentNullException("Maze value is null");
                }
            }
        }
    }
}
