REM Word Templating - example3
REM 
REM 

rem (v) Declare and prepare 
FILESTREAM CustList, INMEMORY, "", "other", "CustomerList.docx"   
FILESTREAM dst, out, "", "Output", "example3.docx"
WORDEXTRACTBODY CustList, text  ' Templating Library
PHVSDATA Placeholders text ' TextReplacer Library

print "Placeholder in the template: "+ubound("Placeholders")
print "......................................"
foreach Placeholders
print [Placeholders.name]
endforeach Placeholders
print ""
print ""

rem (v) Retrieve data
print "Adding customers:"
print "......................................"

CURSOR Cust, "select CustomerID,Name,VatNumber,Email,Address,City from Customers where CustomerID<'34'"   ' SQLData Adapter 
foreach Cust 
    print [Cust.Name]
    WORDAPPENDROW CustList, "Row"
    WORDREPALCEBODY CustList, FIRST, Placeholders
endforeach Cust
WORDDELETEROW CustList, "Row"

rem (v) Add the Copyright message at the end of the document
FILESTREAM Templates, in, "", "other", "StandardText.docx"
MEMSTREAM part
WORDEXTRACTPART Templates, part, "CopyrightBegin", "CopyrightEnd"
WORDAPPEND part, CustList

rem (v) Remove a part of the document
WORDDELETEPART CustList, "DeleteBegin", "DeleteEnd"


rem (v) Create the output document
scopy CustList, dst

Halt
