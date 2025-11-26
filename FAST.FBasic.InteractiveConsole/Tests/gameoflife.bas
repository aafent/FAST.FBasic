REM Conway's Game of Life using 1-D SDATA "array"
REM Grid: N x N, stored row-major in GameScreen

print "Simulating Conway's game of life using standard rules"
REM The line below sets the grid size. One can also use INPUT to set it.
let N = 16

print "How many generations of Game of life should I run? ";
Input M

'SEED is a random 5 digit number or more
let SEED=rnd()*34214*M


REM =========================
REM 1. INITIAL RANDOM GRID
REM =========================

let i = 0

For i = 1 To N * N
    let gameChar = "."
    SEED = mod(SEED * 16807, 2147483647)
    let rng = mod(SEED * 11, 100)
    if rng < 27 Then
        gameChar = "@"
    EndIf
    sdata GameScreen gameChar
Next i

REM Create NextScreen with same size (so SSET works on it)
reset GameScreen
foreach GameScreen
    sdata NextScreen [GameScreen.Item]
endforeach GameScreen

REM =========================
REM 2. LOOP OVER GENERATIONS
REM =========================
let gen = 0

For gen = 0 To M - 1

    REM ---- Print current generation from GameScreen ----
    print "Generation " + str(gen)

    reset GameScreen
    let GameOutput = ""
    let cnt = 0

    foreach GameScreen
        GameOutput = GameOutput + [GameScreen.Item] + " "
        cnt = cnt + 1
        if mod(cnt, N) = 0 Then
            GameOutput = GameOutput + "\n"
        EndIf
    endforeach GameScreen

    print GameOutput
    print ""          ' blank line between generations

    REM If this was the last generation, do not compute a next one
    if gen = M - 1 Then
        goto DoneAll
    EndIf

    REM =========================
    REM 3. COMPUTE NEXT GENERATION (into NextScreen)
    REM =========================

    REM Variables used by neighbor lookups
    let row = 0
    let col = 0
    let qRow = 0
    let qCol = 0
    let targetIndex = 0
    let idx = 0
    let cellChar = "."
    let liveNeighbors = 0
    let newChar = "."
    let curCell = "."
    let cellIndex = 0

    for row = 1 to N
        for col = 1 to N

            REM Get current cell state at (row, col)
            qRow = row
            qCol = col
            gosub GetCell
            curCell = cellChar

            REM Count live neighbors
            liveNeighbors = 0

            REM (row-1, col-1)
            qRow = row - 1
            qCol = col - 1
            gosub GetCell
            if cellChar = "@" Then
                liveNeighbors = liveNeighbors + 1
            EndIf

            REM (row-1, col)
            qRow = row - 1
            qCol = col
            gosub GetCell
            if cellChar = "@" Then
                liveNeighbors = liveNeighbors + 1
            EndIf

            REM (row-1, col+1)
            qRow = row - 1
            qCol = col + 1
            gosub GetCell
            if cellChar = "@" Then
                liveNeighbors = liveNeighbors + 1
            EndIf

            REM (row, col-1)
            qRow = row
            qCol = col - 1
            gosub GetCell
            if cellChar = "@" Then
                liveNeighbors = liveNeighbors + 1
            EndIf

            REM (row, col+1)
            qRow = row
            qCol = col + 1
            gosub GetCell
            if cellChar = "@" Then
                liveNeighbors = liveNeighbors + 1
            EndIf

            REM (row+1, col-1)
            qRow = row + 1
            qCol = col - 1
            gosub GetCell
            if cellChar = "@" Then
                liveNeighbors = liveNeighbors + 1
            EndIf

            REM (row+1, col)
            qRow = row + 1
            qCol = col
            gosub GetCell
            if cellChar = "@" Then
                liveNeighbors = liveNeighbors + 1
            EndIf

            REM (row+1, col+1)
            qRow = row + 1
            qCol = col + 1
            gosub GetCell
            if cellChar = "@" Then
                liveNeighbors = liveNeighbors + 1
            EndIf

            REM Apply Game of Life rules WITHOUT ELSE

            REM default: cell will be dead
            newChar = "."

            REM live cell survives only with 2 or 3 neighbors
            if curCell = "@" Then
                if liveNeighbors = 2 Then
                    newChar = "@"
                EndIf
                if liveNeighbors = 3 Then
                    newChar = "@"
                EndIf
            EndIf

            REM dead cell becomes live only with exactly 3 neighbors
            if curCell <> "@" Then
                if liveNeighbors = 3 Then
                    newChar = "@"
                EndIf
            EndIf

            cellIndex = (row - 1) * N + col
            SSET NextScreen cellIndex newChar

        next col
    next row

    REM =========================
    REM 4. COPY NextScreen BACK INTO GameScreen
    REM =========================
    idx = 0
    reset NextScreen
    foreach NextScreen
        idx = idx + 1
        if idx <= N * N Then
            SSET GameScreen idx [NextScreen.Item]
        EndIf
    endforeach NextScreen

Next gen

DoneAll:
HALT   REM Program ends, subroutine is below


REM ======================================================
REM SUBROUTINE: GetCell(qRow, qCol) -> cellChar ("@" or ".")
REM Uses GameScreen as the current generation.
REM ======================================================
GetCell:

    REM Out-of-bounds = dead cell
    if qRow < 1 Then
        cellChar = "."
        return
    EndIf
    if qRow > N Then
        cellChar = "."
        return
    EndIf
    if qCol < 1 Then
        cellChar = "."
        return
    EndIf
    if qCol > N Then
        cellChar = "."
        return
    EndIf

    REM Convert 2D → 1D index
    targetIndex = (qRow - 1) * N + qCol

    idx = 0
    reset GameScreen
    foreach GameScreen
        idx = idx + 1
        if idx = targetIndex Then
            cellChar = [GameScreen.Item]
            return
        EndIf
    endforeach GameScreen

    REM fallback (should never hit)
    cellChar = "."
    return
