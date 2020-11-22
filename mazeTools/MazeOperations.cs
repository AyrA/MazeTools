using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mazeTools
{
    /// <summary>
    /// Contains functions that operate on mazes but have nothing to do with generation or solving
    /// </summary>
    public static class MazeOperations
    {
        /// <summary>
        /// updates the FOW field
        /// </summary>
        /// <param name="maze">maze</param>
        /// <param name="fow">fof of war</param>
        /// <param name="posX">Player X</param>
        /// <param name="posY">Player Y</param>
        public static void DoFOW(byte[,] MazeArea, bool[,] FOW, int PosX, int PosY)
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
        public static void SetTrue(bool[,] FOW, int X, int Y)
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
        public static void SetTile(byte[,] MazeArea, int PosX, int PosY, byte Tile)
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
        public static bool IsCorridor(byte[,] MazeArea, int PosX, int PosY, Maze.Dir Direction)
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
        /// Gets the currently visible fow region
        /// </summary>
        /// <param name="fow">fow</param>
        /// <param name="posX">center X</param>
        /// <param name="posY">center Y</param>
        /// <param name="w">maximum width</param>
        /// <param name="h">maximum height</param>
        /// <returns>fow subregion</returns>
        public static bool[,] GetRegion(bool[,] FOW, int PosX, int PosY, int W, int H)
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
        public static char GetFancyTile(byte[,] MazeArea, int X, int Y, char Unknown)
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
        public static byte[,] GetRegion(byte[,] MazeArea, int PosX, int PosY, int W, int H)
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
    }
}
