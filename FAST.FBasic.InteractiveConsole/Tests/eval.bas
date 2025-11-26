REM Test program for EVAL statements
REM Category: Flow Control
REM
RESULT 5

let code="
print RESULTVALUE
let X=10
print X
"

eval code
let X=X+1
print "X="+X


