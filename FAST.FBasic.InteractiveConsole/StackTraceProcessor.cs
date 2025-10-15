namespace FAST.FBasic.InteractiveConsole
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    internal class StackTraceProcessor
    {
        // Regular expression to find the pattern " in <path>:<line>"
        // Group 1 captures the content before " in " (e.g., the method call).
        // Group 2 captures the full file path.
        // Group 3 captures the content after the line number (which is usually the end of the line).
        private const string StackTracePattern = @"( at .*?) in (.*?)(:\s*line\s*\d+.*)";

        public static string ReplaceFilePathWithFileName(string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
            {
                return stackTrace;
            }

            // Use Regex.Replace with a MatchEvaluator delegate
            return Regex.Replace(stackTrace, StackTracePattern, (Match m) =>
            {
                // m.Groups[1] is the part before " in " (e.g., " at FAST.FBasicInterpreter.FBasicDecisionTables.DCMAPSimple(Interpreter interpreter, List`1 args)")
                string prefix = m.Groups[1].Value;
                // m.Groups[2] is the full file path (e.g., "C:\Development\FAST\FAST.FBasic\FAST.FBasicInterpreter\Libraries\FBasicDecisionTables.cs")
                string fullPath = m.Groups[2].Value;
                // m.Groups[3] is the part after the file path (e.g., ":line 55")
                string suffix = m.Groups[3].Value;

                try
                {
                    // Get just the file name from the full path
                    string fileName = Path.GetFileName(fullPath);

                    // Reconstruct the line with only the file name
                    return $"{prefix} in {fileName}{suffix}";
                }
                catch (ArgumentException)
                {
                    // If Path.GetFileName fails (e.g., invalid path characters), 
                    // fall back to the original full path to avoid losing context.
                    return m.Value;
                }
            }, RegexOptions.Multiline);
        }
    }

    #region (+) Example Usage (for testing purposes, not part of the final required output)
    /*
    public class Program
    {
        public static void Main()
        {
            string errorTrace = @"Exception:
     at System.Collections.Generic.Dictionary`2.TryInsert(TKey key, TValue value, InsertionBehavior behavior)
     at System.Collections.Generic.Dictionary`2.Add(TKey key, TValue value)
     at FAST.FBasicInterpreter.FBasicDecisionTables.DCMAPSimple(Interpreter interpreter, List`1 args) in C:\Development\FAST\FAST.FBasic\FAST.FBasicInterpreter\Libraries\FBasicDecisionTables.cs:line 55
     at FAST.FBasicInterpreter.Interpreter.GetIdentifierOrCF(Boolean permitIdentifier, Boolean permitCollection, Boolean permitFunc) in C:\Development\FAST\FAST.FBasic\FAST.FBasicInterpreter\Core\Interpreter_Elements.cs:line 193
     at FAST.FBasicInterpreter.Interpreter.Primary() in C:\Development\FAST\FAST.FBasic\FAST.FBasicInterpreter\Core\Interpreter_Elements.cs:line 221
     at FAST.FBasicInterpreter.Interpreter.Expr(Int32 min) in C:\Development\FAST\FAST.FBasic\FAST.FBasicInterpreter\Core\Interpreter_Elements.cs:line 106
     at FAST.FBasicInterpreter.Interpreter.Print() in C:\Development\FAST\FAST.FBasic\FAST.FBasicInterpreter\Core\Interpreter_Statements.cs:line 414
     at FAST.FBasicInterpreter.Interpreter.Statement() in C:\Development\FAST\FAST.FBasic\FAST.FBasicInterpreter\Core\Interpreter_Elements.cs:line 16
     at FAST.FBasicInterpreter.Interpreter.Line() in C:\Development\FAST\FAST.FBasic\FAST.FBasicInterpreter\Core\Interpreter_Elements.cs:line 84
     at FAST.FBasicInterpreter.Interpreter.Exec() in C:\Development\FAST\FAST.FBasic\FAST.FBasicInterpreter\Core\Interpreter_Methods.cs:line 355
     at FAST.FBasicInterpreter.Interpreter_Extensions.ExecWithResult(Interpreter interpreter, Boolean copyOfTheVariables) in C:\Development\FAST\FAST.FBasic\FAST.FBasicInterpreter\Core\Interpreter_Extensions.cs:line 41";

            string modifiedTrace = StackTraceProcessor.ReplaceFilePathWithFileName(errorTrace);
            Console.WriteLine(modifiedTrace);
        }
    }
    */
    #endregion
}
