REM Test program for CALL and GOTO statements
REM 
REM (v) request for an object that will be used into this program 
rinput OBJECT,EMPLOYPROFILE, emp ' load the external object into variable: emp


json text FROM emp
print text
json text to emp
rinput OBJECT,SHOWEMPLOYPROFILE,emp 'show the object
print ""
print ""
print ""

json text2 dynamic FROM emp
print text2
print ""
print ""
json text2 DYNAMIC to emp

rinput OBJECT,SHOWEMPLOYPROFILE,emp 'show the object

Halt