rem  ============================================
rem The Game of Life. 
rem Uses AI to evolve the grid
rem (c) George P. Afentakis
rem ============================================

print "\n\n\nThe Conway's Game of Life."
print "How many Generations should I'll simulate: ";
input M

gosub MakeAIRules

let SEED=rnd()*34214*M  'SEED is a random 5 digit number or more
let i = 0
let N = 16


MEMSTREAM GridStream

REM Init random grid
For i = 1 To N * N
    let gameChar = "."
    SEED = mod(SEED * 16807, 2147483647)
    let rng = mod(SEED * 11, 100)
    if rng < 27 Then
        gameChar = "@"
    EndIf
    SDATA GameScreen gameChar
Next i

let gen = 0

For gen = 0 To M - 1

    print "Generation " + str(gen)

    RESET GameScreen
    let GameOutput = ""
    let cnt = 0

    FOREACH GameScreen
        GameOutput = GameOutput + [GameScreen.Item] + " "
        cnt = cnt + 1
        if mod(cnt, N) = 0 Then
            GameOutput = GameOutput + "\n"
        EndIf
    ENDFOREACH GameScreen

    print GameOutput
    print ""

    if gen = M - 1 Then
        GOTO DoneAll
    EndIf

    REM write grid to GridStream
    RESET GameScreen
    let GridText = ""
    FOREACH GameScreen
        GridText = GridText + [GameScreen.Item]
    ENDFOREACH GameScreen

    SREWIND GridStream
    STOS GridStream FROM GridText

    REM run the AI generated code
    eval RuleProgram

    REM read new grid back
    SREWIND GridStream
    STOS GridStream TO GridTextNext

    let totalCells = len(GridTextNext)
    let idx = 0
    For idx = 1 To totalCells
        let ch = mid(GridTextNext, idx, 1)
        SSET GameScreen idx ch
    Next idx

Next gen

DoneAll:

print "\nChange on the rules:\n"
print RuleChange
print "\n\n"

HALT


REM ============================================
REM GenerateRule
REM Ask AI to generate code based on a template
REM ============================================

REM --- Configure AI provider & session ---
MakeAIRules:

AIPROVIDER MyProvider, CLAUDE, * 
AISESSION RuleSession, MyProvider, "You Generate code in a tiny BASIC-like language called FBASIC. You must output ONLY clear FBASIC code, no markdown marks, no explanations, no markdown, no ``` fences."
print "Should the Game rule include anything special?"
print "(E.g. How many New rules should I add? Or just write \"Be Very Creative\" )"
print "Left empty line, to use the sample code or enter `L` and the last rule will be used: ";
input suggestionRule

gosub loadSampleCode
if suggestionRule = "" then
   let RuleProgram = samplecode
   return
endif
if ucase(suggestionRule) = "L" Then 
   gosub LoadLastRule
   return
endif

let RuleProgram = ""
let prompt = "Here is a complete novel programming language for a Game of Life rule:" + "\n" + samplecode
let prompt = prompt+"\nStudy the syntax and Rewrite this so it follows different rules the rules can be as usual Or as diffrent New And creative as you want.
Output ONLY the final FBASIC program code without markdown code. No explanations. No code block in the begging Or the end. 
The point here Is playing Conways Game of life. But utilize whatever New rules you want."+ suggestionRule

PRINT "Waiting AI to make a new rule code."
AIPROMPT RuleSession, RuleProgram, prompt
AIPROMPT RuleSession, RuleChange, "What exactly did you change? What New rules did you introduce Or what old ones did you delete? Explain in 1 to 10 sentences"


gosub SaveGeneratedRule

RETURN

rem
rem Save the Generated rule
rem
SaveGeneratedRule:
FILESTREAM RuleFile, out, "","Output", "lifeGame.rule"
STOS RuleFile FROM RuleProgram

FILESTREAM ChangeFile, out, "","Output", "lifeGame.txt"
let txt=suggestionRule+"\n\n"+RuleChange
STOS ChangeFile FROM txt
sclose ChangeFile 
sclose RuleFile
PRINT "Generated program saved."
return


rem
rem Load last Generated rule
rem
LoadLastRule:
FILESTREAM oldRuleFile, in, "","Output", "lifeGame.rule"
STOS oldRuleFile TO RuleProgram
FILESTREAM oldRuleText, in, "","Output", "lifeGame.txt"
STOS oldRuleText TO RuleChange
sclose oldRuleText 
sclose oldRuleFile
PRINT "Last generated program loaded."
return

rem
rem Sample Code
rem 
loadSampleCode:

let samplecode = "let N = 16
SREWIND GridStream
STOS GridStream TO GridText
let row = 0
let col = 0
let qRow = 0
let qCol = 0
let idx = 0
let cellChar = \".\"
let curCell = \".\"
let liveNeighbors = 0
let newChar = \".\"
let NextText = \"\"

For row = 1 To N
For col = 1 To N

qRow = row
qCol = col
GOSUB GetCell
curCell = cellChar

liveNeighbors = 0

qRow = row - 1
qCol = col - 1
GOSUB GetCell
if cellChar = \"@\" Then
   liveNeighbors = liveNeighbors + 1
EndIf

qRow = row - 1
qCol = col
GOSUB GetCell
if cellChar = \"@\" Then
   liveNeighbors = liveNeighbors + 1
EndIf

qRow = row - 1
qCol = col + 1
GOSUB GetCell
if cellChar = \"@\" Then
   liveNeighbors = liveNeighbors + 1
EndIf

qRow = row
qCol = col - 1
GOSUB GetCell

if cellChar = \"@\" Then
   liveNeighbors = liveNeighbors + 1
EndIf

qRow = row
qCol = col + 1
GOSUB GetCell

if cellChar = \"@\" Then
   liveNeighbors = liveNeighbors + 1
EndIf

qRow = row + 1
qCol = col - 1
GOSUB GetCell

if cellChar = \"@\" Then
   liveNeighbors = liveNeighbors + 1
EndIf

qRow = row + 1
qCol = col
GOSUB GetCell

if cellChar = \"@\" Then
   liveNeighbors = liveNeighbors + 1
EndIf

qRow = row + 1
qCol = col + 1
GOSUB GetCell

if cellChar = \"@\" Then
   liveNeighbors = liveNeighbors + 1
EndIf

newChar = \".\"
if curCell = \"@\" Then
   if liveNeighbors = 2 Then
      newChar = \"@\"
   EndIf
   
   if liveNeighbors = 3 Then
      newChar = \"@\"
   EndIf
 Else
   if liveNeighbors = 3 Then
      newChar = \"@\"
   EndIf
EndIf

NextText = NextText + newChar
Next col
Next row

SREWIND GridStream
STOS GridStream FROM NextText
RESULT 0
END

GetCell:
if qRow < 1 Then
   cellChar = \".\"
   RETURN    
EndIf    
if qRow > N Then
   cellChar = \".\"
   RETURN
EndIf    

if qCol < 1 Then
   cellChar = \".\"
   RETURN    
EndIf

if qCol > N Then
   cellChar = \".\"
   RETURN
EndIf

idx = (qRow - 1) * N + qCol
cellChar = mid(GridText, idx, 1)
RETURN
"
return 
