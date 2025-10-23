REM Testing the Text Replacer library
REM Library: FBasicTextReplacer
REM Category: Text Replacer Library 
REM
let n1=1234543.1234 
let s1="abCD12345fghijk"
sdata Days "Sat","Mon","Thu","The","Wes","Fri","Sat"

let intext=">>>
Day : {Days.Item}
n1,15-25,R0N: {n1:15,25,r0N}
n1,0-25,N: {n1:0,25,N}  ({n1})
s1,3-5,P: {s1:3,5,P}
"

Print "Collections used in the template:"
    
Print "Setup Collections:"
PHsdata cnames intext

foreach cnames
   print "Setting up...."+[cnames.ph]
   phgosub [cnames.ph] else notfound
   print "after phgosub"
endforeach cnames


print ""
print "The output is:"

PHReplace intext outtext
print outtext
print words(outtext,1)+" Words in total"
print words(outtext,2)+" Words > than 2 characters in length"
print words(outtext,4)+" Words > than 4 characters in length"

halt

rem
rem ...............SETUP COLLECTIONS...................
rem 
Days:
print "Preparing Days"
reset Days
fetch Days
print "....done"
return


notfound:
print "***NOT FOUND***"

return


