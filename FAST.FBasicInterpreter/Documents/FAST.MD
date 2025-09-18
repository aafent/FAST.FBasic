## FBASIC - System Manual

FBasic syntax **represents the fundamental rules of a programming language**. Without these rules, it is impossible to write a functioning code. FBasic is extendable and the developer can incorporate new functions and new statements. It is a free-form language, but as it is an interpreter, the error if that exists will be raised when the execution flow reaches the mistyped command. 

### Index

In this manual you can find the following chapters

1.  Flow Control Statements
2.  Loops
3.  Statements
4.  Debugging
5.  Comments
6.  Functions
7.  Variables
8.  Collections
9.  Operators
10.  Add-Ons
    1.  SQL Data provider
11.  Handlers

### Flow Control

*   IF … THEN … ELSE … ENDIF
*   GOTO
*   GOSUB … RETURN
*   CALL
*   HALT or END
*   RESULT

#### Statement IF

If statements can be nested. 

> Attention: The EndIf statement is without any space

_Example:_

> ```plaintext
> If 1=1 Then
> If 2=3 Then
> assert 0
> EndIf
> Else
> assert 0
> EndIf
> ```

#### Statements GOTO, GOSUB…RETURN

The **GOTO** statement is used to jump to a specific label within the source code; it takes a label (which is a word that ends in a colon).  It is a permanent jump to a new program location. Having the same syntax, the **GOSUB** statement performs **a temporary jump to a separate section of code, from which you will later return** (via the **RETURN** command). Every gosub command must be matched by a corresponding return command. 

_Example 1:_

> ```plaintext
> GoTo abc
> assert 0
> def:
> assert 1
> Halt
> ```

#### Statement HALT or END, RESULT

The command **HALT** or **END** ends the processing of the current program and lets the interpreter return to the caller.  Using the statement RESULT, the program can return a value to the caller. This value is available as value to the variable RESULTVALUE as well as a property to the execution result. The program can have several RESULT statements and the active value is the value specified by the last RESULT statement. The RESULTVALUE is a variable and can be used as any variable.

_Example:_

> ```plaintext
> RESULT 0
> If a > 10 Then
> RESULT 3
> Else
> RESULT 456
> EndIf
> print RESULTVALUE
> ```

### Loops

*   FOR … TO … NEXT
*   FOREACH ( _for further info check collections paragraf_ )

#### Statement:  FOR…TO…NEXT

FOR is used to create a loop within a program with the given parameters. In each loop, the variable value is increased by 1.  When the variable value is above the TO value, the loop stops. The increase of the variable value happened on the NEXT statement. 

_Note: the word STEP known from other BASICs is not supported_. 

_Example:_

> ```plaintext
> For i = 1 To 10
>   let a = a + 1
>   print a
> Next i
> assert a = 10
> ```

### Statements

*   LET
*   PRINT
*   INPUT

#### Statement LET

Used to assign the value of an expression to a variable. Syntax is: **Let**  _varname_ **\=** _expression_

> ```plaintext
> LET D=12
> LET E=12^2
> LET F=12^4
> LET SUM=D+E+F
> ```

#### Statements PRINT, INPUT

Both statements are used for input and output. They have only one argument, which is variable or literal. The input and the output handler should be installed.

_example:_

> ```plaintext
> input a1
> print a1
> ```

### Debug

*   ASSERT
*   DUMP
*   LOG

### Comments

*   REM
*   inline comment character:  ' \[single quote\]

_example:_

> ```plaintext
> REM test code
> let a=10 ' set the value 10 to variable a
> ```

### Functions

*   **str(n)**, converts a numeric value to a string value
*   **num(s)**, converts a string value to a numeric value
*   **abs(n)**, the absolute value of a numeric value
*   **min(n,n)**, the minimum number between two
*   **max(n,n)**, the maximum number between two
*   **not(n)**, 0 for true, any other value is false (1)

### Variables

*   RESULTVALUE _(for usage see statements: CALL, RESULT)_
*   “ … ”  
*   \[….\]

### Collections

FBASIC can consume collections that are bonded with the interpreter before the execution of the program. To support the collection mechanism, the language offers the following elements:

*   Loop: FOREACH…ENDFOREACH
*   Statements: FETCH, RESET
*   Functions: EOD

_example, demonstration of CURSOR, nested ForEeach loops, and RESET statement_

> ```plaintext
> REM Data count loop
> Cursor DATA1, "select top 2 name from sysobjects order by name desc"
> Cursor DATA2, "select top 5 id,name from sysobjects order by name"
> let row=0
> let tms=0
> REM loop
> ForEach DATA1
> ForEach DATA2
>   let row=row+1
>   let msg= str(row)+": "+sql([DATA1.name])+", "+[DATA2.name]
>   print msg
> EndForEach DATA2
> reset DATA2 'Reset it is necessary only in the case that the flow will pass again from the FOREACH statement. 
> EndForEach DATA1
> ```

