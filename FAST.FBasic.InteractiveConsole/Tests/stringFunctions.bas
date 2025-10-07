REM test code for string functions
REM Library: FBasicStrings
REM Category: String Functions
REM

let a = "ABCDefghig" ' 10 characters

print "a=" + a
print "len=" + len(a)
print "left4=" + left(a, 4) + ",    right4=" + right(a, 4)
print "ucase=" + ucase(a) + ",    lcase=" + lcase(a)
print "mid1,3=" + mid(a, 1, 3) + ",    mid5,2=" + mid(a, 5, 2)
print "...."