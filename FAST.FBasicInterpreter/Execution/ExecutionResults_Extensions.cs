namespace FAST.FBasicInterpreter.Execution
{
    /// <summary>
    /// Extension methods for ExecutionResult
    /// </summary>
    public static class ExecutionResults_Extensions
    {
        /// <summary>
        /// Check if the variable exists in the execution result.
        /// </summary>
        /// <param name="variableName">The variables name as it is in the FBASIC program</param>
        /// <returns></returns>
        public static bool IsVariable(this ExecutionResult result, string variableName)
        {
            return result.variables.ContainsKey(variableName);
        }

        /// <summary>
        /// Get the variable value from the execution result
        /// </summary>
        /// <param name="variableName">The variables name as it is in the FBASIC program</param>
        /// <returns></returns>
        public static Value GetVariable(this ExecutionResult result, string variableName)
        {
            if (result.variables.ContainsKey(variableName)) 
                return result.variables[variableName];
            return Value.Error;
        }

        /// <summary>
        /// Get the numeric variable value from the execution result
        /// </summary>
        /// <param name="variableName">The variables name as it is in the FBASIC program</param>
        /// <param name="defaultValue">Optional, the default value</param>
        /// <returns></returns>
        public static double GetNumericVariable(this ExecutionResult result, string variableName, double defaultValue=0)
        {
            if (result.variables.ContainsKey(variableName))
                return result.variables[variableName].Real;
            return defaultValue;
        }

        /// <summary>
        /// Get the integer variable value from the execution result
        /// </summary>
        /// <param name="variableName">The variables name as it is in the FBASIC program</param>
        /// <param name="defaultValue">Optional</param>
        /// <returns></returns>
        public static int GetIntVariable(this ExecutionResult result, string variableName, int defaultValue = 0)
        {
            if (result.variables.ContainsKey(variableName))
                return result.variables[variableName].ToInt();
            return defaultValue;
        }

        /// <summary>
        /// Get the string variable value from the execution result
        /// </summary>
        /// <param name="variableName">The variables name as it is in the FBASIC program</param>
        /// <param name="defaultValue">Optional</param>        /// <returns></returns>
        public static string GetStringVariable(this ExecutionResult result, string variableName, string defaultValue=null)
        {
            if (result.variables.ContainsKey(variableName))
                return result.variables[variableName].String;
            return defaultValue;
        }

        /// <summary>
        /// Get the boolean variable value from the execution result
        /// </summary>
        /// <param name="variableName">The variables name as it is in the FBASIC program</param>
        /// <param name="defaultValue">Optional</param>
        /// <returns></returns>
        public static bool GetBooleanVariable(this ExecutionResult result, string variableName, bool defaultValue = false)
        {
            if (result.variables.ContainsKey(variableName))
                return result.variables[variableName].ToBool();
            return defaultValue;
        }

    }
}
