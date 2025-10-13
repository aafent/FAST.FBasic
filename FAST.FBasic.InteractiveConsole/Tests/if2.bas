REM Test program for IF...THEN...ELSE...ENDIF statements and inline IF...THEN statement 
REM Category: Decision Making
REM 

' single command 
print "Single line: IF...THEN 1-STATEMENT"
If 1 = 1 Then assert 1
If 1 = 2 Then assert 0


' block nested if    
print "Multi line: IF...THEN...n-STATEMENTs...ELSE...ENDIF"

let a=1
let b=1
gosub blockIfTest
assert RESULTVALUE = 1

let a=1
let b=2
gosub blockIfTest
assert RESULTVALUE = 2

let a=2
let b=1
gosub blockIfTest
assert RESULTVALUE = 3

print "End of Block IF test"

print ""
print "IF with command after THEN"


Halt

rem BLOCK IF TEST
rem 
blockIfTest:
print "Test "+a+","+b
if a=1 Then 
    if b=1 then 
       print "YES #2"
       result 1
	else
      print "ELSE #2"
      result 2
	endif
else 
  print "ELSE #1"
  result 3
endif
print "AFTER IF"
print ""
return
