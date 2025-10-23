REM Word Templating  
REM 

FILESTREAM src, in, "lib", "path", "test.docx"
FILESTREAM dst, out, "lib", "path", "dst.docx"
loginfo "Reading the file"
EXTRACTWORDBODY src, text

print text

rem Orchestrate the needed data
rem extract metadata
print "Load data on demand..."
PHVSDATA allNames text
PHSdata cnames text

let n1=1234543.1234 
let s1="abCD12345fghijk"

foreach cnames
   loginfo "Setting up...."+[cnames.ph]
   phgosub [cnames.ph] else notfound
endforeach cnames



rem Do the replacement and save the document
loginfo "Creating the new document"
REPALCEWORDBODY src, newdoc, allNames
SCOPY newdoc,dst




halt

rem
rem ...............SETUP COLLECTIONS...................
rem 
Days:
loginfo "Loading Data for Days"
sdata Days "Sat","Mon","Thu","The","Wes","Fri","Sat"
reset Days
fetch Days
return

Cust:
loginfo "Loading Customers"
CURSOR Cust, "select synal,epvnymia,afm from sy_synal where synal='0031'"
reset Cust
fetch Cust

return

notfound:
print "***NOT FOUND***"
return


End

