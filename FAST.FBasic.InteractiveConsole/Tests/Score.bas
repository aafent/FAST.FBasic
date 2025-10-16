rem Scorecard:
rem 
dtdim SCard1, 0, Age, PaymentHistory, Rank
dtdim SCard2, 15, Ratio, Rank
dtdim SCard3, 0, Years, Rank
dtdim SCard4, 0, Years, Rank
dtdim Decision, 0, Score, Decision

 ' Load the score cards
chain "ScoreCard1.bas"
chain "ScoreCard2.bas"
chain "ScoreCard3.bas"
chain "ScoreCard4.bas"
chain "ScoreDecisionMatrix.bas"

'let birthday="31-12-1970"
'let missedPayments=1
'let creditUtilRatio=10
'let yearsOfCreditHistory=5
'let yearsAtEmployer=3
input birthday
input missedPayments
input creditUtilRatio
input yearsOfCreditHistory
input yearsAtEmployer

let age=datediff("YEAR", birthday,date())
LogInfo "Age:"+age

DTFIND SCard1, Rank, score1, age, missedPayments
DTFIND SCard2, Rank, score2, creditUtilRatio
DTFIND SCard3, Rank, score3, yearsOfCreditHistory
dtfind SCard4, Rank, score4, yearsAtEmployer

let score=score1+score2+score3+score4

dtfind Decision, Decision, decisionText, score

result score


