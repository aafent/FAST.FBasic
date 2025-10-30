REM Test program for CALL and GOTO statements
REM 
REM (v) request for an object that will be used into this program 
rinput OBJECT,EMPLOYPROFILE, emp ' load the external object into variable: emp

json text FROM emp
json text2 dynamic from emp

print emp
print ""
print ""
print ""
print text
print ""
print ""
print text2

Halt