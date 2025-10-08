REM TicTacToe FBASIC (No Arrays)
REM Category: Games
REM FBASIC Tic-Tac-Toe Game (Player 'X' vs. Computer 'O') ---
REM Follows strict constraints: No arrays, No CALL.
REM Uses individual variables (C1-C9) and GOSUB/RETURN/GOTO only.
REM Variables: 
rem PlayerTurn  1 for Player (X), 2 for Computer (O)
rem GlobalWinStatus  Holds the result of CheckWinLabel (0, 1, 2, or 3)
rem CellNumber Cell number input by user
rem MoveInput Raw string input
rem DrawCount Counter for draw logic
rem MarkIndex Loop counter for win check
rem I stands for Loop counter for win check
rem Mark is the Current mark being checked (X or O)
rem FoundMove as Counter for computer move loop

' --- Constants ---
LET PlayerMark = "X"
LET ComputerMark = "O"

' ----------------------------------------------------------------------
' --- MAIN GAME LOOP ---
' ----------------------------------------------------------------------

GOSUB InitBoardLabel

LET PlayerTurn = 1 ' Player X goes first

MainGameLoop:
rem clear screen
for tms=1 To 10
    print ""
next tms
GOSUB DrawBoardLabel

' Check for game end
GOSUB CheckWinLabel

' Win/Draw check logic using sequential IF blocks
IF GlobalWinStatus = 1 THEN
    PRINT "Player X wins! Congratulations!"
    HALT
ENDIF

IF GlobalWinStatus = 2 THEN
    PRINT "Computer O wins! Better luck next time."
    HALT
ENDIF

IF GlobalWinStatus = 3 THEN
    PRINT "It's a draw!"
    HALT
ENDIF

' Execute turn based on PlayerTurn flag
IF PlayerTurn = 1 THEN
    PRINT "Your turn (X)."
    GOSUB PlayerMoveLabel
    LET PlayerTurn = 2 ' Switch to Computer's turn

ENDIF

IF PlayerTurn = 2 THEN
    PRINT "Computer's turn (O)..."
    GOSUB ComputerMoveLabel
    LET PlayerTurn = 1 ' Switch back to Player's turn

ENDIF

GOTO MainGameLoop

PRINT ""
PRINT "Game over. Press ENTER to end."
INPUT dummy
HALT

' ----------------------------------------------------------------------
' --- BOARD & DISPLAY ROUTINES ---
' ----------------------------------------------------------------------

InitBoardLabel:
REM Initialize the 9 board cells with their corresponding number as a string.
LET C1 = "1" : LET C2 = "2" : LET C3 = "3"
LET C4 = "4" : LET C5 = "5" : LET C6 = "6"
LET C7 = "7" : LET C8 = "8" : LET C9 = "9"
RETURN

DrawBoardLabel:
PRINT "--- Tic-Tac-Toe ---"
PRINT "-------------------"

' Print Row 1 (Cells 1, 2, 3)
PRINT "   | "; 
print C1; 
print " | "; 
print C2; 
print " | "; 
print C3; 
print " |"
PRINT "-------------------"

' Print Row 2 (Cells 4, 5, 6)
PRINT "   | "; 
print C4; 
print " | "; 
print C5; 
print " | "; 
print C6; 
print " |"
PRINT "-------------------"

' Print Row 3 (Cells 7, 8, 9)
PRINT "   | "; 
print C7; 
print " | "; 
print C8; 
print " | "; 
print C9; 
print " |"
PRINT "-------------------"
PRINT ""
RETURN

' ----------------------------------------------------------------------
' --- PLAYER MOVE LOGIC ROUTINE ---
' ----------------------------------------------------------------------

PlayerMoveLabel:

PlayerMoveLoop:
PRINT "Enter cell number (1-9): ";
INPUT MoveInput

LET CellNumber = int(num(MoveInput))

' 1. Check if input is in range (1 to 9)
IF CellNumber < 1 OR CellNumber > 9 THEN
    PRINT "Invalid number. Must be between 1 and 9."
    GOTO TryAgainPlayer
ENDIF

' Use GOTO logic to jump to the correct cell check/assignment
IF CellNumber = 1 THEN 
    GOTO CheckC1
