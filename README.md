Maze Tools
==========
This Application provides Maze creation, playing and solving capabilities

Command line syntax:
--------------------
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
    Does not affects save file.

