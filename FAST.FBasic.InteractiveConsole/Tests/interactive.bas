REM 
REM interactive FBASIC interpreter console
REM Category: Examples C
REM

print "FBASIC Interactive console, (c) 2005."
print "                     (enter ? for help)"
print "-----------------------------------------"

nextCommand:
input icmd
let cmd=ucase(icmd)

If cmd = "?" Then
   gosub usage
   GoTo nextCommand
EndIf

let status=(cmd = "HALT" Or cmd = "STOP" Or cmd = "END" Or cmd = "Q")
If status = 1 Then
print "...bye!"
   Halt
End If

let status=(cmd = "HALT" Or cmd = "STOP" Or cmd = "END" Or cmd = "Q")
 


REM at this point the command maybe is a program. So call it
let resultvalue=0
If right(cmd, 4) <> ".BAS" Then
    let icmd=icmd+".bas"
EndIf
Call icmd

GoTo nextCommand


REM .....
REM ..... USAGE
REM .....
usage:
print "Interactive commands:"
print "?      : Returns this help"
print "HALT   : Stop the execution, same as END"
print "To run any FBASIC program file, just type the file name with or without the .bas suffix."
Return

End
