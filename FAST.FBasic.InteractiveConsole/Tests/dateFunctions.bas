REM Testing the DateFunctions library
REM Library: FBasicDateFunctions
REM Category: Date Functions Library 
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

print ""
print "Date Diff:"
print "..............."
let startDate = "15-01-2025"
let endDate = "25-02-2025"

print ""
print "Day 1: "+startDate+"  Day 2: "+endDate
print "Difference in days: " + datediff("DAY", startDate, endDate)
print "Difference in months: " + datediff("MONTH", startDate, endDate)
print "Difference in years: " + datediff("YEAR", startDate, endDate)
print ""
print "Next Year:" + dateadd("YEAR", 1, startDate)
print "Prev 2 Months:" + dateadd("MONTH", -2, startDate)
print "Prev 10 Days:" + dateadd("DAY", -10, startDate)
print "Next day:" + dateadd("DAY", +1, startDate)

let startDate = "15-01-2024"
let endDate = "15-01-2025"

print ""
print "Day 1: "+startDate+"  Day 2: "+endDate
print "Difference in days: " + datediff("DAY", startDate, endDate)
print "Difference in months: " + datediff("MONTH", startDate, endDate)
print "Difference in years: " + datediff("YEAR", startDate, endDate)


halt
