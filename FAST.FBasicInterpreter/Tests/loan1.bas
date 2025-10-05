REM Repayment Schedule Calculation 
REM This program calculates the monthly payment and generates a repayment schedule for a loan.
REM
print "Enter loan amount:"
input loanAmount
print "Enter annual interest rate (percentage):"
input annualInterestRate
print "Enter loan duration in months:"
input months

let monthlyInterestRate = annualInterestRate / 100 / 12
let monthlyPayment = loanAmount * (monthlyInterestRate * (1 + monthlyInterestRate) ^ months) / ((1 + monthlyInterestRate) ^ months - 1)

print "Month   Payment    Interest    Principal    Remaining Balance"

let balance = loanAmount

For i = 1 To months
    let interest = balance * monthlyInterestRate
    let principal = monthlyPayment - interest
    let balance = balance - principal
    print str(i) + "      " + str(monthlyPayment) + "      " + str(interest) + "       " + str(principal) + "        " + str(balance)
Next i
