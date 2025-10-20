REM Test program for CALL and GOTO statements
REM 
REM

coldim arr1, c1, c2
let [arr1.c1(1)] = "xxx"

coldim a1, c1, c2, c3  ' define array a1 with 3 columns
coladd a1, c4  ' add one column

let [a1.c4(2)] = "XXX"
let index=2
print "#2.C4=";
print [a1.c4(index)]

print "#2.C4=";
print [a1.c4(2)]

print "Array length=";
print ubound("a1")

print "#2 Column=";
'print colname("a1",2)
print cname("a1",2)

print "Number of columns=";
print cnamescount("a1")

Halt