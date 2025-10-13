REM FBASIC 40x22 Random Maze Generator
REM
REM This program generates a 40x22 grid where each inner cell has a 60% 
REM chance of being an open path (' ') and a 40% chance of being a wall ('#').
REM It includes one Entrance (E) and one Exit (X).
REM
REM Wall character: #
REM Path character: (space)
REM
REM NOTE: This method generates a random pattern but does NOT guarantee a solvable path.

REM --- Configuration ---
let horizontal = 40 ' Horizontal size (Columns)
let vertical = 22 ' Vertical size (Rows)
let pathchance = 0.60 ' 60% probability that an inner cell is a path (' ')

REM --- Main Program ---

REM Leave 3 lines empty as requested
print ""
print ""
print ""

print "------------------------------------------------"
print "  40x22 RANDOM Maze Generator"
print "------------------------------------------------"
print "Entrance (E) at (Row 1, Col 2)"
print "Exit (X) at (Row 22, Col 39)"
print ""

gosub generateandprintmaze

halt

REM -------------------------------------
REM --- Drawing Subroutine ---
REM -------------------------------------

generateandprintmaze:
    for row = 1 to vertical
        let mazeline = ""
        
        for col = 1 to horizontal
            let char = ""

            REM --- 1. Handle Entrance and Exit ---
            
            REM Entrance Check: Row 1, Col 2
            if row = 1 and col = 2 then
                let char = "E" ' Entrance (Top Left area)
                goto nextcharcheck
            endif

            REM Exit Check: Row 22, Col 39
            if row = vertical and col = horizontal - 1 then
                let char = "X" ' Exit (Bottom Right area)
                goto nextcharcheck
            endif
            
            REM --- 2. Handle Outer Walls ---
            
            REM Check Top or Bottom Boundary
            if row = 1 or row = vertical then
                let char = "#" ' Fixed boundary walls (Top/Bottom)
                goto nextcharcheck
            endif

            REM Check Left or Right Boundary
            if col = 1 or col = horizontal then
                let char = "#" ' Fixed boundary walls (Left/Right)
                goto nextcharcheck
            endif
            
            REM --- 3. Handle Randomized Inner Content ---
            
            REM Generate a random number between 0 and 1
            let randval = rnd() 
            
            if randval < pathchance then
                let char = " " ' Path (60% chance)
            else
                let char = "#" ' Wall (40% chance)
            endif
            
            nextcharcheck:
            
            REM Concatenate the character to the current line string
            let mazeline = mazeline + char
        next col

        REM Print the completed line for the current row
        print mazeline
    next row
return
