REM Program presents data adapter functionality
REM SQL Data to SDATA Collection
REM Connect to SQL Server and fetch data from sysobjects table
REM by using data adapter and cursor
REM NOTE: via RequestObject Handler the SQLConnection object is populated
REM ---------------------------------------------------
REM Category: Collections
REM

print "Loading name from database to internal collection."
Cursor DATA1, "select top 20 id,name from sysobjects order by name"
foreach DATA1
   sdata names [DATA1.name]
endforeach DATA1

print ""
print "Column name:"
Foreach names
print [names.Item]
EndForeach names

halt

