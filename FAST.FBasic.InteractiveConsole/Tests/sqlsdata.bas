REM Program presents data adapter functionality
REM SQL Data to SDATA Collection
REM Connect to SqlLite  and fetch data from Customers table
REM by using data adapter and cursor
REM NOTE: via RequestObject Handler the SQLConnection object is populated
REM ---------------------------------------------------
REM Category: Collections
REM


rem uncomment the folloing line to perform a database test
'rinput DB, DEMO, ok


print "Loading name from database to internal collection."
Cursor DATA1, "SELECT CustomerID,Name FROM Customers ORDER BY Name"
foreach DATA1
   sdata names [DATA1.Name]
endforeach DATA1

print ""
print "Column name:"
Foreach names
    print [names.Item]
EndForeach names

halt