ENDIF
IF CellNumber = 2 THEN 
    GOTO CheckC2
ENDIF
IF CellNumber = 3 THEN 
    GOTO CheckC3
ENDIF
IF CellNumber = 4 THEN 
    GOTO CheckC4
ENDIF
IF CellNumber = 5 THEN 
    GOTO CheckC5
ENDIF
IF CellNumber = 6 THEN 
    GOTO CheckC6
ENDIF
IF CellNumber = 7 THEN 
    GOTO CheckC7
ENDIF
IF CellNumber = 8 THEN 
    GOTO CheckC8
ENDIF
IF CellNumber = 9 THEN 
    GOTO CheckC9
ENDIF
' Safety net in case of unexpected input
GOTO TryAgainPlayer 

CheckC1:
    IF C1 <> "1" THEN 
        GOSUB CellTaken
        GOTO TryAgainPlayer 
    ENDIF
    LET C1 = PlayerMark
    GOTO PlayerMoveExit

CheckC2:
    IF C2 <> "2" THEN 
        GOSUB CellTaken
        GOTO TryAgainPlayer 
    ENDIF
    LET C2 = PlayerMark
    GOTO PlayerMoveExit

CheckC3:
    IF C3 <> "3" THEN 
        GOSUB CellTaken
        GOTO TryAgainPlayer 
    ENDIF
    LET C3 = PlayerMark
    GOTO PlayerMoveExit

CheckC4:
    IF C4 <> "4" THEN 
        GOSUB CellTaken
        GOTO TryAgainPlayer 
    ENDIF
    LET C4 = PlayerMark
    GOTO PlayerMoveExit

CheckC5:
    IF C5 <> "5" THEN 
        GOSUB CellTaken
        GOTO TryAgainPlayer 
    ENDIF
    LET C5 = PlayerMark
    GOTO PlayerMoveExit

CheckC6:
    IF C6 <> "6" THEN 
        GOSUB CellTaken
        GOTO TryAgainPlayer 
    ENDIF
    LET C6 = PlayerMark
    GOTO PlayerMoveExit

CheckC7:
    IF C7 <> "7" THEN 
        GOSUB CellTaken
        GOTO TryAgainPlayer 
    ENDIF
    LET C7 = PlayerMark
    GOTO PlayerMoveExit

CheckC8:
    IF C8 <> "8" THEN 
        GOSUB CellTaken
        GOTO TryAgainPlayer 
    ENDIF
    LET C8 = PlayerMark
    GOTO PlayerMoveExit

CheckC9:
    IF C9 <> "9" THEN 
        GOSUB CellTaken
        GOTO TryAgainPlayer 
    ENDIF
    LET C9 = PlayerMark
    GOTO PlayerMoveExit

TryAgainPlayer:

GOTO PlayerMoveLoop

CellTaken:
PRINT "Cell "; 
PRINT CellNumber; 
PRINT " is already taken. Try again."
RETURN

PlayerMoveExit:
RETURN

' ----------------------------------------------------------------------
' --- COMPUTER MOVE LOGIC ROUTINE ---
' ----------------------------------------------------------------------

ComputerMoveLabel:
LET FoundMove = 0

ComputerMoveLoop:
LET CellNumber = int(rnd() * 9) + 1

' Check if the cell is empty (i.e., still contains a number)
IF CellNumber = 1 And C1 = "1" THEN 
    LET C1 = ComputerMark
    GOTO FoundMoveExit 
ENDIF
IF CellNumber = 2 And C2 = "2" THEN 
    LET C2 = ComputerMark
    GOTO FoundMoveExit 
ENDIF
IF CellNumber = 3 And C3 = "3" THEN 
    LET C3 = ComputerMark 
    GOTO FoundMoveExit 
ENDIF
IF CellNumber = 4 And C4 = "4" THEN 
    LET C4 = ComputerMark 
    GOTO FoundMoveExit 
ENDIF
IF CellNumber = 5 And C5 = "5" THEN 
    LET C5 = ComputerMark 
    GOTO FoundMoveExit 
ENDIF
IF CellNumber = 6 And C6 = "6" THEN 
    LET C6 = ComputerMark 
