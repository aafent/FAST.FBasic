REM SQL Script
REM 

'sqlexec "CREATE TABLE IF NOT EXISTS mytable (num1 integer not null)"
'sqlexec "delete from mytable"
'sqlexec "insert into mytable (num1) values (10)"

block sql1
  CREATE TABLE IF Not EXISTS mytable (num1 integer Not null);
  delete from mytable;
  insert into mytable (num1) values (10);
endblock
sqlexec sql1

RETRIEVE tbl, NEW, 1, "Select num1 from mytable"
print "Data Result: ";
print [tbl.num1]

halt

