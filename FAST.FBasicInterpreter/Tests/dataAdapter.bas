REM Data count loop
Cursor DATA1, "select top 20 id,name from sysobjects order by name"
let row=0
let tms=0
REM loop
dataloop1:
fetch DATA1
If EOD("DATA1") Then
GoTo exitloop
EndIf


let row=row+1
let msg= str(row)+": "+sql([DATA1.id])+", "+sql([DATA1.name])
print msg

GoTo dataloop1

exitloop:
REM end loop
print "***EOD****"
let tms=tms+1
If (tms = 1) Then
reset DATA1
GoTo dataloop1
EndIf

