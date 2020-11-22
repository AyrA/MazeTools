
# Maze Tools

This Application provides Maze creation, playing and solving capabilities

## Command Line

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

## Maze Layout

Generated mazes start at the top left corner and end at the bottom right corner.
The generator that is in use will create mazes where any two points will always have exactly one path that connects them.
Loading a maze from file will find start and end.
Mazes always have an uneven number of rows and columns.
Supplied values are adjusted by subtraction in case they are even.

## Input and Output Formats

The applications supports multiple formats.
All formats can be used for input and output.
If not specified by the user, the format is automatically detected.
For the input file, the format is detected by reading a few bytes and examining them.
For the output format, the file extension is used.
`png` assumes an image file, all other extensions assume UTF-8 output.

### ASCII

Output in ASCII printable characters.
Each character is 1 byte.

| Maze     | Character |
|----------|-----------|
| Wall     | `#`       |
| Way      | space     |
| Start    | `S`       |
| End      | `E`       |
| Solution | `.`       |
| Player   | `!`       |

### UTF

Output in UTF-8 printable characters

| Maze     | Character |
|----------|-----------|
| Wall     | `█`       |
| Way      | space     |
| Start    | `S`       |
| End      | `E`       |
| Solution | Line art  |
| Player   | `☺`       |

The solution is drawn using line art characters from the box drawing set.

### Image

This is a PNG image of the maze.

| Maze     | Color   |
|----------|---------|
| Wall     | Black   |
| Way      | White   |
| Start    | Green   |
| End      | Red     |
| Solution | Yellow  |
| Player   | Magenta |

## Manual solving

The `/G` switch lets the user play the maze in the console after all other switches are processed. The player is moved using the arrow keys.
The application employs smart movement. It will move the player into the given direction until a crossing or corner is hit. Pressing the `S` key allows the user to save the progress.
Progress is saved as `game.txt` unless a custom output file name is specified.

### `/FOW`

**F**og **O**f **W**ar hides parts of the maze that the player can't see.
This makes solving mazes more difficult.
The path that the player has taken is still recorded and revealed if a section is revisited.
Saving the maze will still save the entire maze to file.

### `/MAP`

This switch only works together with /FOW.
Uncovered tiles stay visible if this argument is specified. (As if the player drew a map.)
Saving the maze will still save the entire maze to file.
