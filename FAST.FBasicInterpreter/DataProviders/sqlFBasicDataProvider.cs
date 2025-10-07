using System.Data.Common;

namespace FAST.FBasicInterpreter.DataProviders
{
    /// <summary>
    /// FBASIC SQL Data Provider 
    /// </summary>
    public class sqlFBasicDataProvider : IfbasicDataAdapter
    {
        public const string adapterName = "SQL";
        public string name => adapterName;

        public class cursorProperties
        {
            public bool eod = true;
            public string sqlText = "";
        }

        private Interpreter interpreter = null;
        public Dictionary<string, cursorProperties> cursors;

        public ErrorReturnClass Error(string source, string text) 
            => this.interpreter.Error(source, text);

        public void bind(Interpreter interpreter)
        {
            this.interpreter = interpreter;
            this.cursors = new();
            this.interpreter.AddStatement("CURSOR", CURSOR);
            this.interpreter.AddFunction("sql", SQL);
        }

        public bool openCursor(string cursorName, cursorCollection collection)
        {
            bool errorFound = false;

            try
            {
                var connectionObject = interpreter.RequestForObject(name, "CONNECTION", cursorName);
                if (connectionObject==null)
                {
                    return interpreter.Error(name,Errors.E128_RequestHandlerNullReturn(name,"CONNECTION",cursorName,"DbConnection object is expected (SqlConnection,OdbcConnect...etc")).falseError;
                }

                collection.channel = connectionObject as DbConnection; // to support all the types of connections (sql,odbc,etc)

                using var command = collection.channel.CreateCommand();
                collection.channel.Open();
                // (>) maybe to do something for the transactions here (see sqlChannel.executeAndGetReader() of FAST, maybe not.
                command.CommandText = cursors[cursorName].sqlText;
                command.CommandType = System.Data.CommandType.Text; // (!) check if need to do something different here 
                                                                    // (>) if pass parameters, this is a nice time to do it
                collection.reader = command.ExecuteReader();
            } 
            catch (fBasicException)
            {
                errorFound = true;
                throw;
            }
            catch (Exception ex)
            {
                return interpreter.Error(name,Errors.X012_GeneralException(name, ex)).falseError;
            }

            return !errorFound; //!collection.channel.hasError;
        }

        public void closeCursor(string cursorName, cursorCollection collection)
        {
            if (collection.channel == null ) return;
            collection.channel.Close();
        }

        public static void CURSOR(Interpreter interpreter)
        {
            // Syntax: CURSOR name,sql_command
            //  Used to set the cursor and the sql command or to re-set the sql command
            //
            interpreter.Match(Token.Identifier);
            string name = interpreter.lex.Identifier;

            interpreter.GetNextToken();
            interpreter.Match(Token.Comma);

            interpreter.GetNextToken();
            string sql=interpreter.Expr().ToString();
            //string sql = interpreter.lex.Value.String;

            //interpreter.GetNextToken();

            // (v) do the statement
            //var adapter = (sqlFBasicDataProvider)interpreter.dataAdapters[adapterName];
            var adapter = interpreter.GetDataAdapter<sqlFBasicDataProvider>(adapterName);
            if (!adapter.cursors.ContainsKey(name))
            {
                adapter.cursors.Add(name, new() { sqlText = sql });
                interpreter.AddCollection(name, new cursorCollection(name, adapter));
            }
            else
            {
                adapter.cursors[name].sqlText = sql; //reset the sql command
                interpreter.collections[name].Reset();
            }
        }
        public static Value SQL(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
            {
                interpreter.Error("SQL", "SQL() takes only one argument [S002]");
                return new Value(0); // not necessary as Error is exception.
            }

            switch (args[0].Type)
            {
                case ValueType.String:
                    string quote = "'";
                    return new Value(quote + args[0].String + quote);
                case ValueType.Real:
                    args[0] = args[0].Convert(ValueType.String);
                    return args[0];
                default:
                    interpreter.Error("SQL", $"Unsupported data type {args[0].Type.ToString()} [S001]");
                    break;
            }
            return args[0]; // will never reach this point
        }


    }

}
