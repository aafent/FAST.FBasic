REM Test program for CALL statements
REM Category: Flow Control
REM
RESULT 0
print "PROG-A: before call"
Call "callB.bas"
print "PROG-A: back to program A"
print RESULTVALUE
ASSERT RESULTVALUE = 50
