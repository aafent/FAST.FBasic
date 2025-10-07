using FAST.FBasicInterpreter;
using FAST.FBasicInterpreter.DataProviders;
using System.Text.RegularExpressions;

/// <summary>
/// Statements Syntax:
/// Provides functionality to substitute placeholders in a string.
/// PHGOSUB variable [ELSE label]   :: variable contains as value a label to GOSUB to,  if the label does not exists, 
///                                     then the flow will GOSUB to the ELSE label.
/// PHREPLACE intext outtext        :: Perform a text replace to the intext giving the replaced text to the outtext. 
/// PHSDATA colName intext          :: Create a SDATA type of collection with name colName with the collection names found in the intext. 
/// PHVSDATA colName intext         :: Similar to PHSDATA but it is collection all the identifiers are used. 
/// 
/// Functions Syntax:
/// pcase(string)                   :: Converts the string to Proper Case (first letter upper rest lower)
/// 
/// Placeholders are defined as {name}, where name is the name of a variable.
/// The placeholder can also have format specifiers and modifiers, e.g., {name:10,20,U0}.
/// Modifiers:
///     min,max, - Minimum and maximum width of the substituted value. eg {name:10,20}   
///     U        - Upper case 
///     L        - Lower case
///     P        - Proper (title) case (first letter upper, rest lower)
///     T        - Trim spaces from both ends of the value.
///     c        - Format a numeric value as currency, # uses the current thread's culture
///     N        - Format a numeric value as readable string with thousand separator and decimal point
///     0        - Left Padding character is '0' instead of space
///     r        - Alignment: left, right, and if both letter then center. Default is left. eg {name:10,20,r}
///     l        - ...
///     lr       - ...
///     
/// </summary>
public class FBasicTextReplacer : IFBasicLibrary
{
    public void InstallAll(Interpreter interpreter)
    {
        interpreter.AddStatement("PHREPLACE", placeHolderReplace);
        interpreter.AddStatement("PHSDATA", PlaceHolderSData);
        interpreter.AddStatement("PHVSDATA", PlaceHolderVSData);
        interpreter.AddStatement("PHGOSUB", PlaceHolderGoSub);

        interpreter.AddFunction("ucase", PCase); // Proper Case
    }


    private static void PlaceHolderGoSub(Interpreter interpreter)
    {
        // Syntax PHGOSUB Identifier_with_label [ELSE label]
        //

        interpreter.Match(Token.Identifier);
        string gosubName = interpreter.lex.Identifier;
        string elseName = string.Empty;

        gosubName = interpreter.GetValue(gosubName).ToString();

        var saveMarker = interpreter.GetCurrentInstructionMarker();
        if (interpreter.GetNextToken() == Token.Else)
        {
            interpreter.GetNextToken();
            interpreter.Match(Token.Identifier);
            elseName = interpreter.lex.Identifier;
            interpreter.PushCurrentInstructionMarker();
        }
        else
        {
            interpreter.PushInstructionMarker(saveMarker);
            interpreter.GoToInstructionMarker(saveMarker); // go back to the saved point
        }

        if (!interpreter.SetFlowToLabelIfExists(gosubName))
        {
            if (string.IsNullOrEmpty(elseName))
            {
                interpreter.Error("PHGOSUB", Errors.E107_LabelNotFound(gosubName));
                return;
            } 
            else
            {
                if(!interpreter.SetFlowToLabelIfExists(elseName))
                {
                    interpreter.Error("PHGOSUB", Errors.E107_LabelNotFound(elseName));
                    return;
                }
            }
        }
    }
    private static void PlaceHolderSData(Interpreter interpreter)=>placeHolderFilter(interpreter,true);
    private static void PlaceHolderVSData(Interpreter interpreter) => placeHolderFilter(interpreter,false);
    private static void placeHolderReplace(Interpreter interpreter)
    {
        // Syntax: PLACEHOLDER input_identifier output_identifier
        //  input is the template, output the variable with the result after the replacing.


        interpreter.Match(Token.Identifier);
        string templateName = interpreter.lex.Identifier;

        interpreter.GetNextToken();

        interpreter.Match(Token.Identifier);
        string outputName = interpreter.lex.Identifier;

        interpreter.GetNextToken();

        var output = substitutePlaceholders(interpreter, interpreter.GetVar(templateName).String);
        interpreter.SetVar(outputName, new Value(output));
    }


