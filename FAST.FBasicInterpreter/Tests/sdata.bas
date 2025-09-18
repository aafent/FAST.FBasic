REM SDATA Test
REM 
sdata Months "Jan","Feb","Mar"
sdata Months "Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"
sdata DayNo 1,2,3,4,5,6,7
let ln=1
Foreach Months
let msg=str(ln)+". "+[Months.Item]
print msg
let ln=ln+1
EndForeach Months
assert ln = 13

print "Second list:"
reset Months
Foreach Months
print [Months.Item]
EndForeach Months

print "Day Numbers:"
Foreach DayNo
print [DayNo.Item]
EndForeach DayNo

