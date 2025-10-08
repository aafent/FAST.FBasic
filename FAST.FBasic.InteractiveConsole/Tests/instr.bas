REM Test program for  instr() functions
REM Category: Loops
REM 
REM --- FBASIC Program: Find Substring Position (INSTR) ---
REM This program demonstrates the use of the INSTR function
REM to find the starting position of a substring within a main string.
REM ----------------------
REM --- Define the strings ---
MainString = "THE QUICK BROWN FOX JUMPS OVER THE LAZY FOX"
SubString = "FOX"
StartPosition = 1

PRINT "Main String: "; 
print MainString
PRINT "Substring to find: "; 
print SubString
PRINT "Starting search at position: "; 
print StartPosition

REM --- Use the INSTR function ---
REM Result will hold the 1-based character position.
let p1 = instr(StartPosition, MainString, SubString)

PRINT "Result of INSTR is: "; 
print p1

REM --- Test a search starting later in the string ---
PRINT "---"
StartPosition = 25
PRINT "Starting search again at position: "; 
print StartPosition

Result2 = instr(StartPosition, MainString, SubString)

PRINT "Result of INSTR is: "; 
print Result2

END
