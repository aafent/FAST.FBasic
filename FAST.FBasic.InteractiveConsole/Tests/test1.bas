REM Word Templating - example1
REM Create a Loan Application Form by populating with data the template and 
REM attaching a second document at the end of the first. The example demonstrating
REM how to retrieve only the necessary for the template data from the database.
REM


FILESTREAM template, in, "", "other", "LoanTemplate.docx"    ' Streams Library
FILESTREAM final, out, "", "Output", "example1.docx"
loginfo "Reading the file"
WORDEXTRACTBODY template, text   ' Templating Library

rem Orchestrate the needed data
rem extract metadata
print "Load data on demand..."
PHVSDATA allNames text          ' TextReplacer Library
PHSdata cnames text             ' TextReplacer Library

let LoanAmount=25000

foreach cnames 
   loginfo "Setting up...."+[cnames.ph]
   phgosub [cnames.ph] else notfound ' TextReplacer Library
endforeach cnames


rem (v) Do the replacement and save the document
loginfo "Creating the new document"
WORDREPALCEBODY template, newdoc, allNames   ' Templating Library

rem (v) Append Consent 
FILESTREAM Consent, in, "", "other", "Consent.docx"
'WORDPAGEBREAK newdoc
WORDAPPEND Consent, newdoc

SCOPY newdoc,final    ' Streams Library

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

