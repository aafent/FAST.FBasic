rem Scorecard:
rem 
rem Age, PaymentHistory, Rank

dtrow SCard1, <22, 0, 10 ' PaymentHistory here is irrelevant  -10

dtrow SCard1, 23:45, 0, 40
dtrow SCard1, 23:45, 1, 25
dtrow SCard1, 23:45, 2, 10
dtrow SCard1, 23:45, >=3, 3

dtrow SCard1, 46:65, 0, 30
dtrow SCard1, 46:65, 1, 15
dtrow SCard1, 46:65, 2, 7
dtrow SCard1, 46:65, >=3, 0

dtrow SCard1, >=66,0 , 3  ' PaymentHistory here is irrelevant  -3
