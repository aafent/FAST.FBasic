REM Decision Table Example - A
REM Using DTFIND to lookup values in a decision table
REM

dtdim ScoreTable, 0, Salary, Score
dtrow ScoreTable, <100, 1
dtrow ScoreTable, <200, 2
dtrow ScoreTable, <699, 3
dtrow ScoreTable, 700:900, 10
' no for 901
dtrow ScoreTable, 902:2000, 20
dtrow ScoreTable, >=2001, 25

sdata Salary 1500, 50,110,300, 400, 210, 700, 950, 901

foreach Salary
    DTFIND ScoreTable, Score, score, [Salary.Item]
    print "For Salary " + Salary + " Score is " + score
EndForeach Salary

' Not found case
DTFIND ScoreTable, Score, score, 901


If FOUND Then
    print "For Salary 901 Score is " + score
Else
    print "For Salary 901 No matching rule found Score is " + score
endif


Halt
