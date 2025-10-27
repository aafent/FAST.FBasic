//
// Statements: 
// COLDIM name, [ColumnName1], [ColumnName2], ..., [ColumnNameN]
// COLNAME array_name, column_number, column_name
// ADDCOL array_name, column_name
// INDEXDELETE array_name, index
//
// Functions:
// count(array_name)
// cname(array_name,column_index)
// colnamescount(array_name)
// 
// Reference:
// [array.column(index)] 
// 
// index and column_number are 1-base number
//


namespace FAST.FBasicInterpreter
{
    public partial class FBasic2DArrays  : IFBasicLibrary
    {
        public string uniqueName => "Arrays2D";

        public void InstallAll(IInterpreter interpreter)
        {
            interpreter.AddStatement("COLDIM", JaggedArrayDim);
            interpreter.AddStatement("COLNAME", ColumnName);
            interpreter.AddStatement("COLADD", ColumnAddName);
            interpreter.AddStatement("INDEXDELETE", IndexDelete);

            interpreter.AddFunction("cnamescount", ColumnNamesCount);
            interpreter.AddFunction("cname", ColumnName);
            

        }

        private void JaggedArrayDim(IInterpreter interpreter)
        {
            //
            // Syntax JADIM name, [ColumnName1], [ColumnName2], ..., [ColumnNameN]
            //

            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string arrayName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            // (v) arguments: column1...columnN
            var columns= new List<string>();
            while (true)
            {
                interpreter.Match(Token.Comma);

                interpreter.GetNextToken();
                interpreter.Match(Token.Identifier);
                var colName = interpreter.lex.Identifier;

                columns.Add(colName);

                if (interpreter.GetNextToken() == Token.NewLine) break;
            }

            // (v) process arguments
            FBasicArray array = new();
            int colNo=0;
            foreach(var col in columns )
            {
                colNo++;
                array.SetColumnName(colNo, col);
            }
            interpreter.AddArray(arrayName, array);
            
            return;
        }

        private void ColumnName(IInterpreter interpreter)
        {
            //
            // Syntax COLNAME array_name, column_number, column_name
            //

            // (v) argument: array_name
            interpreter.Match(Token.Identifier);
            string arrayName = interpreter.lex.Identifier;
            interpreter.GetNextToken();
            interpreter.Match(Token.Comma);

            // (v) argument: column_number
            int columnNo=0;
            switch (interpreter.GetNextToken())
            {
                case Token.Value:
                    columnNo = interpreter.lex.Value.ToInt();
                    interpreter.GetNextToken();
                    break;
                case Token.Identifier:
                    columnNo = interpreter.GetValue(interpreter.lex.Identifier).ToInt();
                    interpreter.GetNextToken();
                    break;
                default:
                    interpreter.Match(Token.Value); // (<) will raise error
                    return;
            }

            // (v) argument: column_name
            interpreter.Match(Token.Comma);
            string colName="";
            switch (interpreter.GetNextToken())
            {
                case Token.Value:
                    colName= interpreter.lex.Value.String;
                    interpreter.GetNextToken();
                    break;
                case Token.Identifier:
                    colName = interpreter.GetValue(interpreter.lex.Identifier).String;
                    interpreter.GetNextToken();
                    break;
                default:
                    interpreter.Match(Token.Value); // (<) will raise error
                    return;
            }

            // (v) process arguments
            FBasicArray array = interpreter.GetArray(arrayName);
            array.SetColumnName(columnNo, colName);

            return;
        }

        private void ColumnAddName(IInterpreter interpreter)
        {
            //
            // Syntax ADDCOL array_name, column_name
            //

            // (v) argument: array_name
            interpreter.Match(Token.Identifier);
            string arrayName = interpreter.lex.Identifier;
            FBasicArray array = interpreter.GetArray(arrayName);
            interpreter.GetNextToken();

            // (v) argument: column_name
            interpreter.Match(Token.Comma);
            string colName = "";
            switch (interpreter.GetNextToken())
            {
                case Token.Value:
                    if (interpreter.lex.Value.Type == ValueType.Real)
                    {
                        colName = array.GetColumnName(interpreter.lex.Value.ToInt());
                    }
                    else
                    {
                        colName = interpreter.lex.Value.String;
                    }

                    interpreter.GetNextToken();
                    break;
                case Token.Identifier:
                    colName = interpreter.lex.Identifier;
                    interpreter.GetNextToken();
                    break;
                default:
                    interpreter.Match(Token.Value); // (<) will raise error
                    return;
            }

            // (v) process arguments
            
            array.SetColumnName(array.ColumnNamesCount + 1, colName);

            return;
        }

        private void IndexDelete(IInterpreter interpreter)
        {
            //
            // Syntax ROWDELETE array_name, row_number
            //

            // (v) argument: array_name
            interpreter.Match(Token.Identifier);
            string arrayName = interpreter.lex.Identifier;
            FBasicArray array = interpreter.GetArray(arrayName);
            interpreter.GetNextToken();

            // (v) argument: column_name
            interpreter.Match(Token.Comma);
            interpreter.GetNextToken();

            var row=interpreter.Expr().ToInt();

            // (v) process arguments
            array.DeleteRow(row);
            return;
        }

        private Value ColumnName(IInterpreter interpreter, List<Value> args)
        {
            string syntax = "cname(array_name,column_index)";
            if (args.Count != 2)
                return interpreter.Error("cname", Errors.E125_WrongNumberOfArguments(2, syntax)).value;

            var arrayName = args[0].String;
            FBasicArray array = interpreter.GetArray(arrayName);

            var index = args[1].ToInt();
            return new Value(array.GetColumnName(index));
        }

        private Value ColumnNamesCount(IInterpreter interpreter, List<Value> args)
        {
            string syntax = "colnamescount(array_name)";
            if (args.Count != 1)
                return interpreter.Error("colnamescount", Errors.E125_WrongNumberOfArguments(1, syntax)).value;

            var arrayName = args[0].String;
            FBasicArray array = interpreter.GetArray(arrayName);
            return new Value(array.ColumnNamesCount);
        }

    }

}