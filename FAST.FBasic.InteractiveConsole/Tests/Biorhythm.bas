REM FBASIC Biorhythm Calculator 
REM Category : example
REM
print ""
print ""
print "Enter your birth date (year month day), press enter after each value:";
input BYR, BMO, BDA


let BDATE = right("0"+BDA,2)+"-"+right("0"+BMO,2)+"-"+BYR

'print "Enter target date (year month day):"
'input TYR, TMO, TDA
let TODAY=date()
let TYR=year(TODAY)
let TMO=month(TODAY)
let TDA=day(TODAY)

print "Calculating biorhythm for date: " + TODAY+ "  Birth: "+BDATE

let diffDays = datediff("day", BDATE, TODAY)
print "You are alive for "+diffDays+" days!"
print ""

REM Constants
let physCycle = 23
let emoCycle = 28
let intCycle = 33

REM Calculate sine value helper emulation using mathFunctions.bas
REM We'll inline math function calls and calculations

REM Start chart header
print "Day          Phys Emo  Int"
print "--------------------------"

let d = diffDays - 7
startLoop:

    if d > diffDays + 7 then
        goto endLoop
    EndIf

    REM Calculate angle for each cycle
    let angPhys = 2 * PI * (d / physCycle)
    let angEmo = 2 * PI * (d / emoCycle)
    let angInt = 2 * PI * (d / intCycle)

    REM Call sin function (assumed available from mathFunctions.bas)
    let valPhys = sin(angPhys)
    let valEmo = sin(angEmo)
    let valInt = sin(angInt)

    REM Convert val to symbol: +, -, 0
    if valPhys > 0.3 then
        let symbolPhys = "+"
    else
        if valPhys < -0.3 then
            let symbolPhys = "-"
        else
            let symbolPhys = "0"
        endif
    endif

    if valEmo > 0.3 then
        let symbolEmo = "+"
    else
        if valEmo < -0.3 then
            let symbolEmo = "-"
        else
            let symbolEmo = "0"
        endif
    endif

    if valInt > 0.3 then
        let symbolInt = "+"
    else
        if valInt < -0.3 then
            let symbolInt = "-"
        else
            let symbolInt = "0"
        endif
    endif

    print dateadd("day",d,BDATE) + "    " + symbolPhys + "    " + symbolEmo + "    " + symbolInt

    let d = d + 1
    goto startLoop

endLoop:

end
