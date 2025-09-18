REM GOSUB...RETURN Test
REM
print "This is a GOSUB...RETURN EXAMPLE"
let tms=0
gosub pointA 'points A,B,C
gosub pointD 'point D
assert tms = 4

HALT

REM This is the point C
pointA:
print "Welcome to pointA"
let tms=tms+1
gosub pointB

Return

REM This is the point C
pointB:
print "Welcome to pointB"
let tms=tms+1
gosub pointC

Return

REM This is the point C
pointC:
print "Welcome to pointC"
let tms=tms+1

Return

REM This is the point D
pointD:
print "Welcome to pointD"
let tms=tms+1

Return
