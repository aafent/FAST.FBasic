REM FBASIC Tower of Hanoi Game (4 Disks)
REM Uses PUSH/POP to simulate stacks for the three pegs.
REM Follows strict FBASIC conventions: no $, all lowercase functions, no ELSEIF.
REM All IF statements now use the standard multi-line block format.

PRINT "*****************************************************"
PRINT "THIS PROGRAM DOES NOT WORK FINE YET DUE TO LOGIC BUGS"
PRINT "*****************************************************"


REM --- Configuration ---
let numdisks = 4 ' Number of disks in the game
let maxheight = 5 ' Height of the drawing area (max disks + 1 for base)
let peg1name = "A" ' Source Peg name
let peg2name = "B" ' Auxiliary Peg name
let peg3name = "C" ' Destination Peg name
let moves = 0 ' Move counter
let gameover = 0 ' Flag for game end

REM Disk size strings for drawing
let disk4 = "=========" ' Largest disk (size 4)
let disk3 = "  =====  "   ' Size 3 disk
let disk2 = "   ===   "    ' Size 2 disk
let disk1 = "    =    "     ' Smallest disk (size 1)
let peg =   "    |    "       ' Peg vertical line
let base =  "---------" ' Tower base
let spacer= "         " ' Space between towers

REM --- Stack Simulation Variables ---
REM We use the global stack and then PUSH/POP to move items between local
REM storage variables that represent the state of the three pegs.

REM Stack variables for Peg A (Source)
let pegA1 = 0
let pegA2 = 0
let pegA3 = 0
let pegA4 = 0
let countA = 0

REM Stack variables for Peg B (Auxiliary)
let pegB1 = 0
let pegB2 = 0
let pegB3 = 0
let pegB4 = 0
let countB = 0

REM Stack variables for Peg C (Destination)
let pegC1 = 0
let pegC2 = 0
let pegC3 = 0
let pegC4 = 0
let countC = 0

REM --- Main Program ---
gosub initgame ' Set up the initial state (all disks on Peg A)

MainGameLoop:
    if gameover = 1 then
        goto gamewon
    endif

    REM Print 10 empty lines
    for i = 1 to 10
        print ""
    next i

    print "Tower of Hanoi: Moves: " + str(moves)
    gosub drawtowers ' Redraw the current state

    gosub getmoveinput ' Get user move

    if gamemovevalid = 0 then
        print "Invalid move! Try again."
        goto MainGameLoop
    endif

    gosub performmove ' Execute the valid move
    
    let moves = moves + 1

    gosub checkwin ' Check if the game is over

    goto MainGameLoop ' Loop back to the start

gamewon:
    print ""
    print "*****************************************************"
    print "CONGRATULATIONS! You solved the Tower of Hanoi!"
    print "Total moves: " + str(moves)
    print "*****************************************************"
    halt


REM -------------------------------------
REM --- Subroutines ---
REM -------------------------------------

initgame:
    REM Place all disks on Peg A (Largest disk is 4, smallest is 1)
    let pegA1 = 1
    let pegA2 = 2
    let pegA3 = 3
    let pegA4 = 4
    let countA = numdisks
    let countB = 0
    let countC = 0
return

checkwin:
    REM Win condition: All disks are on Peg C (countC = numdisks)
    if countC = numdisks then
        let gameover = 1
    endif
return

getmoveinput:
    print ""
    print "Enter move (e.g. AB for: A to C):";
    input moveinput
    
    let frompeg = ucase(mid(moveinput, 1, 1))
    let topeg = ucase(mid(moveinput, 2, 1))

    if frompeg="Q" Then
        print "Bye!"
        Halt
    endif

    gosub validatemove ' Check if the move is legal

    return

