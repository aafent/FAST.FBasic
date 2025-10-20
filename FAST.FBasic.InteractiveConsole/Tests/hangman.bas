rem 
rem Hangman (game)
rem Category: Examples B
rem
sdata words "apple", "banana", "computer", "python", "hangman", "programming" 
sdata words "science", "guitar", "electric", "holiday"
sdata words "keyboard", "library", "mountain", "notebook", "oxygen", "picture"
sdata words "quality", "rainbow", "sunshine", "travel"
fetch words

print "Welcome to Hangman!"

let randomIndex = int(rnd() * ubound("words"))

let wordToGuess = SCI("words",randomIndex)
let wordLength = len(wordToGuess)

let guessedSoFar = ""
For i = 1 To wordLength
    let guessedSoFar = guessedSoFar + "-"
Next i

let maxTries = 6
let tries = 0

startGame:
print "Word: " + guessedSoFar
print "Enter guess (single letter):";
input guess

let found = 0
For i = 1 To wordLength

If lcase(mid(wordToGuess, i, 1)) = lcase(guess) Then
    let guessedSoFar = left(guessedSoFar, i - 1) + guess + mid(guessedSoFar, i + 1, wordLength)
    let found = 1
EndIf
Next i

If found = 0 Then
    let tries = tries + 1
    print "Wrong guess! Tries left: " + str(maxTries - tries)
Else
print "Good guess!"
EndIf

If tries >= maxTries Then
print "You lost! The word was: " + wordToGuess
    GoTo endGame
EndIf

If guessedSoFar = wordToGuess Then
print "Congratulations! You guessed the word: " + wordToGuess
    GoTo endGame
EndIf

GoTo startGame

endGame:
print "Game over!"

Halt