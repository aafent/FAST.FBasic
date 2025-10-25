REM Word Templating  
REM 

FILESTREAM src, in, "", "other", "LoanTemplate.docx"    ' Streams Library
FILESTREAM dst, out, "", "Output", "a.docx"
loginfo "Reading the file"
WORDEXTRACTBODY src, text   ' Templating Library

rem Orchestrate the needed data
rem extract metadata
print "Load data on demand..."
PHVSDATA allNames text          ' TextReplacer Library
PHSdata cnames text             ' TextReplacer Library

let LoanAmount=25000
let s1="abCD12345fghijk"

foreach cnames 
   loginfo "Setting up...."+[cnames.ph]
   phgosub [cnames.ph] else notfound ' TextReplacer Library
endforeach cnames



rem Do the replacement and save the document
loginfo "Creating the new document"
WORDREPALCEBODY src, newdoc, allNames   ' Templating Library

rem (v) Append Consent 
FILESTREAM Consent, in, "", "other", "Consent.docx"
'WORDPAGEBREAK newdoc
WORDAPPEND Consent, newdoc



SCOPY newdoc,dst    ' Streams Library



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
CURSOR Cust, "select CustomerID,Name,VatNumber,Email,Address,City from Customers"   ' SQLData Adapter 
reset Cust
fetch Cust

return

notfound:
print "***NOT FOUND***"
return


End

