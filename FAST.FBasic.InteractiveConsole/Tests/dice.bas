REM FBASIC Dice Roller with ASCII Art
REM
REM This program rolls a six-sided die, generates a random result (1-6),
REM and then draws the corresponding die face using ASCII characters.
REM
REM Uses GOSUB to manage the drawing logic.

REM --- Main Program ---

REM Leave 3 lines empty as requested
PRINT ""
PRINT ""
PRINT ""

PRINT "-----------------------------------"
PRINT "  Rolling a Six-Sided Die..."
PRINT "-----------------------------------"

REM Generate a random integer from 1 to 6
REM RND() returns a float from 0 to < 1.0
REM INT(RND() * 6) gives 0 to 5.
REM Adding 1 gives 1 to 6.
rollagain:
LET DieRoll = int(rnd() * 6) + 1

PRINT "The result is: " + str(DieRoll)
PRINT ""

REM Call the subroutine to draw the dice
GOSUB DrawDice

print "Enter to roll again or Q to quit: ";
input cmd
let cmd=left(ucase(cmd),1)

if cmd="Q" Then
   Halt
endif

goto rollagain



HALT

REM --- Subroutines ---

REM DrawDice: Determines which pattern to call based on DieRoll.
DrawDice:

    REM Print the top border of the square matrix
    PRINT "+-------+"

    REM Use GOSUB to draw the correct inner pattern
    IF DieRoll = 1 THEN 
        GOSUB DrawPattern1
    endif
    IF DieRoll = 2 THEN 
        GOSUB DrawPattern2
    endif
    IF DieRoll = 3 THEN 
        GOSUB DrawPattern3
    endif
    IF DieRoll = 4 THEN 
        GOSUB DrawPattern4
    endif
    IF DieRoll = 5 THEN 
        GOSUB DrawPattern5
    endif
    IF DieRoll = 6 THEN 
        GOSUB DrawPattern6
    endif

    REM Print the bottom border of the square matrix
    PRINT "+-------+"
    
RETURN

REM --- Individual Die Patterns (3x3 Matrix) ---
REM A dot is 'O', empty space is ' '

REM Pattern for 1: Center dot only
DrawPattern1:
    PRINT "|       |"
    PRINT "|   O   |"
    PRINT "|       |"
RETURN

REM Pattern for 2: Top-left and bottom-right dots
DrawPattern2:
    PRINT "| O     |"
    PRINT "|       |"
    PRINT "|     O |"
RETURN

REM Pattern for 3: Top-left, center, and bottom-right dots
DrawPattern3:
    PRINT "| O     |"
    PRINT "|   O   |"
    PRINT "|     O |"
RETURN

REM Pattern for 4: Four corner dots
DrawPattern4:
    PRINT "| O   O |"
    PRINT "|       |"
    PRINT "| O   O |"
RETURN

REM Pattern for 5: Four corner dots and a center dot
DrawPattern5:
    PRINT "| O   O |"
    PRINT "|   O   |"
    PRINT "| O   O |"
RETURN

REM Pattern for 6: Six dots (no center dot)
DrawPattern6:
    PRINT "| O   O |"
    PRINT "| O   O |"
    PRINT "| O   O |"
RETURN
