REM Test program for CALL and GOTO statements
REM 
REM
RESULT 0
let a=1*3*(-1)
print "string" + " print"
print(1 + 2 + 4)/abs(1)
Call "callB.bas"
If 1 = 1 Then
assert 1
Else
assert 0
EndIf
GoTo point2 
point1:
assert 0
point2:
assert 1
For i = 1 To 5 
assert i = i
Next i
assert i = 6

halt

