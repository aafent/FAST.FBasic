REM SDATA Test2
REM 
sdata Months "Jan","Feb","Mar"
sdata Months "Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"
sdata Weeks "Week1","Week2","Week3","Week4"

print "Loop x Loop"
Foreach Months
Foreach Weeks
let msg=[Months.Item]+"."+[Weeks.Item]
print msg
EndForeach Weeks

reset Weeks
print "."
EndForeach Months

End

