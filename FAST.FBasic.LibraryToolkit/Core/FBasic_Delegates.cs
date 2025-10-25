﻿//
// Delegates used by the FBASIC Interpreter
//
namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The delegation type of the  printHandler
    /// </summary>
    /// <param name="text">The the to PRINT</param>
    public delegate void FBasicPrintFunction(string text);

    /// <summary>
    /// The delegation type of the input handler
    /// </summary>
    /// <returns>The INPUT</returns>
    public delegate string FBasicInputFunction();

    /// <summary>
    /// The delegation type of the load source for the CALL statement
    /// </summary>
    /// <param name="name">the name of the source program</param>
    /// <returns>the source</returns>
    [Obsolete("Use the IFBasicFileManagementLayer instead")]
    public delegate string FBasicSourceProgramLoader(string name);

    /// <summary>
    /// The delegation type of any file request
    /// </summary>
    /// <param name="file">The requested file</param>
    /// <returns>The file management layer</returns>
    public delegate IFBasicFileManagementLayer FBasicFileManagement(IFBasicFileDescriptor file);

    /// <summary>
    /// The delegation of object requesting
    /// </summary>
    /// <param name="context">The Context</param>
    /// <param name="group">The group of names</param>
    /// <param name="name">The name</param>
    /// <returns>object</returns>
    public delegate object FBasicRequestForObject(string context, string group, string name);


    /// <summary>
    /// A new function delegate
    /// </summary>
    /// <param name="interpreter">The instance of this interpreter</param>
    /// <param name="args">Arguments</param>
    /// <returns>The function result</returns>
    public delegate Value FBasicFunction(IInterpreter interpreter, List<Value> args);

    /// <summary>
    /// A new statement delegate
    /// </summary>
    /// <param name="interpreter">The instance of this interpreter</param>
    public delegate void FBasicStatement(IInterpreter interpreter);

    /// <summary>
    /// Delegate type for GetNextToken Method
    /// For future use
    /// </summary>
    /// <returns>Token</returns>
    public delegate Token FBasicGetNextTokenMethod();

}
