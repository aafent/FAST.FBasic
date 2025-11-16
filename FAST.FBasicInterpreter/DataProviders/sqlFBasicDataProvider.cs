using System.Data;
using System.Data.Common;

namespace FAST.FBasicInterpreter.DataProviders
{
    /// <summary>
    /// FBASIC SQL Data Provider 
    /// </summary>
    public class sqlFBasicDataProvider : IFBasicDataAdapter
    {
        public const string adapterName = "SQL";
        public string name => adapterName;

        public class cursorProperties
        {
            public bool eod = true;
            public string sqlText = "";
        }

        private IInterpreter interpreter = null;
        public Dictionary<string, cursorProperties> cursors;

        public ErrorReturnClass Error(string source, string text) 
            => this.interpreter.Error(source, text);

        public void bind(IInterpreter interpreter)
        {
            this.interpreter = interpreter;
            this.cursors = new();
            this.interpreter.AddStatement("CURSOR", CURSOR);
            this.interpreter.AddStatement("RETRIEVE", RETRIEVE);
            this.interpreter.AddFunction("sql", SQL);
        }

        public bool openCursor(string cursorName, cursorCollection collection)
        {
            bool errorFound = false;

            try
            {
                var request=new FBasicRequestForObjectDescriptor(this.interpreter,name, "CONNECTION",cursorName);
                var connectionObject = interpreter.RequestForObject(request);  // was:FBasicRequestForObjectDescriptor
                if (connectionObject==null)
                {
                    return interpreter.Error(name,Errors.E128_RequestHandlerNullReturn(name,"CONNECTION",cursorName,"DbConnection object is expected (SqlConnection,OdbcConnect...etc")).falseError;
                }

                collection.channel = connectionObject as DbConnection; // to support all the types of connections (sql,odbc,etc)
                collection.channel.Open();

                collection.command = collection.channel.CreateCommand();
                // (>) maybe to do something for the transactions here (see sqlChannel.executeAndGetReader() of FAST, maybe not.
                collection.command.CommandText = cursors[cursorName].sqlText;
                collection.command.CommandType = System.Data.CommandType.Text; // (!) check if need to do something different here 
                // (>) if pass parameters, this is a nice time to do it
                collection.reader = collection.command.ExecuteReader();

            } 
            catch (FBasicException)
            {
                errorFound = true;
                throw;
            }
            catch (Exception ex)
            {
                return interpreter.Error(name,Errors.X012_GeneralException(name, ex)).falseError;
            }

            if (errorFound) return false; // No open due errors

            return !collection.reader.IsClosed;
        }

        public void closeCursor(string cursorName, cursorCollection collection)
        {
            if (collection.channel == null ) return;
            collection.channel.Close();
        }



        public static void CURSOR(IInterpreter interpreter)
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
                interpreter.GetCollection(name).Reset();
            }
        }

        public static void RETRIEVE(IInterpreter interpreter)
        {
            // Syntax: RETRIEVE array_name, NEW|APPEND, number|*, SQL Data retrieval statement
            //  Used to retrieve rows and place them in an array
            //
            interpreter.Match(Token.Identifier);           
            string name = interpreter.lex.Identifier;

            interpreter.GetNextToken();
            interpreter.Match(Token.Comma);

            interpreter.GetNextToken();
            interpreter.Match(Token.Identifier);
            string rowSet = interpreter.lex.Identifier.ToUpper();
            if (rowSet!="NEW" && rowSet!="APPEND")
            {
                interpreter.Error(adapterName, Errors.E106_ExpectingKeyword(rowSet,"Only NEW or APPEND is expected.") );
                return;
            }

            interpreter.GetNextToken();
            interpreter.Match(Token.Comma);

            interpreter.GetNextToken();
            int maxRows=0;
            interpreter.MatchAny(Token.Identifier, Token.Value,Token.Asterisk);
            if (interpreter.LastToken == Token.Asterisk)
            {
                maxRows=0;
            }
            else
            {
                maxRows=interpreter.ValueOrVariable().ToInt();
            }

            interpreter.GetNextToken();
            interpreter.Match(Token.Comma);

            interpreter.GetNextToken();
            string sql = interpreter.Expr().ToString();

            // (v) do the statement
            var adapter = interpreter.GetDataAdapter<sqlFBasicDataProvider>(adapterName);
            IBasicCollection collection;
            if (!adapter.cursors.ContainsKey(name))
            {
                adapter.cursors.Add(name, new() { sqlText = sql });
                collection=new cursorCollection(name, adapter);
                interpreter.AddCollection(name, collection);
            }
            else
            {
                adapter.cursors[name].sqlText = sql; //reset the sql command
                collection = new cursorCollection(name, adapter);
                collection.ClearCollection();
            }
            collection.MoveNext();


            if (collection.Current is IDataRecord data)
            {
                FBasicArray array;
                if (interpreter.IsArray(name))
                {
                    array = interpreter.GetArray(name);
                    if (rowSet == "NEW") array.ResetArray();
                }
                else
                {
                    array=new();
                    interpreter.AddArray(name, array);
                }
                array.SetColumnNamesFrom(data);
  
                int row;
                if (rowSet == "APPEND")
                {
                    row=array.Length+1;
                }
                else
                { 
                    row = array.GetCurrentRow();
                }
                
                if (maxRows == 0 ) maxRows=int.MaxValue;
                for (int times = 1; times<=maxRows; times++)
                {
                    for (int inx = 0; inx < array.ColumnNamesCount; inx++)
                    {
                        array[row - 1 + (times-1), inx] = ToolKitHelper.ToValue(data[inx]);
                    }

                    collection.MoveNext();
                    if (collection.endOfData) break; // stop early (before maxRow) as no more datas
                }

                interpreter.DropCollection(name);
            }

        }
        public static Value SQL(IInterpreter interpreter, List<Value> args)
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