For more examples: see the other paragraphs of this manual. 

### Operators

FBASIC supports the following operators:

<table><tbody><tr><td>Comparisons</td><td>&lt;</td><td>&gt;</td><td>&lt;&gt;</td><td>&gt;=</td><td>&lt;=</td><td rowspan="7"><p><i>Example:</i></p><blockquote><pre><code class="language-plaintext">assert 1 = 1
assert 3 = 2 + 1
assert 2 = (5 - 3)
assert 6 = 3 * 2
assert 2 = 6 / 3
assert -1 = 1 - 2
assert 1 &lt;&gt; 2
assert Not 1 &lt;&gt; 1
assert 3 &gt; 2
assert 2 &lt; 3
assert 2 &gt;= 1
assert 1 &gt;= 1
assert 1 &lt; 2
assert 1 &lt;= 1
assert 1 And 1
assert 1 Or 0
assert "abc" = "abc"
assert "abc" &lt;&gt; "abc1"
assert 1 = Not 0
assert 0 = Not 1
assert 9 = 3 ^ 2</code></pre></blockquote></td></tr><tr><td>Arithmetic Operations</td><td>+</td><td>-</td><td>*</td><td>/</td><td>^</td></tr><tr><td>Equality and Assignment</td><td>=</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr><tr><td>Other</td><td>(</td><td>)</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr><tr><td>Logical</td><td>And</td><td>Or</td><td>Not</td><td>&nbsp;</td><td>&nbsp;</td></tr><tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr><tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></tbody></table>

## Add-Ons

### SQL Data Provider

SQL Data provider is designed to provide data collections based on a SQL statement and handle it as a readonly cursor. Adding the following extensions to the language:

*   Statements: CURSOR
*   Functions: SQL

_example demonstrating the elements: CURSOR, FETCH, EOD(), RESET, GOTO, Labels_

> ```plaintext
> REM Data print and count loop, two times
> REM
> Cursor DATA1, "select top 20 id,name from sysobjects order by name"
> let row=0
> let tms=0
> REM loop
> dataloop1:
>   fetch DATA1
>   If EOD("DATA1") Then
>      GoTo exitloop
>   End If
> 
>   let row=row+1
>   let msg= str(row)+": "+sql([DATA1.id])+", "+sql([DATA1.name])
>   print msg
> 
>   GoTo dataloop1
> 
> exitloop:
>   REM end loop
>   print "***EOD****"
>   let tms=tms+1
>   If (tms = 1) Then
>      reset DATA1
>      GoTo dataloop1
>   EndIf
> ```

To get the connection string, make a request to the “Request For Object” handler with Context=SQL, Group=CONNECTION, Name=ADAPTER and expecting a type of _IdatabaseConnectionAdapter_ as Object

## Handlers

*   PRINT handler
*   INPUT handler
*   Program load handler
*   Request for Objecthandler

## Error messages

### Errors:

*   The Request For Object Handler is not installed ({context},{group},{name}) \[E100\]
*   Cannot get object for: ({context},{group},{name}) \[E101\]
*   Expecting {tok}, got {lastToken} \[E102\]
*   Unexpected end of program \[E103\]
*   Expecting new line, got {lastToken} \[E104\]
*   Expecting keyword/statement, got {keyword}(...) \[E106\]
*   Cannot find label named {name} \[E107\]
*   Assertion fault \[E108\]
*   There is no handler for the CALL statement \[E109\]
*   Collection: {collectionName} not found. \[E110\]
*   Undeclared name {lex.Identifier} \[E111\]
*   Undeclared variable {lex.Identifier} \[E112\]
*   Unexpected token: {lastToken} in primary! \[E113\]
*   FOREACH {collectionName} without corresponding ENDFOREACH {collectionName} statement \[E114\].
*   Cannot jump to next token {tok} \[E115\]
*   EOF found trying to jump to next token {tok} \[E116\]
*   Collection {collectionName} is not SDATA type of collection [E117]
*   SDATA collections supporting only the field name ITEM [E118].

### Exceptions:

*   Data adapter: ${adapter.name} exists. Cannot add more than once. \[X100\]
*   Variable with name {name} does not exist. \[X101\]
*   FATAL ERROR FOUND \[X002\]
*   Found opening bracket but expected closing \[X003\]
*   Found end of line but expected closing bracket \[X004\]
*   Identifier inside brackets must contains a dot character \[X005\]
*   Bracket Identifier cannot starts or ends with dot character \[X006\]
*   Can only do unary operations on numbers. \[X007\]
*   Unknown unary operator. \[X008\]
*   Cannot do binop on strings(except +). \[X009\]
*   Unknown binary operator. \[X010\]

Notes:

> The editor of this file is: https://onlinemarkdowneditor.dev/