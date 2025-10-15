REM Test program for CALL statements
REM Category: Flow Control
REM
RESULT 0
let variableFromBProgram="N/A"
print "PROG-A: before call"
chain "callB.bas"
print "PROG-A: back to program A"
print "PROG-A: back to program A #2"

print "Variable from B program: "+variableFromBProgram

print RESULTVALUE
ASSERT RESULTVALUE = 50
