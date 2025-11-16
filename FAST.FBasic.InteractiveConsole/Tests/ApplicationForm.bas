REM Word Templating - ApplicationForm (example5)
REM (This example is similar to example1 at wordTest1.bas)
REM Create a Loan Application Form by populating with data the template and 
REM attaching a second document at the end of the first. The example demonstrating
REM how to retrieve only the necessary for the template data from the database.
REM

rem Prepare template documents
FILESTREAM template, inmemory, "", "other", "LoanTemplate.docx"    ' Streams Library
FILESTREAM final, out, "", "Output", "example5.docx"
WORDEXTRACTBODY template, text   ' Templating Library

rem Extract metadata
PHVSDATA allNames text          ' TextReplacer Library

rem Orchestrate the needed data 
print "Load data on demand..."
let AppID=1010  ' Input from the caller application
let Today = date()
retrieve Appl,NEW,1, "select CustomerID,RequestedAmount,LoanTerms,Purpose,ApplicationDate from Applications  where ApplicationID="+sql(AppID)   ' SQLData Adapter 
retrieve Cust,NEW,1, "select Name,VatNumber,Email,Address,City from Customers where CustomerID="+sql([Appl.CustomerID])

rem (v) Do the replacement and save the document
loginfo "Creating the new document"
WORDREPALCEBODY template, ALL, allNames   ' Templating Library

rem (v) Append Consent 
FILESTREAM Consent, in, "", "other", "Consent.docx"
WORDAPPEND Consent, template

SCOPY template,final    ' Streams Library

End
