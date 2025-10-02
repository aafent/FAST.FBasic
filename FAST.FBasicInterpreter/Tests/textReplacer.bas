REM test code
let ph1=10 ' set the value 10 to variable a

let ph2="abcd12345fghijk"
sdata Days "Sat","Mon","Thu","The","Wes","Fri","Sat"

let intext=">>>
PH1 : {ph1}
PH2 : {ph2}
Day : {Days.Item}
ph1,min 3, max 10: {ph1:1,10,c0}
ph2,min 3, max 10: {ph2:3,5,U}
"

Print "Collections used in the template:"

Print "Setup Collections:"
PHsdata cnames intext
foreach cnames
   print "Setting up...."+[cnames.item]
   phgosub [cnames.item] else notfound
   print "after phgosub"
endforeach cnames


print ""
print "The output is:"

PHReplace intext outtext
print outtext
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


