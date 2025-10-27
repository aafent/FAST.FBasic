REM Word Templating - example2
REM Take 2 parts of a document and pace it at a new
REM 

rem (v) Declare and prepare 
FILESTREAM src, in, "", "other", "StandardText.docx"  
FILESTREAM dst, out, "", "Output", "example2.docx"
WORDEMPTYDOC newDoc


rem (v) implementation
WORDEXTRACTPART src, disclaimer, "DisclaimerBegin", "DisclaimerEnd"
srewind src
WORDEXTRACTPART src, copyright, "CopyrightBegin", "CopyrightEnd"

WORDAPPEND disclaimer, newDoc
WORDAPPEND copyright, newDoc

SCOPY newDoc,dst 

Halt
