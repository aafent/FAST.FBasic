REM Scorecard: Credit Utilization Ratio	
REM 
REM Ratio, Rank


dtrow SCard2, <1, 15        ' also this is the default at DTDIM statement
dtrow SCard2, 1:10, 30 
dtrow SCard2, 11:30, 20
dtrow SCard2, 31:50, 10
dtrow SCard2, >50, 5