    internal static void placeHolderFilter(Interpreter interpreter, bool dottedOnly)
    {
        // Syntax: PHSDATA collectionName identifier_string_template
        //  
        staticDataCollection collection = null;
        interpreter.Match(Token.Identifier);
        string collectionName = interpreter.lex.Identifier;

        if (interpreter.collections.ContainsKey(collectionName))
        {
            if (interpreter.collections[collectionName] is staticDataCollection)
            {
                collection = (staticDataCollection)interpreter.collections[collectionName];
            }
            else
            {
                interpreter.Error("PHxSDATA",Errors.E117_CollectionIsNotSDATAType(collectionName));
                return; // not necessary as Error is exception
            }

        }
        else
        {
            collection = new(interpreter);
            collection.Reset();
            interpreter.AddCollection(collectionName, collection);
        }

        interpreter.GetNextToken(); // move to the first item of the collection

        interpreter.Match(Token.Identifier);
        string templateName = interpreter.lex.Identifier;
        interpreter.GetNextToken(); 

        var list=getUniquePlaceholders(getValueForPlaceHolder(interpreter,templateName), dottedOnly: dottedOnly, leftPartOnly:true);

        foreach (var item in list)
        {
            if (!collection.data.Any(i=>i.String==item))
                collection.data.Add(new Value(item) );
        }

        return;
    }

    /// <summary>
    /// Substitutes all placeholders in the input string with their actual values.
    /// </summary>
    /// <param name="input">The string containing placeholders.</param>
    /// <returns>The string with placeholders substituted.</returns>
    internal static string substitutePlaceholders(Interpreter interpreter, string input)
    {
        if (input == null) return null;

        // Regex to find placeholders, but not those escaped with a backslash
        // (?<!\\)\{([^\{\}]+)\}
        var regex = new Regex(@"(?<!\\)\{([^\{\}]+)\}", RegexOptions.Compiled);

        // For escaped placeholders, replace \{name\} with {name} (unescaped)
        input = input.Replace(@"\{", "{").Replace(@"\}", "}");

        var result = regex.Replace(input, match =>
        {
            var placeholder = match.Groups[1].Value;

            // Check for formatter: {name:specifier,modifiers}
            string name = placeholder;
            int minSize = 0, maxSize = 0;
            string modifiers = null;

            #region (+) Parse for arguments (sizes,modifiers,etc)
            if (placeholder.Contains(":"))
            {
                var parts = placeholder.Split(new[] { ':' }, 2);
                name = parts[0];
                var rightPart = parts[1];

                // Specifier and modifiers
                string specifier = rightPart;
                if (rightPart.Contains(","))
                {
                    var specParts = rightPart.Split(',');
                    // SpecParts[0] is min, [1] is max, [2] (optional) is modifiers
                    if (specParts.Length >= 2)
                    {
                        int.TryParse(specParts[0], out minSize);
                        int.TryParse(specParts[1], out maxSize);

                        if (specParts.Length >= 3)
                            modifiers = specParts[2];
                    }
                }
                else
                {
                    // If only modifiers are present (no min/max)
                    modifiers = rightPart;
                }
            }
            #endregion (+) Parse for arguments (sizes,modifiers,etc)

            string value = getValueForPlaceHolder(interpreter, name) ?? "";

            #region (+) Apply value changing modifiers
            if (!string.IsNullOrEmpty(modifiers))
            {
                if (modifiers.Contains("c"))
                {
                    Double.TryParse(value, out double numValue);
                    {
                        value = numValue.ToString("C");
                    }
                }
                if (modifiers.Contains("N"))
                {
                    Double.TryParse(value, out double numValue);
                    {
                        value = numValue.ToString("#,##0.#########");
                    }
                }


                if (modifiers.Contains("U")) value = value.ToUpper();
                if (modifiers.Contains("L")) value = value.ToLower();
                if (modifiers.Contains("P")) value = ToProperCase(value);
                if (modifiers.Contains("T")) value = value.Trim();
            }
            #endregion (+) Apply value changing modifiers

            // (v) Padding character
            char leftPadChar = ' ';
            char rightPadChar = ' ';
            if (!string.IsNullOrEmpty(modifiers))
            {
                if (modifiers.Contains("0")) leftPadChar = '0';
            }


            #region (+) Work with sizes and Alignments
            if (maxSize > 0)
            {
                // Truncate if longer than maxSize
                if (value.Length > maxSize) value = value.Substring(0, maxSize);

                if (minSize > 0 && value.Length < minSize)
                {
                    int padLen = minSize - value.Length;
                    if (modifiers.Contains("l")  && modifiers.Contains("r")) // center
                    {
                        int leftPad = padLen / 2;
                        int rightPad = padLen - leftPad;
                        value = new string(leftPadChar, leftPad) + value + new string(rightPadChar, rightPad);
                    }
                    else if (modifiers.Contains("r")) // align right
                    {
                        value = new string(leftPadChar, padLen) + value;
                    }
                    else if (modifiers.Contains("l")) // align left
                    {
                        value = value + new string(rightPadChar, padLen);
                    }
                }
            }
            else // maxSize <= 0
            {
                if (minSize > 0 && value.Length < minSize)
                {
                    // Default left align
                    value = value + new string(rightPadChar, minSize - value.Length);
                }
            }
            #endregion (+) Work with sizes and Alignments


            return value;
        });

        return result;
    }

