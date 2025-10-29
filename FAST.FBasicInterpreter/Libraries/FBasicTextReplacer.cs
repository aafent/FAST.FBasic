using FAST.FBasicInterpreter;
using System.Text.RegularExpressions;

/// <summary>
/// Statements Syntax:
/// Provides functionality to substitute placeholders in a string.
/// PHGOSUB variable [ELSE label]   :: variable contains as value a label to GOSUB to,  if the label does not exists, 
///                                     then the flow will GOSUB to the ELSE label.
/// PHREPLACE intext outtext        :: Perform a text replace to the intext giving the replaced text to the outtext. 
/// PHSDATA colName intext          :: Create a SDATA type of collection with name colName with the collection names found in the intext. 
/// PHVSDATA array intext           :: Create an array with two columns the `name` and the `ph`. Name contain the variable that it is used in the placeholder and ph is the placeholder as it in the text.
/// WORDFREQ array_name, input_text, min_word_length, min_count_to_result  :: Analyze the input_text, finding words frequency for the words that they have the minimum word length and results in the array_name, on the first index the word found and on the second the word's frequency. Reports only word that they have at least the min_count frequency. 
/// 
/// Functions Syntax:
/// pcase(string)                   :: Converts the string to Proper Case (first letter upper rest lower)
/// words(s, 1)                     :: counts the number of words in a string s that are at least 1 character long. if the input string is empty returns 0, if the minWordSize is less than 1 it is set to 1. The word delimiters are space, tab, newline and carriage-return. Consequent delimiters are treated as one.
/// phtoname(s)                     :: from a placeholder keeps only the variable name (simple or squear bracket) 


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
    public void InstallAll(IInterpreter interpreter)
    {
        interpreter.AddStatement("PHREPLACE", placeHolderReplace);
        interpreter.AddStatement("PHSDATA", PlaceHolderSData);
        interpreter.AddStatement("PHVSDATA", PlaceHolderVSData);
        interpreter.AddStatement("PHGOSUB", PlaceHolderGoSub);
        interpreter.AddStatement("WORDFREQ", WordFrequency);

        interpreter.AddFunction("pcase", PCase); // Proper Case
        interpreter.AddFunction("words", Words); // words count
        interpreter.AddFunction("phtoname",ToName);
    }

    #region (+) FBASIC Statments

    private static void PlaceHolderGoSub(IInterpreter interpreter)
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
    private static void PlaceHolderSData(IInterpreter interpreter)=>placeHolderFilter(interpreter,true);
    private static void PlaceHolderVSData(IInterpreter interpreter) => placeHolderFilter(interpreter,false);
    private static void placeHolderReplace(IInterpreter interpreter)
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

    private static void WordFrequency(IInterpreter interpreter)
    {
        //
        // Syntax WORDFREQ array_name, input_text, min_word_length, min_count_to_result
        //
        //

        interpreter.Match(Token.Identifier);
        var arrayName=interpreter.lex.Identifier;

        interpreter.GetNextToken();
        interpreter.Match(Token.Comma);

        interpreter.GetNextToken();
        interpreter.Match(Token.Identifier);
        Value value = interpreter.GetValue(interpreter.lex.Identifier);

        interpreter.GetNextToken();
        interpreter.Match(Token.Comma);

        interpreter.GetNextToken();
        interpreter.Match(Token.Identifier);
        Value minWordLength = interpreter.GetValue(interpreter.lex.Identifier);

        interpreter.GetNextToken();
        interpreter.Match(Token.Comma);

        interpreter.GetNextToken();
        interpreter.Match(Token.Identifier);
        Value minCount = interpreter.GetValue(interpreter.lex.Identifier);

        interpreter.GetNextToken();

        var result=CalculateWordFrequency(value.String, minWordLength.ToInt(), minCount.ToInt());

        FBasicArray array;
        if (interpreter.IsArray(arrayName) )
        {
            array = interpreter.GetArray(arrayName);
            array.ResetArray();
        }
        else
        {
            array = new();
            interpreter.AddArray(arrayName,array);
        }
                
        int index = 0;
        foreach (var word in result)
        {
            array[index,0]=new Value(word.Key);
            array[index, 1] = new Value(word.Value);
            index++;
        }

    }


    #endregion (+) FBASIC Statements


    #region (+) Supporting methods 

    private static void placeHolderFilter(IInterpreter interpreter, bool dottedOnly)
    {
        // Syntax: PHSDATA array identifier_string_template
        //  
        FBasicArray array=null;

        interpreter.Match(Token.Identifier);
        string name = interpreter.lex.Identifier;

        if (interpreter.IsArray(name))
        {
            array = interpreter.GetArray(name);
            array.Reset();
            array.ResetArray();
        }
        else
        {
            array = new();
            interpreter.AddArray(name,array);
        }
        array.ResetColumnNames();
        array.SetColumnName(1,"ph");
        array.SetColumnName(2, "name");

        interpreter.GetNextToken(); // move to the first item of the collection

        interpreter.Match(Token.Identifier);
        string templateName = interpreter.lex.Identifier;
        interpreter.GetNextToken(); 

        var list=getUniquePlaceholders(getValueForPlaceHolder(interpreter,templateName), dottedOnly: dottedOnly, leftPartOnly:true);
        
        List<string> unique=new();
        int index = 0;
        foreach (var item in list)
        {
            if (!unique.Any(i => i == item))
            {
                unique.Add(item);
                array[index, 0] = new Value(item);  // set the item
                int p1 = item.IndexOf(':');
                if (p1 >= 0)
                {
                    array[index, 1] = new Value(item.Substring(0, p1));
                }
                else
                {
                    array[index, 1] = new Value(item);
                }
            }

            index++;
        }

        return;
    }

    /// <summary>
    /// Substitutes all placeholders in the input string with their actual values.
    /// </summary>
    /// <param name="input">The string containing placeholders.</param>
    /// <returns>The string with placeholders substituted.</returns>
    private static string substitutePlaceholders(IInterpreter interpreter, string input)
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

            string name = placeholder;
            int p1=name.IndexOf(':');
            if (p1>=0) name=name.Substring(0,p1);
            string value = getValueForPlaceHolder(interpreter, name) ?? "";
            return ToolKitHelper.FormatStringValue(value, placeholder);

        });

        return result;
    }

    /// <summary>
    /// This method should be implemented to return the value of the placeholder.
    /// </summary>
    private static string getValueForPlaceHolder(IInterpreter interpreter, string placeHolderName)
    {

        return interpreter.GetValue(placeHolderName).ToString();
    }


    /// <summary>
    /// Extracts all unique, unescaped placeholder names from the input text.
    /// </summary>
    /// <param name="text">The string content to search.</param>
    /// <returns>A List of unique placeholder names (e.g., "ph1", "ph2:1,10,c0").</returns>
    private static List<string> getUniquePlaceholders(string text, bool dottedOnly=false, bool leftPartOnly=false)
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

    #endregion (+) Supporting methods 

    #region (+) FBASIC Functions



    /// <summary>
    /// FBASIC Function PCase()
    /// </summary>
    private static Value PCase(IInterpreter interpreter, List<Value> args)
    {
        string syntax = "PCASE(string)";
        if (args.Count != 1)
            return interpreter.Error("PCASE", Errors.E125_WrongNumberOfArguments(1, syntax)).value;
        var value= ToolKitHelper.ToProperCase(args[0].String);
        return new Value(value);
    }

    /// <summary>
    /// FBASIC Function Words()
    /// </summary>
    /// <param name="interpreter"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private static Value Words(IInterpreter interpreter, List<Value> args)
    {
        // Syntax: words(s,1) : counts the number of words in a string s that are at least 1 character long.
        // if the input string is empty returns 0, if the minWordSize is less than 1 it is set to 1
        // The word delimiters are space, tab, newline and carriage-return. Consequent delimiters are treated as one.
        //
        string syntax = "words(string,minWordSize)";
        if (args.Count != 2)
            return interpreter.Error("WORDS", Errors.E125_WrongNumberOfArguments(2, syntax)).value;

        string str = args[0].Convert(FAST.FBasicInterpreter.ValueType.String).String;
        int size = args[1].ToInt();

        return new Value(CountLongWords(str, size));
    }

    private static Value ToName(IInterpreter interpreter, List<Value> args)
    {
        string syntax = "phtoname(string)";
        if (args.Count != 1)
            return interpreter.Error("phtoname", Errors.E125_WrongNumberOfArguments(1, syntax)).value;

        string str = args[0].Convert(FAST.FBasicInterpreter.ValueType.String).String;
        int p1=str.IndexOf(":");
        if (p1>=0)
        { // (v) remove the placeholder formatter 
            return new Value(str.Substring(0,p1));
        }
        else
        {
            return args[0];
        }
    }

    #endregion (+) FBASIC Functions

    #region (+) Public static methods 


    /// <summary>
    /// Counts the number of words in a given text string whose length is
    /// equal to or greater than the specified minimum word size.
    /// </summary>
    /// <param name="text">The input string to be analyzed. If null or whitespace, returns 0.</param>
    /// <param name="minWordSize">The minimum required length for a sequence of characters to be considered a 'word'.</param>
    /// <returns>The total number of words meeting the minimum size requirement.</returns>
    public static int CountLongWords(string text, int minWordSize)
    {
        if (minWordSize < 1) minWordSize = 1;

        // 1. Handle null or empty input gracefully, returning 0 as specified.
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        // This leverages the string.Split(char[], StringSplitOptions) overload.
        char[] separators = new char[] { ' ', '\t', '\n', '\r' };

        // 2. Use LINQ for a declarative and efficient counting process.
        // The approach involves:
        //    a. Splitting the string by whitespace characters (spaces, tabs, newlines).
        //    b. Removing any resulting empty entries (e.g., from multiple spaces)
        //       using StringSplitOptions.RemoveEmptyEntries.
        //    c. Trimming each word to ensure leading/trailing punctuation or non-word
        //       characters from the split process (though uncommon here) don't skew the count.
        //    d. Filtering the array to include only words where the length is 
        //       greater than or equal to minWordSize.
        //    e. Counting the final filtered collection.


        int count = text.Split(
            separator: separators,
            options: StringSplitOptions.RemoveEmptyEntries
        )
        .Where(word => word.Length >= minWordSize)
        .Count();

        return count;
    }


    /// <summary>
    /// Performs word frequency counting and filters the result based on 
    /// a minimum character length and a minimum occurrence count.
    /// </summary>
    /// <param name="input">The input string</param>
    /// <param name="minWordLength">The minimum length of a word</param>
    /// <param name="minCount">The minimum count in the result dictionary</param>
    /// <returns>Dictionary with the word as key and the count as an int. value</returns>
    public static Dictionary<string, int> CalculateWordFrequency(string input, int minWordLength, int minCount)
    {
        char[] separators = { ' ', ',', '.', ':', ';', '?', '\t', '\r', '\n' };

        // 1. Transformation (MAP) and Length Filtering:
        var allWords = input
            .ToLower()
            .Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.Trim())
            // **NEW FILTER: Filter out words shorter than the specified minLength**
            .Where(word => word.Length >= minWordLength);

        // 2. Aggregation (REDUCE) and Frequency Filtering (HAVING equivalent):
        var wordCounts = allWords
            .GroupBy(word => word)
            .Where(group => group.Count() >= minCount)
            .ToDictionary(
                group => group.Key,
                group => group.Count()
            );

        return wordCounts;
    }


    #endregion (+) Public static methods


}
