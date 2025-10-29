REM Stream Example-1
REM Demonstrating the STOS statement (Stream TO|FROM String)
REM
FILESTREAM lorem, in, "", "other", "LoremIpsum.txt"    ' Streams Library
FILESTREAM dest, out, "", "Output", "StreamExample1.txt"

stos lorem to text 

print "Text length is: "+len(text)
let text=ucase(text)
print mid(text,1,11)

stos dest from text 

End

