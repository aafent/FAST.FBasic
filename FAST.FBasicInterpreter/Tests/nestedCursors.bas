REM Nested Cursors
REM 
Cursor DATA1, "select top 2 name,id from sysobjects where type='U' order by name desc"
'Cursor DATA2, "select top 5 colid, name, type from syscolumns where id=[DATA1.id]"
let row=0
let tms=0
REM loop
ForEach DATA1
let msg="TABLE NAME:"+ [DATA1.name]+", ("+[DATA1.id]+")"
print msg
let sqlcmd="select top 5 colid, name, type from syscolumns where id="+[DATA1.id]
log sqlcmd
Cursor DATA2, sqlcmd
log "***HERE*1**"

ForEach DATA2
log "***HERE*2**"

  let row=row+1
log "***HERE*3**"
let msg="#"+str(row)+": "+sql([DATA1.name])+"."+[DATA2.name]+",  COLID:"+[DATA2.colid]

  print msg
EndForEach DATA2
log "** AFTER END OF DATA2"
reset DATA2 'Reset it is necessary only in the case that the flow will pass again from the FOREACH statement. 
EndForEach DATA1
