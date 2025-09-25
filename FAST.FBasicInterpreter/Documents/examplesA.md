**Example A1.**

Demonstrating SQL data and SDATA collections. Program loading the data  from a SQL SELECT statement  and store them in a SDATA collection. The just print the static data collection.

```cs
REM 
REM Data to SDATA Collection
REM 

print "Loading name from database to internal collection."
Cursor DATA1, "select top 20 id,name where type='S' from sysobjects order by name"
foreach DATA1
   sdata names [DATA1.name]
endforeach DATA1

print ""
print "Column name:"
Foreach names
   print [names.Item]
EndForeach names

halt
```

**Example A2**

Demonstrating CURSOR, EOD(), FETCH, sql(), REST and the fetching data without the FOREACH…ENDFOREACH loop 

```cs
REM Data count loop
Cursor DATA1, "select top 20 id,name from sysobjects order by name"
let row=0
let tms=0
REM loop
dataloop1:
fetch DATA1
If EOD("DATA1") Then
   GoTo exitloop
EndIf

let row=row+1
let msg= str(row)+": "+sql([DATA1.id])+", "+sql([DATA1.name])
print msg

GoTo dataloop1

exitloop:
REM end loop
print "***EOD****"
let tms=tms+1
If (tms = 1) Then
  reset DATA1
  GoTo dataloop1
EndIf

```

**Example - A3**

Demonstrating long strings and remarks 

```cs
REM Long string code
let a=10 ' set the value 10 to variable a
let ss=">>>

select id,name,type from sysobjects "+"

where 1=1"

print ss
```

**Example - A4**

Demonstrating GOSUB…RETURN statements 

```cs
REM GOSUB...RETURN Test
REM
print "This is a GOSUB...RETURN EXAMPLE"
let tms=0
gosub pointA 'points A,B,C
gosub pointD 'point D
assert tms = 4

HALT

REM This is the point C
pointA:
print "Welcome to pointA"
let tms=tms+1
gosub pointB

Return

REM This is the point C
pointB:
print "Welcome to pointB"
let tms=tms+1
gosub pointC

Return

REM This is the point C
pointC:
print "Welcome to pointC"
let tms=tms+1

Return

REM This is the point D
pointD:
print "Welcome to pointD"
let tms=tms+1

Return
```

**Example - A5**

Demonstrating the Data functions library

```cs

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
```

///