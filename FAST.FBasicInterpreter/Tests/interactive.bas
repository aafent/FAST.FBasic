REM Interactive Console
REM 
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
EndIf

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
Return

End
