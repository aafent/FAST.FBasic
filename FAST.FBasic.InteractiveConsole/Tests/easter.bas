REM Calculating the Orthodox Easter date (Gregorian calendar)
REM Using the Meeus/Jones/Butcher algorithm adapted for Orthodox Easter
REM Reference: https://en.wikipedia.org/wiki/Computus#Meeus/Jones/Butcher_Gregorian_algorithm
REM Note: This algorithm gives the date in the Julian calendar, so we need to convert it to the Gregorian calendar by adding 13 days for the 20th and 21st centuries.
REM This code assumes the input year is in the 20th or 21st century.
REM This algorithm does not using modulo function, as does not exist in FAST BASIC (yet)
REM Category: Date Functions Library 
REM
print "Enter the year:"
input iYear

let avalue = iYear - 4 * int(iYear / 4)
let bvalue = iYear - 7 * int(iYear / 7)
let cvalue = iYear - 19 * int(iYear / 19)
let dvalue = (19 * cvalue + 15) - 30 * int((19 * cvalue + 15) / 30)
let evalue = (2 * avalue + 4 * bvalue - dvalue + 34) - 7 * int((2 * avalue + 4 * bvalue - dvalue + 34) / 7)

let calcmonth = int((dvalue + evalue + 114) / 31)
let calcday = (dvalue + evalue + 114) - 31 * calcmonth + 1

REM Convert from Julian to Gregorian calendar by adding 13 days
let calcday = calcday + 13

REM Adjust for overflow
If calcday > 31 Then
  let calcday = calcday - 31
  let calcmonth = calcmonth + 1
EndIf

print "Orthodox Easter date (Gregorian calendar): " + str(calcday) + "-" + str(calcmonth) + "-" + str(iYear)
