REM Data count loop
Cursor DATA1, "select top 2 name from sysobjects order by name desc"
Cursor DATA2, "select top 5 id,name from sysobjects order by name"
let row=0
let tms=0
REM loop
ForEach DATA1
ForEach DATA2
  let row=row+1
  let msg= str(row)+": "+sql([DATA1.name])+", "+[DATA2.name]
  print msg
EndForEach DATA2
reset DATA2 'Reset it is necessary only in the case that the flow will pass again from the FOREACH statement. 
EndForEach DATA1
