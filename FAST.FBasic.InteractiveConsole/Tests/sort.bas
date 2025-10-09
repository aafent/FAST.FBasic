REM FBASIC Program to Collect, Store, Sort, and Print Names
REM Sorting Algorithm is the Bubble Sort
REM This program uses a loop to collect user input into an SDATA collection,
REM implements a manual Bubble Sort algorithm to fulfill the sorting requirement,
REM and then prints the final sorted list.
REM
REM --- Assumptions for Sorting Algorithm ---
REM To implement a Bubble Sort, the following functions are assumed to be
REM available for SDATA collections (as they are standard in most BASIC arrays):
REM 1. Collection access by 1-based index: [names(i)].
REM 2. The ability to assign/set a value to a collection index: [names(i)] = value.
REM 3. A function to get the collection size: COUNT(names).
REM -----------------------------------------

let nameInput=""    ' Variable to hold the name entered by the user (String)
let tempName=""     ' Temporary variable needed for swapping during the sort (String)
let i=0             ' Loop counter 1 for Bubble Sort (Numeric)
let j=0             ' Loop counter 2 for Bubble Sort (Numeric)
let collectionSize=0' Holds the total number of names (Numeric)


REM --- Input Loop ---

print "Enter names. Type '.' to finish input."

let cnt= 0 ' Initialize count of names to 0
inputLoop:
   print "Name: "; ' Prompt with semicolon to suppress the newline
   input nameInput

   REM Check for end of input
   If nameInput = "." Then
        If cnt = 0 Then
            print "No names were entered. Exiting program."
            HALT
        endif
        GoTo endInput
   EndIf

   let cnt = cnt + 1 ' Increment count of names
   REM Store the name into the SDATA collection
   sdata names nameInput

   GoTo inputLoop

endInput:

REM --- Sorting Algorithm (Bubble Sort) ---

REM Get the size of the collection. (Assumption 3)
let collectionSize = SCNT("names") 
let i = 0 ' Initialize i to 0
print ""
print "Input complete. Starting sort process for "+collectionSize+" items..."

REM Outer loop: i from 1 to collectionSize - 1
iLoopStart:
   i = i + 1
   If i >= collectionSize Then
      GoTo iLoopEnd
   EndIf

   let j = 0 ' Reset inner loop counter to 0

   REM Inner loop: j from 1 to collectionSize - i
   jLoopStart:
      let j = j + 1
      
      REM Comparison boundary check
      If j > (collectionSize - i) Then
         GoTo jLoopEnd
      EndIf
      
      REM Compare adjacent elements: [names(j)] and [names(j+1)]
      REM String comparison (<, >) is assumed to work alphabetically.
      let nextJ = j+1

      print "I="+i+",  J="+j+",    NextJ="+nextJ+"  ";

      If SCI("names",j) > SCI("names",nextJ) Then
            REM Swap logic: (Assumption 1 & 2)
            let tempName = SCI("names",j)
            let value = SCI("names",nextJ)

            SSET names j value 
            SSET names nextJ tempName    
            print "T"
        ELSE
            print "F"
      EndIf

GoTo jLoopStart
   jLoopEnd:
   
   GoTo iLoopStart
iLoopEnd:

REM --- Output ---

print ""
print "Sorted Names List (Alphabetical):"
print "---------------------------------"

Foreach names
   print [names.Item]
EndForeach names

HALT ' End program execution
