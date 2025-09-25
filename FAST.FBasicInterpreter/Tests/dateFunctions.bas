REM test code
REM test program for DateFunctions library
REM
REM
let d=jtod("2010-12-31") 'YYYY-MM-DD

print "Date Functions:"
print "..............."
print "date=" + d
if not(isdate(d)) Then
    print "NOT A VALID DATE"
else
    print "VALID DATE"
endif
print "Today=" + date()
print "Today in japan=" + dtoj(date())
print "Day=" + day(d) + "    +1=" + num(day(d)) + 1
print "Year=" + year(d) + ",   month="+month(d)+",    day="+day(d)

print ""
print "Time Functions:"
print "..............."
print "Time=" + time()
print "Hour=" + hour(time())
print "Minute=" + minute(time())
print "Second=" + second(time())

halt