validatemove:
    let gamemovevalid = 0
    let fromcount = 0
    let tocount = 0

    ' Map peg name to count and value variables
    if frompeg = "A" then
        let fromcount = countA
        let fromtopdisk = pegA1
        let totopdisk = 0
        if topeg = "B" then
            let tocount = countB
            let totopdisk = pegB1
        endif
        if topeg = "C" then
            let tocount = countC
            let totopdisk = pegC1
        endif
    endif

    if frompeg = "B" then
        let fromcount = countB
        let fromtopdisk = pegB1
        let totopdisk = 0
        if topeg = "A" then
            let tocount = countA
            let totopdisk = pegA1
        endif
        if topeg = "C" then
            let tocount = countC
            let totopdisk = pegC1
        endif
    endif

    if frompeg = "C" then
        let fromcount = countC
        let fromtopdisk = pegC1
        let totopdisk = 0
        if topeg = "A" then
            let tocount = countA
            let totopdisk = pegA1
        endif
        if topeg = "B" then
            let tocount = countB
            let totopdisk = pegB1
        endif
    endif

    ' --- Validation Logic ---
    
    ' Rule 1: Cannot move from an empty peg
    if fromcount = 0 then
        print "Error: Source peg is empty."
        return
    endif

    ' Rule 2: Cannot place a larger disk onto a smaller disk
    if tocount > 0 then
        if fromtopdisk > totopdisk then
            print "Error: Cannot place a larger disk on a smaller one."
            return
        endif
    endif

    ' If we reach here, the move is valid
    let gamemovevalid = 1
    
    return

performmove:
    let movedisk = 0
    
    ' --- Step 1: POP the disk from the Source Peg (simulated) ---
    if frompeg = "A" then
        gosub popA
        let movedisk = diskval
    endif
    if frompeg = "B" then
        gosub popB
        let movedisk = diskval
    endif
    if frompeg = "C" then
        gosub popC
        let movedisk = diskval
    endif

    ' --- Step 2: PUSH the disk onto the Destination Peg (simulated) ---
    if topeg = "A" then
        let diskval = movedisk
        gosub pushA
    endif
    if topeg = "B" then
        let diskval = movedisk
        gosub pushB
    endif
    if topeg = "C" then
        let diskval = movedisk
        gosub pushC
    endif

    return

REM -------------------------------------
REM --- Stack Operations (POP) ---
REM -------------------------------------

popA:
    if countA = 0 then 
        let diskval = 0 ' Should not happen if validation is correct
        return
    endif

    if countA = 1 then
        let diskval = pegA1
        let pegA1 = 0
        let countA = countA - 1
        return
    endif
    if countA = 2 then
        let diskval = pegA2
        let pegA2 = 0
        let countA = countA - 1
        return
    endif
    if countA = 3 then
        let diskval = pegA3
        let pegA3 = 0
        let countA = countA - 1
        return
    endif
    if countA = 4 then
        let diskval = pegA4
        let pegA4 = 0
        let countA = countA - 1
        return
    endif
return

popB:
    if countB = 0 then 
        let diskval = 0 
        return
    endif

    if countB = 1 then
        let diskval = pegB1
        let pegB1 = 0
        let countB = countB - 1
        return
    endif
    if countB = 2 then
        let diskval = pegB2
        let pegB2 = 0
        let countB = countB - 1
        return
    endif
    if countB = 3 then
        let diskval = pegB3
        let pegB3 = 0
        let countB = countB - 1
        return
    endif
    if countB = 4 then
        let diskval = pegB4
        let pegB4 = 0
        let countB = countB - 1
        return
    endif
return

popC:
    if countC = 0 then 
        let diskval = 0 
        return
    endif

    if countC = 1 then
        let diskval = pegC1
        let pegC1 = 0
        let countC = countC - 1
        return
    endif
    if countC = 2 then
        let diskval = pegC2
        let pegC2 = 0
        let countC = countC - 1
        return
    endif
    if countC = 3 then
        let diskval = pegC3
        let pegC3 = 0
        let countC = countC - 1
        return
    endif
    if countC = 4 then
        let diskval = pegC4
        let pegC4 = 0
        let countC = countC - 1
        return
    endif
return

REM -------------------------------------
REM --- Stack Operations (PUSH) ---
REM -------------------------------------

pushA:
    if countA = numdisks then
        return ' Stack full (shouldn't happen in 4-disk game)
    endif

    let countA = countA + 1
    if countA = 1 then
        let pegA1 = diskval
        return
    endif
    if countA = 2 then
        let pegA2 = diskval
        return
    endif
    if countA = 3 then
        let pegA3 = diskval
        return
    endif
    if countA = 4 then
        let pegA4 = diskval
        return
    endif
return

pushB:
    if countB = numdisks then
        return 
    endif

    let countB = countB + 1
    if countB = 1 then
        let pegB1 = diskval
        return
    endif
    if countB = 2 then
        let pegB2 = diskval
        return
    endif
    if countB = 3 then
        let pegB3 = diskval
        return
    endif
    if countB = 4 then
        let pegB4 = diskval
        return
    endif
