REM Test program for CALL and GOTO statements
REM 
REM

text="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do 
eiusmod tempor incididunt ut labore et dolore magna aliqua. 
Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip 
ex ea commodo consequat. Duis aute irure dolor in reprehenderit in 
voluptate velit esse cillum dolore eu fugiat nulla pariatur. 
Excepteur sint occaecat cupidatat non proident, sunt in culpa 
qui officia deserunt mollit anim id est laborum
"

let minWordLength=2
let minCount=2
WORDFREQ words, text, minWordLength, minCount

COLNAME words, 1, "a"
COLNAME words, 2, "b"

let cnt=ubound("words")
print "Words found: "+cnt

for i=1 To cnt
  print "Word: ";
  print [words.a(i)];
  print " : ";
  print [words.b(i)]
next i

halt

