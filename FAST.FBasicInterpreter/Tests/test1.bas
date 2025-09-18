REM test code
let a=10 ' set the value 10 to variable a

let ss=">>>

select id,name,type from sysobjects "+"


where 1=1"

dump "LIST:"
print ss
halt

dump "dump point:"
If [table.column]="marika" Then
result 1
Else
result 0
EndIf

print "before call"
Call "test2.bas"
print "back to program A"
print RESULTVALUE
halt

let num=(-1)
print abs(num)
print RESULTVALUE
let aa=5
let a = 0


log "select * from gl_kin where aa=:aa order by perigrafh"

REM log PI*aa


For i = 1 To 6
    let a = a + 1
    print a
Next i
print "assert a:"
assert a = 6

print "assert b:"
For i = 1 To 9
print "i=" 
    print i
Next i
print i
assert i = 10

REM (v) test

If aa = 5 Then
If 2 = 3 Then
print "2=3 Why?" ; print "MARIKA"
Else
print "thanks god 2<>3"
EndIf
Else
print "aa<>5 Good!"
EndIf



print "kiki"

print "---the end---"

let aa=3
