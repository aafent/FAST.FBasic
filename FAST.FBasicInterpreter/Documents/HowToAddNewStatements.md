How to add a new Statement to the FBASIC Core interpreter.

1. Use Interpreter_Statement.cs or/and the partial class Interpreter
2. Implement a method like:
void MYSTATEMENT(){}
to code how the statement works. 

3. Open Token.cs and add a new value to enumeration: Token
4. Open Lexer.cs find the GetToken() and add a CASE to the long SWITCH converting the MYSTATEMENT to token. 
5. In the Lexer.cs find the isStatement() and add CASE to the SWITCH if the added token is statement.
6. Open interpreter_Elements.cs and at method Statement() add a CASE to the long SWITCH to map the Token with the implementing method

7. Add entry for the new statement to FBasicManual.md

