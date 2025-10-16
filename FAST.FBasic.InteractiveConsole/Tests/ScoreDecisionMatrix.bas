REM Scorecard: Total Rank and Decision Matrix
REM 
REM Score, Decision

dtrow Decision, <0, "Automatic Reject"  
dtrow Decision, 0:30, "Automatic Reject"  
dtrow Decision, 31:70, "Risk Controller Manual Check"
dtrow Decision, 71:100, "Automatic Approve"
dtrow Decision, >101, "Automatic Approve"

