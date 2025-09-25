REM 
REM SQL Data to SDATA Collection
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

