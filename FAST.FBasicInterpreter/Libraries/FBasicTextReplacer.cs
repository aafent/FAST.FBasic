using FAST.FBasicInterpreter;
using System.Text.RegularExpressions;

/// <summary>
/// Syntax:
/// Provides functionality to substitute placeholders in a string.
/// PHGOSUB variable [ELSE label]   :: variable contains as value a label to GOSUB to,  if the label does not exists, 
///                                     then the flow will GOSUB to the ELSE label.
/// PHREPLACE intext outtext        :: Perform a text replace to the intext giving the replaced text to the outtext. 
/// PHSDATA colName intext          :: Create a SDATA type of collection with name colName with the collection names found in the intext. 
/// PHVSDATA colName intext         :: Similar to PHSDATA but it is collection all the identifiers are used. 
/// </summary>
public class FBasicTextReplacer : IFBasicLibrary
{
    public void InstallAll(Interpreter interpreter)
    {
        interpreter.AddStatement("PHREPLACE", placeHolderReplace);
        interpreter.AddStatement("PHSDATA", PlaceHolderSData);
        interpreter.AddStatement("PHVSDATA", PlaceHolderVSData);
        interpreter.AddStatement("PHGOSUB", PlaceHolderGoSub);
    }


    private void PlaceHolderGoSub(Interpreter interpreter)
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

            string value = getValueForPlaceHolder(interpreter, name) ?? "";

            // Apply modifiers
            if (!string.IsNullOrEmpty(modifiers))
            {
                if (modifiers.Contains("U")) 
                    value = value.ToUpper();
                if (modifiers.Contains("L"))
                    value = value.ToLower();
            }

            // Alignments
            if (maxSize > 0)
            {
                // Truncate if longer than maxSize
                if (value.Length > maxSize)
                    value = value.Substring(0, maxSize);
            }

            if (!string.IsNullOrEmpty(modifiers))
            {
                if (minSize > 0 && value.Length < minSize)
                {
                    int padLen = minSize - value.Length;
                    if (modifiers.Contains("c"))
                    {
                        // Center
                        int leftPad = padLen / 2;
                        int rightPad = padLen - leftPad;
                        value = new string(' ', leftPad) + value + new string(' ', rightPad);
                    }
                    else if (modifiers.Contains("r"))
                    {
                        // Right
                        char padChar=' ';
                        if (modifiers.Contains("0")) padChar='0';
                        value = new string(padChar, padLen) + value;
                    }
                    else if (modifiers.Contains("l"))
                    {
                        // Left
                        value = value + new string(' ', padLen);
                    }
                }
            }
            else
            {
                if (minSize > 0 && value.Length < minSize)
                {
                    // Default left align
                    value = value + new string(' ', minSize - value.Length);
                }
            }

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

}