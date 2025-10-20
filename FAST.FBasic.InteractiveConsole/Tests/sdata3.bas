REM Test program for statement SDATA
REM Access to a static collection of items
REM Category: Collections
REM 
For i = 1 To 10
sdata Numbers i
Next i

sdata More 999
fetch More
sdata Numbers [More.Item]

foreach Numbers
print [Numbers.Item]
let lastItem=[Numbers.Item]
endforeach Numbers

assert lastItem = 999

print "test:"
print "Total items in collection: " + ubound("Numbers")
print "Item#1:" + SCI("Numbers", 1)
print "Item#2:" + SCI("Numbers", 2)

let last=ubound("Numbers")
print "Item#Last" + ":" + SCI("Numbers", last)
End

