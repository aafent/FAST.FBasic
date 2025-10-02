## FBASIC - Language Reference Manual

FBasic syntax **represents the fundamental rules of a programming language**. Without these rules, it is impossible to write a functioning code. FBasic is extendable and the developer can incorporate new functions and new statements. It is a free-form language, but as it is an interpreter, the error if that exists will be raised when the execution flow reaches the mistyped command. 

### Index

In this manual you can find the following chapters

1.  Flow Control Statements
2.  Loops
3.  Statements
4.  Debugging
5.  Comments
6.  Datatypes
7.  Functions
8.  Variables
9.  Collections
10.  Operators
11.  Libraries and Add-Ons  
    1\. String functions  
    2\. Mathematical functions  
    3\. Date and Time functions   
    4\. SQL Data provider  
    5\. Events triggering  
    6\. Placeholder Replacer
12.  Handlers

---

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

---

### Statements

*   LET
*   PRINT
*   INPUT
*   RINPUT

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

#### Statement RINPUT

Used to perform an input request to the RequestObjectHandler. Has 3 arguments separated by comma.  
Usage: RINPUT group, name, identifier  
Where Group and Name are identifiers that are passed to the RequestObjectHandler to organize the request. The Context is always "IN"  
The last argument, which is variable name that will store the return of the request.  
The RequestObjectHandler, handler should be installed.

_example:_

> ```plaintext
> let s="empty!"
> print s
> rinput TEST, NAME, s
> print s
> End
> ```

### Debug

*   ASSERT
*   DUMP
*   LOGINFO

### Comments

*   REM
*   inline comment character:  ' \[single quote\]

_example:_

> ```plaintext
> REM test code
> let a=10 ' set the value 10 to variable a
> ```

---

### Datatypes

FBASIC, as programming language, typically offers two primary data types: **String** and **Real**.

*   A **String** is a sequence of characters, such as letters, numbers, or symbols, enclosed in double quotes. It's used for handling text.
*   A **Real** (also known as a floating-point number) represents a number with a fractional component. It's used for all numerical calculations, from simple arithmetic to complex equations.

In FBASIC interpreter, other data types like **Integer**, **Boolean**, **Date**, and **Time** are not natively supported. The FBASIC simulate these by using the existing **Real** and **String** types and applying specific conventions or libraries to manage them. For instance, integers are stored as **Real** numbers, and logical values (true/false) can be represented by numeric value. This simplifies the language while still allowing for a wide range of programming tasks.

Especially for **Date** and **Time** datatypes check this manual at libraries chapter (Date & Time libraries). 

About the Boolean values there a need to explain further the concept. In most of the BASIC implementations, **0 is false** and **\-1 is true**.  
Why this?  In most modern programming languages, the integer 0 represents **false**, and any non-zero integer represents **true**.  
However, in early versions of BASIC, a different convention was adopted. This was due to the way logical operations were implemented at the machine-code level.  
The bitwise operations for _AND_, _OR_, and _NOT_ were often optimized to work with all the bits of a value.  
A bitwise _NOT_ of 00000000 (which is 0 in a byte) results in 11111111, which is -1 in two's complement representation.  
This made it convenient to use -1 for true, as it was the direct result of a _NOT_ operation on the false value (0).  
In conclusion, FBASIC will consider the zero (0) numeric value as False, and any non-zero as True. If the value of True will requested the interpreter will return -1.

### Functions

---

#### Built-ins functions:

*   **str(n)**, converts a numeric value to a string value
*   **num(s)**, converts a string value to a numeric value
*   **abs(n)**, the absolute value of a numeric value
*   **min(n,n)**, the minimum number between two
*   **max(n,n)**, the maximum number between two
*   **not(n)**, If the underlying number is **not equal** to the value for true (1) return True, otherwise returns False (-1)

---

#### String functions:

Functions for string manipulation, available if library _FBasicStringFunctions_ is enabled.

*   **len(string):** Returns the length (number of characters) of a string.
*   **left(string, n)**: Extracts the first n characters from a string.
*   **right(string, n)**: Extracts the last n characters from a string.
*   **mid(string, start, length)**: Extracts a substring of a specific length from a string, starting at a given position.
*   **ucase(string)**: Converts a string to uppercase.
*   **lcase(string)**: Converts a string to lowercase.

---

