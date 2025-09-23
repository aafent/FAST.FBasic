## How to execute the interpreter

There are serveral ways to execute the interpreter and run a FBASIC program. Check the following examples for them:

**Example 1 - Instanciate the interpreter** 

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

**Example 2 - Use the FBasicSource helper**

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

**Example 3 - Execute using an executionEnvironment object**

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


