REM SQL Script
REM 

sqlexec "CREATE TABLE IF NOT EXISTS mytable (num1 integer not null)"
sqlexec "delete from mytable"
sqlexec "insert into mytable (num1) values (10)"
RETRIEVE tbl, NEW, 1, "Select num1 from mytable"
print "Data Result: ";
print [tbl.num1]

halt

