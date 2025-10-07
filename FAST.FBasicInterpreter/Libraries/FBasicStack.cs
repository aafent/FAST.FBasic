using FAST.FBasicInterpreter;

/// <summary>
/// Provides Stack functionality.
/// Statements Syntax:
/// 
/// PUSH Identifier                   | push value on stack
/// POP Identifier                    | Pop from stack to the identifier
/// STACKPEEK Identifier              | Peek the top of the stack to the identifier
/// 
/// Functions Syntax:
/// stackcnt()                        | Returns the number of items in the stack
/// 
/// </summary>
public class FBasicStack : IFBasicLibraryWithMemory
{
    private Stack<Value> stack = new();
    public string uniqueName => "Stack";

    public void InstallAll(Interpreter interpreter)
    {
        interpreter.AddStatement("PUSH", StackPush);
        interpreter.AddStatement("POP", StackPop);
        interpreter.AddStatement("STACKPEEK", StackPeek);

        interpreter.AddFunction("stackcnt", StackCount); // Proper Case
    }


    private void StackPush(Interpreter interpreter)
    {
        // Syntax PUSH identifier|value
        //
        Value value;
        switch( interpreter.lastToken)
        {
            case Token.Identifier:
                string name = interpreter.lex.Identifier;
                value = interpreter.GetValue(name);
                break;
            case Token.Value:
                value = interpreter.lex.Value;
                break;
            default:
                value=Value.Error;
                interpreter.Match(Token.Identifier); // (<) will raise error
                break;
        }
        interpreter.GetNextToken(); // advance to next command

        stack.Push(value);
    }

    private void StackPop(Interpreter interpreter)
    {
        // Syntax POP identifier
        //
        interpreter.Match(Token.Identifier);
        string name = interpreter.lex.Identifier;

        interpreter.GetNextToken(); // advance to next command

        if (stack.Count < 1)
        {
            interpreter.Error(uniqueName, Errors.E129_IsEmpty("Stack"));
            return;
        }

        interpreter.SetVar(name, stack.Pop());

    }

    private void StackPeek(Interpreter interpreter)
    {
        // Syntax PEEK identifier
        //
        interpreter.Match(Token.Identifier);
        string name = interpreter.lex.Identifier;
        interpreter.GetNextToken(); // advance to next command

        if (stack.Count < 1)
        {
            interpreter.Error(uniqueName, Errors.E129_IsEmpty("Stack"));
            return;
        }
        interpreter.SetVar(name, stack.Peek());

    }


    /// <summary>
    /// FBASIC Function stackcnt()
    /// </summary>
    private Value StackCount(Interpreter interpreter, List<Value> args)
    {
        string syntax = "stackcnt()";
        if (args.Count != 0)
            return interpreter.Error("stackcnt", Errors.E125_WrongNumberOfArguments(0, syntax)).value;

        return new Value(stack.Count);
    }

    /// <summary>
    /// Clear the stack on program restart/execute
    /// </summary>
    public void ClearMemory()
    {
        stack.Clear();
    }
}