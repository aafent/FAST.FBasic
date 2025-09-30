## FBasic Events

The FBasic standard interpreter does not provide event triggering or handling on the minimal interpreter. To enable the evnet triggering and handing functionality you need to add the library: _FBasicEvents._

### Statements:

#### **RaiseEvent**

The syntax for the **RaiseEvent** statement  is used to trigger a custom event. The event does not need to has been previously declared but to activate it and make sense a handler has to been installed before the run of the FBasic program.

**Syntax:** RaiseEvent eventname \[arg1, arg2, … argN\]

| Part | Description |
| --- | --- |
| **RaiseEvent** | The required keyword/statement that instructs the program to trigger the named event. |
| **eventname** | The **required** name of the custom event. |
| **(argumentlist)** | **Optional** list of comma separated variables, collection items and values that match the sequence of the expected by the listener sequence of arguments (see examples). |

**Example** of FBasic program:

```php
REM Library: FBasicEvents
REM EVENTRAISE testing
REM ...................................

let xx=10
let ss="This is a string"
print "Raising Event:"

RaiseEvent EV1 xx,ss,"text",5

Print "End of events test program"

End
```

Output:

> `Program name: event.bas, enter R(UN) command to run it.`  
> `[enter command: R=rerun, Q=Exit, H=Help, L=Load, DIR....] r`  
> `**Raising Event:**`  
> `**1st Listener for event: EV1 triggered. Stack contains 4 arguments**`  
> `**2nd Listener for event: EV1 triggered. Stack contains 4 arguments**`  
> `**End of events test program**`
> 
> `....................end of program....................`

**How to setup the interpreter:** 

```cs
 var env = new ExecutionEnvironment();
 ...
 ...
 env.AddLibrary(new FBasicEvents());

 FBasicEvents.Reset(); // not necessary the 1st time 
 FBasicEvents.FBasicEventHandler += testEventHandler1!;
 FBasicEvents.FBasicEventHandler += testEventHandler2!;
 ...
 ...
 public void testEventHandler1(object sender, (string name, Stack<Value> args) e)
 {
	Console.WriteLine($"1st Listener for event: {e.name} triggered. Stack contains {e.args.Count} arguments");
 } 
 public void testEventHandler2(object sender, (string name, Stack<Value> args) e)
 {
    Console.WriteLine($"2nd Listener for event: {e.name} triggered. Stack contains {e.args.Count} arguments");
 }
```

**Special attention** on the signature of the event's delegation. The signature is:

```cs
        void testEventHandler2(object sender, (string name, Stack<Value> args) e)
```

The event using the built-in EventHandler\<T> generic delegate where the sender is the interpreter instance, the fulling the dynamic object  e (see the parenthesis) where the property e.name is the event name (EV1 in this example) and the e.args is a stack collections with the arguments. The developer of the handler should pop the values of the stack with the revered order the the places (most recent, in the top of the stack is the last argument).