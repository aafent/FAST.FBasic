namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Methods that offering functionality to the FBasic Library
    /// </summary>
    public static class ToolKitHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="placeholder"></param>
        /// <returns></returns>
        public static string FormatStringValue(string placeholder, string value )
        {
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

                if (modifiers.Contains("D"))
                {
                    // Date: YYYY-MM-DD to DD-MM-YYYY 
                    if (value.Length >= 10 )
                    {
                        value = value.Substring(8, 2) + value[4]
                              + value.Substring(5,2) + value[4]
                              + value.Substring(0,4) 
                              + value.Substring(10);
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
                    if (modifiers.Contains("l") && modifiers.Contains("r")) // center
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
}