return

pushC:
    if countC = numdisks then
        return 
    endif

    let countC = countC + 1
    if countC = 1 then
        let pegC1 = diskval
        return
    endif
    if countC = 2 then
        let pegC2 = diskval
        return
    endif
    if countC = 3 then
        let pegC3 = diskval
        return
    endif
    if countC = 4 then
        let pegC4 = diskval
        return
    endif
return

REM -------------------------------------
REM --- Drawing Logic ---
REM -------------------------------------

drawtowers:
    REM We draw from the top of the tower (row 1) down to the base (row maxheight)
    for row = 1 to maxheight
        let printline = ""

        REM --- 1. Draw Peg A Disk or Space ---
        gosub getdiskforrowA ' Find disk size for this row
        gosub builddiskstring ' Convert size to string
        let printline = printline + diskstring + spacer

        REM --- 2. Draw Peg B Disk or Space ---
        gosub getdiskforrowB
        gosub builddiskstring
        let printline = printline + diskstring + spacer

        REM --- 3. Draw Peg C Disk or Space ---
        gosub getdiskforrowC
        gosub builddiskstring
        let printline = printline + diskstring
        
        print printline
    next row

    REM Print Peg Labels and Base
    print "  " + peg1name + base + spacer +"  "+ peg2name + base + spacer+ "  " + peg3name + base
    print ""

return

getdiskforrowA:
    let diskval = 0 ' Default is empty
    let diskindex = maxheight - row ' 4 for row 1 (top), 1 for row 4 (bottom disk)
    
    if diskindex <= 0 then
        let diskval = 0
        goto getdiskexitA ' Skip base row (index 0)
    endif

    if diskindex > countA then 
        let diskval = 0 ' No disk at this height
        goto getdiskexitA
    endif

    ' Find the disk value (size) at this height
    if diskindex = 1 then
        let diskval = pegA1
    endif
    if diskindex = 2 then
        let diskval = pegA2
    endif
    if diskindex = 3 then
        let diskval = pegA3
    endif
    if diskindex = 4 then
        let diskval = pegA4
    endif

getdiskexitA:
return

getdiskforrowB:
    let diskval = 0
    let diskindex = maxheight - row
    
    if diskindex <= 0 then
        let diskval = 0
        goto getdiskexitB
    endif

    if diskindex > countB then 
        let diskval = 0 
        goto getdiskexitB
    endif

    if diskindex = 1 then
        let diskval = pegB1
    endif
    if diskindex = 2 then
        let diskval = pegB2
    endif
    if diskindex = 3 then
        let diskval = pegB3
    endif
    if diskindex = 4 then
        let diskval = pegB4
    endif

getdiskexitB:
return

getdiskforrowC:
    let diskval = 0
    let diskindex = maxheight - row
    
    if diskindex <= 0 then
        let diskval = 0
        goto getdiskexitC
    endif

    if diskindex > countC then 
        let diskval = 0 
        goto getdiskexitC
    endif

    if diskindex = 1 then
        let diskval = pegC1
    endif
    if diskindex = 2 then
        let diskval = pegC2
    endif
    if diskindex = 3 then
        let diskval = pegC3
    endif
    if diskindex = 4 then
        let diskval = pegC4
    endif

getdiskexitC:
return


builddiskstring:
    let diskstring = ""
    let emptyhalf = "    " ' Half-width empty space for peg alignment

    ' The total width of the peg area is 7 characters (peg + base)
    
    if diskval = 4 then
        let diskstring = disk4
    endif
    if diskval = 3 then
        let diskstring = disk3
    endif
    if diskval = 2 then
        let diskstring = disk2
    endif
    if diskval = 1 then
        let diskstring = disk1
    endif

    if diskval > 0 then
        ' Pad disk to center it over the base
        if diskval = 4 then
            let diskstring = "   " + diskstring + ""
        endif
        if diskval = 3 then
            let diskstring = "   " + diskstring + ""
        endif
        if diskval = 2 then
            let diskstring = "   " + diskstring + ""
        endif
        if diskval = 1 then
            let diskstring = "   " + diskstring + ""
        endif

    else
        ' Print only the central peg line if no disk
        let diskstring = "   " + peg + ""
    endif

return