    /// <summary>
    /// This method should be implemented to return the value of the placeholder.
    /// </summary>
    internal static string getValueForPlaceHolder(Interpreter interpreter, string placeHolderName)
    {

        return interpreter.GetValue(placeHolderName).ToString();
    }


    /// <summary>
    /// Extracts all unique, unescaped placeholder names from the input text.
    /// </summary>
    /// <param name="text">The string content to search.</param>
    /// <returns>A List of unique placeholder names (e.g., "ph1", "ph2:1,10,c0").</returns>
    internal static List<string> getUniquePlaceholders(string text, bool dottedOnly=false, bool leftPartOnly=false)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<string>();
        }

        // 1. Find all matches using the Regex.
        // The Regex to find placeholders, but not those escaped with a backslash.
        // Group 1 captures the content/name of the placeholder.
        var placeholderRegex =new Regex(@"(?<!\\)\{([^\{\}]+)\}", RegexOptions.Compiled);
        MatchCollection matches = placeholderRegex.Matches(text);

        // 2. Project the matches to get the content of the first (and only) capturing group (the placeholder name).
        // 3. Use Distinct() to ensure each placeholder name is returned only once.
        // 4. Convert the resulting collection to a List<string>.
        List<string> uniquePlaceholders;

        if (!dottedOnly)
        {
            uniquePlaceholders = matches
                .Cast<Match>() // Cast the MatchCollection to IEnumerable<Match>
                .Select(m => m.Groups[1].Value) // Select the content of Group 1 (the placeholder name)
                .Distinct() // Only keep unique values
                .ToList();
        }
        else
        {
            uniquePlaceholders = matches
                .Cast<Match>() // Cast the MatchCollection to IEnumerable<Match>                
                .Select(m => m.Groups[1].Value) // Select the content of Group 1 (the placeholder name)
                .Where(name => name.Contains("."))
                .Distinct() // Only keep unique values
                .ToList();

            if (leftPartOnly)
            {
                // Create a new list containing only the unique left part of the names.
                uniquePlaceholders = uniquePlaceholders
                    .Select(name =>
                    {
                        // Find the index of the first dot. Since the input list already contains only dotted names,
                        // we're guaranteed to find a dot.
                        int dotIndex = name.IndexOf('.');

                        // Return the substring from the start up to the index of the first dot.
                        return name.Substring(0, dotIndex);
                    })
                    .Distinct() // Ensure the resulting left parts are also unique (e.g., if "User.Name" and "User.ID" were both in the input, "User" is listed once).
                    .ToList();
            }

        }
 
        return uniquePlaceholders;
    }

    /// <summary>
    /// FBASIC Function PCase()
    /// </summary>
    internal static Value PCase(Interpreter interpreter, List<Value> args)
    {
        string syntax = "PCASE(string)";
        if (args.Count != 1)
            return interpreter.Error("PCASE", Errors.E125_WrongNumberOfArguments(1, syntax)).value;
        var value= ToProperCase(args[0].String);
        return new Value(value);
    }

    /// <summary>
    /// Converts the input string to Proper Case (first letter uppercase, rest lowercase).
    /// </summary>
    /// <param name="input">The input string</param>
    /// <returns>String</returns>
    public static string ToProperCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // 1. Capitalize the first character.
        char firstCharUpper = char.ToUpper(input[0]);
        // 2. Lowercase the rest of the string.
        string restOfStringLower = input.Substring(1).ToLower();
        // 3. Combine and return.
        return firstCharUpper + restOfStringLower;
    }

}