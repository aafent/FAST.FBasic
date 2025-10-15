REM Decision Table Example - B
REM

DTDIM CreditCheck, "No", Eligible ,Age, Income, Employment

' (v) data to the Decision Table
DTROW CreditCheck, "Yes",>=25, >=40000, "Permanent"
DTROW CreditCheck, "No" ,25, 20000:30000, "Temporary"
DTROW CreditCheck, "Yes", >=35, >0, "CEO"

DTFIND CreditCheck, Eligible, isElig, 25, 20000, "Temporary"
gosub showResult

rem (v) use of wildcard on position of factor: Income. The factor will ignored on search
DTFIND CreditCheck, Eligible, isElig, 35, *, "CEO"
gosub showResult

DTFIND CreditCheck, Eligible, isElig, 35, 20000, "CEO"
gosub showResult


Halt

rem
rem Show results
rem
showResult:

If FOUND  THEN
    PRINT "Match found, eligibility: "+isElig
Else
    PRINT "No matching rule found. Result:" + isElig
ENDIF

return


'DTSET CreditCheck, Age, 30
'DTSET CreditCheck, Income, 50000
'DTSET CreditCheck, Employment, "Permanent"
'DTGET CreditCheck, Eligible, result


