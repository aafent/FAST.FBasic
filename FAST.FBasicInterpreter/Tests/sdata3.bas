REM SDATA Test-3
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
print "Total items in collection: " + SCNT("Numbers")
print "Item#0:" + SCI("Numbers", 0)
print "Item#1:" + SCI("Numbers", 1)

let last=SCNT("Numbers")-1
print "Item#Last" + ":" + SCI("Numbers", last)
End