GOTO FoundMoveExit 
ENDIF
IF CellNumber = 7 And C7 = "7" THEN 
    LET C7 = ComputerMark 
    GOTO FoundMoveExit 
ENDIF
IF CellNumber = 8 And C8 = "8" THEN 
    LET C8 = ComputerMark 
    GOTO FoundMoveExit 
ENDIF
IF CellNumber = 9 And C9 = "9" THEN 
    LET C9 = ComputerMark 
    GOTO FoundMoveExit 
ENDIF

' Check for a draw condition to prevent infinite loops 
LET FoundMove = FoundMove + 1
IF FoundMove < 9 THEN 
    GOTO ComputerMoveLoop
endif

' Fallthrough if no move found (Draw is caught by CheckWinLabel)
GOTO ComputerMoveExit

FoundMoveExit:
PRINT "Computer has moved."

ComputerMoveExit:
RETURN

' ----------------------------------------------------------------------
' --- WIN/DRAW CHECK ROUTINE ---
' ----------------------------------------------------------------------

CheckWinLabel:
REM Sets the result in GlobalWinStatus: 0 (No win/No draw), 1 (Player X win), 2 (Computer O win), 3 (Draw)

LET GlobalWinStatus = 0 ' Default: Game is ongoing

' Loop through both marks (X then O)
FOR MarkIndex = 1 TO 2
IF MarkIndex = 1 THEN 
    LET Mark = PlayerMark 
ENDIF
IF MarkIndex = 2 THEN 
    LET Mark = ComputerMark 
ENDIF

' --- Check Winning Combinations ---

' Rows (1-2-3, 4-5-6, 7-8-9)
IF C1 = Mark And C2 = Mark And C3 = Mark THEN 
    GOTO WinnerFound 
ENDIF
IF C4 = Mark And C5 = Mark And C6 = Mark THEN 
    GOTO WinnerFound 
ENDIF
IF C7 = Mark And C8 = Mark And C9 = Mark THEN 
    GOTO WinnerFound 
ENDIF

' Columns (1-4-7, 2-5-8, 3-6-9)
IF C1 = Mark And C4 = Mark And C7 = Mark THEN 
    GOTO WinnerFound 
ENDIF
IF C2 = Mark And C5 = Mark And C8 = Mark THEN 
    GOTO WinnerFound 
ENDIF
IF C3 = Mark And C6 = Mark And C9 = Mark THEN 
    GOTO WinnerFound 
ENDIF

' Diagonals (1-5-9, 3-5-7)
IF C1 = Mark And C5 = Mark And C9 = Mark THEN 
    GOTO WinnerFound 
ENDIF
IF C3 = Mark And C5 = Mark And C7 = Mark THEN 
    GOTO WinnerFound 
ENDIF

GOTO NextMarkCheck ' Skip to the next mark check

WinnerFound:
LET GlobalWinStatus = MarkIndex ' Set 1 for X win, 2 for O win

GOTO CheckWinExit ' Exit the routine immediately

NextMarkCheck:

NEXT MarkIndex

' --- Check for Draw (Board Full) ---
LET DrawCount = 0

' Check if each cell is marked (i.e., not a number 1-9)
IF C1 <> "1" THEN 
    LET DrawCount = DrawCount + 1 
ENDIF
IF C2 <> "2" THEN 
    LET DrawCount = DrawCount + 1 
ENDIF
IF C3 <> "3" THEN 
    LET DrawCount = DrawCount + 1 
ENDIF
IF C4 <> "4" THEN 
    LET DrawCount = DrawCount + 1 
ENDIF
IF C5 <> "5" THEN 
    LET DrawCount = DrawCount + 1 
ENDIF
IF C6 <> "6" THEN 
    LET DrawCount = DrawCount + 1 
ENDIF
IF C7 <> "7" THEN 
    LET DrawCount = DrawCount + 1 
ENDIF
IF C8 <> "8" THEN 
    LET DrawCount = DrawCount + 1 
ENDIF
IF C9 <> "9" THEN 
    LET DrawCount = DrawCount + 1 
ENDIF

IF DrawCount = 9 THEN
    LET GlobalWinStatus = 3 ' Set 3 for a draw

ENDIF

CheckWinExit:
RETURN


End