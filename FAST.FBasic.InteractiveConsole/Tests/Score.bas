rem Scorecard:
rem 
dtdim SCard1, 0, Age, PaymentHistory, Rank

 ' Load the score cards
chain "ScoreCard1.bas"

'input birthday
'input missedPayments
let birthday="31-12-1970"
let missedPayments=1

let age=datediff("YEAR", birthday,date())
LogInfo "Age:"+age

DTFIND SCard1, Rank, score1, age, missedPayments

Print "Score - Part1 : "+score1 

