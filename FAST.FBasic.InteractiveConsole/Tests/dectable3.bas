REM Decision Table Example - C
REM Mapping statements and functions
REM

dtdim M1, "Error", inValue, outValue, BuyerPower
dtrow M1, 500, "Pure", "Low"
dtrow M1, 501:1000, "Lower income", "Low"
dtrow M1, 1001:3000, "Middle income", "Low" 
dtrow M1, 3001:5000, "Moderate", "Middle"
dtrow M1, >5001, "Upper incode", "Hign"

print ""
print "Function map(): "
print "---------------------------------"
' default mapper (aka inValue,outValue as first and second column in the decision table)
let MonthlyIncome=800
print map("M1",MonthlyIncome)
print map("M1",3000)
print map("M1",64000)
print "3001: is "+map("M1",3001)+", FOUND variable is: "+FOUND
print "-5: is "+map("M1",-5)+", FOUND variable is: "+FOUND

'dcmapper M1, inValue, BuyerPower
print map("M1",MonthlyIncome)
print map("M1",3000)
print map("M1",64000)
print map("M1",3001)
print map("M1",-5)

print ""
print "Function dcmap(): "
print "---------------------------------"

print MonthlyIncome +" is "+ dtmap("M1",MonthlyIncome,"InValue","outValue")+" and has buyer power: "+dtmap("M1",MonthlyIncome,"InValue","BuyerPower")

print "Moderate is: "+dtmap("M1","Moderate","outValue","BuyerPower")
print "But Low is: "+dtmap("M1","Low","BuyerPower","outValue")


print ""
print "Statement dtmapper: "
print "---------------------------------"

let MonthlyIncome=1800
print "BEFORE: "+map("M1",MonthlyIncome) ' before dcmapper, key is the 1st column (inValue), value is the 2nd (outValue)

dtmapper M1, "", inValue, BuyerPower
print "AFTER : "+map("M1",MonthlyIncome) ' after dcmapper the key and the value has changed


Halt
