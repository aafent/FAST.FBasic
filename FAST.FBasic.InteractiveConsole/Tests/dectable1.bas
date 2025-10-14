REM Decision Table Example
REM
dtdim ScoreTable, 0, Salary, Score
dtrow ScoreTable, <100, 0
dtrow ScoreTable, <200, 1
dtrow ScoreTable, <699, 2
dtrow ScoreTable, 700:900, 10
' no for 901
dtrow ScoreTable, 902:2000, 20
dtrow ScoreTable, >=2001, 25

sdata Salary 1500, 300, 400, 210, 700, 950, 901

reset Salary
foreach Salary
    DTFIND ScoreTable, Score, score, [Salary.Item]
    print "For Salary " + Salary + " Score is " + score
EndForeach Salary

Halt


DTDIM CreditCheck, "No", Eligible ,Age, Income, Employment

' (v) data to the Decision Table
DTROW CreditCheck, "Yes",>=25, >=40000, "Permanent"
DTROW CreditCheck, "No" ,25, 20000:30000, "Temporary"
DTROW CreditCheck, "Yes", >=35, >0, "CEO"

DTFIND CreditCheck, Eligible, isElig, 25, 20000, "Temporary"
gosub showResult

DTFIND CreditCheck, Eligible, isElig, 35, 0, "CEO"
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


