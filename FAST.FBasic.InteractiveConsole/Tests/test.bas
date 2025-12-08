REM Test program for CALL and GOTO statements
REM 
REM
let a=10

print a

statement 2, mystatement
    input lastI
    input arg2

    print arg2+":"
    for i=1 To lastI
        print i;
        print " ";
    next i
    print ""
endstatement

for j=1 To 10
 let msg="Count from 1 to "+j
 mystatement j, msg
next j


block marika
let a= 1 
let a= 2 + 1
assert 2 = (5 - 3)
let a= 3 * 2
let a= 6 / 3
let a= 1 - 2
let a= 1 <> 2
let a= Not 1 <> 1
let a= 3 > 2
let a= 2 < 3
let a= 2 >= 1
let a= 1 >= 1
let a= 1 < 2
let a= 1 <= 1
let a= 1 And 1
let a= 1 Or 0
let a= "abc" = "abc"
let a= "abc" <> "abc1"
Endblock

print a
mystatement 10,"Count to 10"