#### **Math Functions:**

FBASIC,  includes a variety of built-in mathematical functions to perform common calculations. There are available if library _FBasicMathFunctions_ is enabled.

**Trigonometric Functions**

*   **sin(X):** Returns the sine of X. The value of X must be in radians.
*   **cos(X):** Returns the cosine of X. The value of X must be in radians.
*   **tan(X):** Returns the tangent of X. The value of X must be in radians.
*   **atn(X):** Returns the arctangent of X. The result is in radians.

**Logarithmic and Exponential, Powers Functions**

*   **log(X):** Returns the natural logarithm (base e) of X.
*   **exp(X):** Returns the value of e raised to the power of X. This is the inverse of the LOG function.
*   **sqr(X):** Square Root. Returns the square root of X. The value of X must be non-negative.

**Value Functions**

*   **abs(X):** Returns the absolute value of X. This function is useful for ensuring a number is non-negative. Note that abs() functions is available without the math library.
*   **sgn(X):** Returns the sign of X. It returns 1 if X>0, -1 if X\<0, and 0 if X=0.
*   **rnd():** This function is used to generate a random number. Its behavior can vary between different versions of BASIC. Often, RND without an argument generates a random number between 0 and 1.

**Integer and Rounding Functions**

*   **int(X):** Returns the largest integer less than or equal to X. For positive numbers, this is equivalent to truncating the decimal part.
*   **fix(X):** Returns the integer part of X by truncating the decimal part. For negative numbers, it behaves differently from INT. For example, FIX(-3.7) returns -3, while INT(-3.7) returns -4.

**Constants**

as the FBASIC does not support constants, a constant is a variable with a value. Its up to the programmer to avoid to modify that value. 

*   **PI:** The PI value, approximately equal to 3.14159.

---

#### Date & Time Functions

Date and Time functions in the BASIC programming language are used to handle and manipulate dates and times. These functions allow programs to retrieve the current date and time, perform calculations with dates, and format dates for display or any other usage. The native date format is the DD-MM-YYYY. As the FBASIC does not support date as datatype, the functions will consider any valid value having this format as a date. 

Date and Time functions listed here are available if library _FBasicDateFunctions_ is enabled.

*   **date():** This function returns a string representing the current system date. The format typically includes the day, month, and year.
*   **isdate(d)**: Check if the input string is a valid date.
*   **day():** Extract the day as an numeric value (integer) from a date variable. For example, day("26-10-2025") would return 26.
*   **month():** Extract the month as an numeric value (integer) from a date variable. For example, month("26-10-2025") would return 10.
*   **year():** Extract the year as an numeric value (integer) from a date variable. For example, year("26-10-2025") would return 2025.
*   **jtod():** Converts the japan style of date (YYYY-MM-DD) to International (DD-MM-YYYY).  For example, JTOD("2025-10-26") would return “26-10-2025”.
*   **dtoj():** Converts the International (DD-MM-YYYY) date to japan style of date (YYYY-MM-DD).

The native time format is the HH:MM:SS.FFF. As the FBASIC does not support date or time as datatype, the functions will consider any valid value having this format as a time. Also the functions will consider valid a time of the formats:  HH:MM:SS, HH:MM, and the HH:MM:SS.FFF, HH:MM:SS.FF and HH:MM:SS.F

*   **time():** This function returns a string representing the current system time. The format is usually HH:MM:SS.
*   **hour():** extracts the hours as an integer from a time variable.
*   **minute():** extracts the minutes as an integer from a time variable.
*   **second():** extracts the seconds as an integer from a time variable.

---

### Variables

*   RESULTVALUE _(for usage see statements: CALL, RESULT)_
*   “ … ”  
*   \[….\]

---

### Collections

FBASIC can consume collections that are bonded with the interpreter before the execution of the program. To support the collection mechanism, the language offers the following elements:

*   Loop: FOREACH…ENDFOREACH
*   Statements: FETCH, RESET
*   Functions: EOD, SCI, SCNT

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

---

### Static Collections

Using the statement SDATA the developer can define a static collection of data. For further information see the following example:

