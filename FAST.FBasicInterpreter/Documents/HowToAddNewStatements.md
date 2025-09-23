## Extending the language.

There are two ways to extend the FBASIC programming language. 

1.  To add a library with new statements and functions.
2.  To add statements, functions and variable values before invoke the interpreter via C# code.  
3.  To add new statements only in the main code of the interpreter (non recommended)
    

### Adding a library 

Library is a set of statements and/or functions. To create a new library, create a new class that implements the _IFBasicLibrary_ interface. The are only one method to implement like the following example:

```cs
    public class MyFBasicLibrary : IFBasicLibrary
    { 
        public void InstallAll(Interpreter interpreter)
        {
            interpreter.AddFunction("FUN", Fun);

            interpreter.AddStatement("STATE", State);
            interpreter.SetVar("PI",3.14159);
        }
        
        public static Value Fun(Interpreter interpreter, List<Value> args)
        {
        ...
        }
        
        public static void State(Interpreter interpreter)
      	{
      	...
      	}
   }
```

_Note: The implementation of the functions and the statements are **static** methods._ 

To add the library to the interpreter use the AddLibrary() method: 

```cs
Interpreter interp = new();
interp.AddLibrary(new FBasicStringFunctions());
```

### Adding before program interpretation

Before a program execution, the developer has the ability to add Variables with values, Statements and Functions. 

Example 1:

```cs
Interpreter interp = new();
interp.AddFunction("FUN", MyClass.Fun);

interp.AddStatement("STATE", MyClass.State);
interp.SetVar("PI",3.14159);
...
... code to run (interprate) a program
...
```

Example 2:

```cs
executionResult result;
string basProgramFile = Directory.GetFiles(programsFolder, startupName).FirstOrDefault();
result = fBasicHelper.run(env, basProgramFile, (interp) =>
	{

    	interp.SetVar("table.column", new Value("myColumn1"));
		interp.AddLibrary(new FBasicStringFunctions());
    });
    if (result.hasError)
    {
    	Console.WriteLine(result.errorText);
        if (!string.IsNullOrEmpty(result.errorSourceLine)) Console.WriteLine(result.errorSourceLine);
    }
    else Console.WriteLine($"Result: {result.value}");
```

### Adding new (in main code) statements.   

There are seven (7) steps to add a new statement to the FBASIC Core interpreter core code:

1.  Use **Interpreter\_Statement.cs** or/and the partial class _Interpreter_
2.  Implement a method like:  
    _void MYSTATEMENT(){}_  
    to code how the statement works.
3.  Edit **Token.cs** and add a new value to enumeration: _Token_
4.  Edt **Lexer.cs** find the _GetToken()_ and add a _CASE_ to the long _SWITCH_ converting the _MYSTATEMENT_ to token.
5.  In the **Lexer.cs** find the _isStatement()_ and add _CASE_ to the _SWITCH_ if the added token is statement.
6.  Edit code file  **interpreter\_Elements.cs** and at method _Statement()_ add a _CASE_ to the long _SWITCH_ to map the Token with the implementing method
7.  Add entry for the new statement to **FBasicManual.md**