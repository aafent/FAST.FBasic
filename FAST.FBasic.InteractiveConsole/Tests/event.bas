REM Demonstrating program for event raising
REM Library: FBasicEvents
REM -----------------------------

let xx=10
let ss="This is a string"

print "Raising Event:"

RaiseEvent EV1 xx,ss,"SSS",5

Print "End of events test program"

End