```plaintext
REM SDATA Test
REM 
sdata Months "Jan","Feb","Mar"
sdata Months "Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"
sdata DayNo 1,2,3,4,5,6,7
let ln=1
Foreach Months
let msg=str(ln)+". "+[Months.Item]
print msg
let ln=ln+1
EndForeach Months
assert ln = 13

print "Second list:"
reset Months
Foreach Months
print [Months.Item]
EndForeach Months

print "Day Numbers:"
Foreach DayNo
print [DayNo.Item]
EndForeach DayNo
```

---

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

---

## Libraries and Add-Ons

### What is a library

Libraries are add-ons to the main FBASIC Interpreter that provide **functions**, **statements**, and **variables** (or **constants**). You can only enable a library during the interpreter's initialization, before you run the FBASIC program. Implementing a library is incredibly easy; refer to the how-to manual for more information.

*   **Buildins Libray:** contains the very basic functions are available to the FBASIC programing language, without the need to enable it. There is switch that the developer can disable that code.
*   **Buildins Collections Library:**  contains the very basic functions and statements for collections manipulation. It is enabled by default but the developer can disable that code.
*   **Strings Library:** The name of this library is: _FBasicStringFunctions_ and for further information see the _Functions_ section of this document.
*   **Mathematical Library:** The name of this library is: _FBasicMathFunctions_ and for further information see the _Functions_ section of this document.
*   **Date and Time Library:** The name of this library is: _FBasicDateFunctions_ and for further information see the _Functions_ section of this document.
*   **Events Triggering Library:** The name this library is: _FBasicEvents_ (see below for further information)

---

### SQL Data Provider

SQL Data provider is designed to provide data collections based on a SQL statement and handle it as a read only cursor. To active it add the library: _FBasicSQLDataAdapter_. Adding the following extensions to the language:

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

---

### Events Triggering

Event triggering is the act of generating a signal that notifies other parts of a program or system that a specific,  
significant action or condition has occurred. Essentially, it's the mechanism that initiates  
the event-driven sequence.

It is provided by the Library **FBasicEvents** and for further details see the document: [Events](event.md)

---

### Placeholder Replacer

Placeholder Replacer implements the low level functionality is necessary to substitute placeholders in a string with value of identifiers (variables or collections).  
It is provided by the Library **FBasicTextReplacer**

Available statements are:

**PHGOSUB variable \[ELSE label\]** :: variable contains as value a label to GOSUB to, if the label does not exists,  
then the flow will GOSUB to the ELSE label.  
**PHREPLACE intext outtext** :: Perform a text replace to the intext giving the replaced text to the outtext.  
**PHSDATA colName intext** :: Create a SDATA type of collection with name colName with the collection names found in the intext.  
**PHVSDATA colName intext** :: Similar to PHSDATA but it is collection all the identifiers are used.

---

## Handlers

*   PRINT handler
*   INPUT handler
*   Program load handler
*   Request for Objecthandler

---

## Error messages

### Errors:

*   The Request For Object Handler is not installed ({context},{group},{name}) \[E100\]
*   Cannot get object for: ({context},{group},{name}) \[E101\]
*   Expecting {tok}, got {lastToken} \[E102\]
*   Unexpected end of program \[E103\]
*   Expecting new line, got {lastToken} \[E104\]
*   Expecting keyword, got {keyword} \[E106\]
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
*   {name} is a function, cannot be declared as variable name \[E117\].
*   CALL return's error: {subResult.errorText} \[E118\]
*   SDATA collections supporting only the field name ITEM \[E119\].
*   Collection for {name} is empty/out-of-ForEachLoop \[E120\].
*   Found RETURN without corresponding GOSUB \[E121\]
*   Expecting Value or Identifier, got {lastToken} \[E123\]
*   {forbittenItem} not permitted. Use LET to assign the result value and use the variable \[E124\]
*   Wrong number of arguments. Expected {argNo}. {syntax} \[E125\]
*   Wrong argument(s) type. Check argument: {argNo}. {syntax} \[E126\]
*   Wrong referred type. Expected: {expectedType}. {syntax} \[E127\]
*   Request object error. An object expected but got null. \[E128\]

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
*   Cannot convert {fromType} to {toType}. \[X011\]
*   FATAL Error ({source}). {text} \[X012\]

**Errors from SQL Data Adapter**

*   Unsupported data type {type} \[S001\]
*   SQL() takes only one argument \[S002\]
*   Name: {name} is missing from {cursorName} \[S003\]

Notes:

> The editor of this file is: https://onlinemarkdowneditor.dev/
