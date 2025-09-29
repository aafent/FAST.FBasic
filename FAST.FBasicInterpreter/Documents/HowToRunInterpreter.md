## How to execute the interpreter

There are several ways to execute the interpreter and run a FBASIC program. Check the following examples for them:

**Example 1 - Most easy way**

You will need two (2) steps:

**Step 1** - Prepare the environment (once), using a simple or more complex way (2 examples)

Simple:

```cs
            ...
            this.env = new();
            env.DefaultEnvironment();

            ...
```

_Complex:_

```cs
            ...
            this.env = new();
            env.DefaultEnvironment();
            env.callHandler = (name) => 
            { 
            	var filepath = Path.Combine(programsFolder, name); 
            	return File.ReadAllText(filepath); 
           	};
            env.requestForObjectHandler = (context, group, name) =>
            {
                if ($"{context}.{group}.{name}" == "SQL.CONNECTION.DATA1") 
                {
                    var connection = new OdbcConnection("<replace with your CS>");
                    return connection;
                }
                if ($"{context}.{group}.{name}" == "IN.TEST.NAME") return "THIS IS AN IN TEST!";
                return null;
            };
            env.AddLibrary(new FBasicStringFunctions());
            env.AddLibrary(new FBasicMathFunctions());
            env.AddLibrary(new FBasicDateFunctions());
            env.AddLibrary(new FBasicSQLDataAdapter());
            env.AddVariable("V1", "Value For V1");
            ...
```

**Step 2 - Invoke the interpreter, once for each FBASIC program** 

```cs
            ... 
            ...
            // string variable basProgramFile has the FBASIC source program.
            result = FBasicSource.Run(env, basProgramFile);
            if (result.hasError)
            {
                Console.WriteLine(result.errorText);
                if (!string.IsNullOrEmpty(result.errorSourceLine)) Console.WriteLine(result.errorSourceLine);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("....................end of program....................");
                Console.WriteLine($"Result: {result.value}");
            }
```

---

**Example 2 - Instantiate the interpreter** 

```cs
...
Interpreter basic = new Interpreter(true, sourceCode);
basic.printHandler = Console.WriteLine;
basic.inputHandler = Console.ReadLine;
basic.callHandler = null; // No call command will used
basic.requestForObjectHandler = null; // No request handler will used
basic.log = null; // the built-in logger will used
...
basic.Exec()
...Or...
var result = basic.ExecWithResult();
...
...
```

---

**Example 3 - Use the FBasicSource helper**

```cs
...
executionResult result;
var basProgramFile = "MyProgram.bas";
result = FBasicSource.Run(env, basProgramFile, (interp) =>
         {
                // interp.AddDataAdapter(new sqlFBasicDataProvider());
                interp.SetVar("MyVar", new Value("XXXXX"));
                interp.AddLibrary(new FBasicStringFunctions());
         });
if (result.hasError)
{
    Console.WriteLine(result.errorText);
    if (!string.IsNullOrEmpty(result.errorSourceLine)) Console.WriteLine(result.errorSourceLine);
}
else Console.WriteLine($"Result: {result.value}");
...
```

**Example 4 - Execute using an executionEnvironment object**

```cs
...
...
var connection="<ADO_CONNECTION_STRING>";
this.env = new();
env.printHandler += Console.WriteLine;
env.inputHandler += Console.ReadLine;
env.callHandler += (name) => { var filepath = Path.Combine(programsFolder, name); return File.ReadAllText(filepath); };
env.requestForObject += (context, group, name) =>
            {
                if ($"{context}.{group}.{name}" == "ADAPTER.CONNECTION.SQL") return connection;
                if ($"{context}.{group}.{name}" == "IN.TEST.NAME") return "THIS IS AN IN TEST!";
                return null;
            };

...
...

executionResult result;
var basProgramFile = "MyProgram.bas";
result = FBasicSource.Run(env, basProgramFile, (interp) =>
{
    interp.AddDataAdapter(new sqlFBasicDataProvider());
    interp.SetVar("table.column", new Value("myColumn1"));
    interp.AddLibrary(new FBasicStringFunctions());
});
if (result.hasError)
{
    Console.WriteLine(result.errorText);
    if (!string.IsNullOrEmpty(result.errorSourceLine)) Console.WriteLine(result.errorSourceLine);
}
else Console.WriteLine($"Result: {result.value}");
...
...
```

