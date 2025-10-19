namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Provides Decision Table functionality.
    /// 
    /// Statements:
    /// DTDIM name, not_found_Value,factor1[, factor2,] ... [,factorN]
    /// DTROW name, [factor1], [factor2], ..., [factorN], value
    /// DTFIND name, resultFactor, VariableForTheFoundValue, [factor1], [factor2], ..., [factorN]
    /// DTMAPPER name, not_found_value, key_factor, value_factor
    /// 
    /// map(DecTable,inValue) :: returns the mapped value or the default if not found
    /// dtmap(DecTable,value,key_factor_name,value_factor_name) :: returns the mapped value or the default if not found
    /// See: https://github.com/aafent/FAST.FBasic/wiki/Language-Reference,-Decision-Tables
    /// Variables:
    /// FOUND - TRUE if a matching row was found, otherwise FALSE, Value change from DTFIND statement and the map(),dcmap() functions
    /// 
    /// </summary>
    public partial class FBasicDecisionTables : IFBasicLibraryWithMemory
    {
        private Dictionary<string, decTable> decTables = new();

        public string uniqueName => "DecisionTables";

        public void InstallAll(Interpreter interpreter)
        {
            interpreter.AddStatement("DTDIM", DecisionTableDim);
            interpreter.AddStatement("DTROW", DecisionTableRow);
            interpreter.AddStatement("DTFIND", DecisionTableFind);
            interpreter.AddStatement("DTMAPPER", DecisionTableMapper);

            interpreter.AddFunction("map", DTMAPSimple);
            interpreter.AddFunction("dtmap", DTMAPFull);

            interpreter.SetVar("FOUND",Value.False);
        }

        private Value DTMAPSimple(Interpreter interpreter, List<Value> args)
        {
            const string syntax = "map(DecTable,inValue)";
            if (args.Count != 2)
                return interpreter.Error("MAP", Errors.E125_WrongNumberOfArguments(2, syntax)).value;

            if (args[0].Type != ValueType.String )
                return interpreter.Error("MAP", Errors.E126_WrongArgumentType(syntax: syntax)).value;

            string dcTable = args[0].String;
            var inValue=args[1]; 

            if (!decTables.TryGetValue(dcTable, out decTable table ))
            {
                return interpreter.Error("MAP", Errors.E112_UndeclaredEntity("DecisionTable",dcTable)).value;
            }

            // (v) Set the search criteria 
            table.search.Clear();
            foreach (var factor in table.factors)
            {
                if (factor == table.mapperKeyFactor)
                {
                    table.search.Add(factor, inValue);
                }
                else
                {
                    table.search.Add(factor, Value.Wildcard);
                }
            }

            // Do the search for each table row 
            foreach ( var item in table.data)
            {
                if (table.Match(table.mapperValueFactorIndex, item) )
                {
                    interpreter.SetVar("FOUND", Value.True);
                    return item[table.mapperValueFactorIndex].value;
                }
            }
            
            interpreter.SetVar("FOUND",Value.False);
            return table.mapperOtherwise;
        }

        private Value DTMAPFull(Interpreter interpreter, List<Value> args)
        {
            const string syntax = "dtmap(DTable,value,key_factor_name,value_factor_name)";
            if (args.Count != 4)
                return interpreter.Error("DTMAP", Errors.E125_WrongNumberOfArguments(4, syntax)).value;

            if (args[0].Type != ValueType.String)
                return interpreter.Error("DTMAP", Errors.E126_WrongArgumentType(syntax: syntax)).value;

            string dcTable = args[0].String;
            var inValue = args[1];

            if (args[2].Type != ValueType.String)
                return interpreter.Error("DTMAP", Errors.E126_WrongArgumentType(3, syntax)).value;
            if (args[3].Type != ValueType.String)
                return interpreter.Error("DTMAP", Errors.E126_WrongArgumentType(3, syntax)).value;

            var keyFactor = args[2].String;
            var valueFactor = args[3].String;

            if (!decTables.TryGetValue(dcTable, out decTable table))
            {
                return interpreter.Error("DTMAP", Errors.E112_UndeclaredEntity("DecisionTable", dcTable)).value;
            }

            var valueFactorIndex = Array.IndexOf(table.factors, valueFactor);
            if ( valueFactorIndex<0 )
            {
                return interpreter.Error("DTMAP", Errors.E112_UndeclaredEntity("Value search factor", valueFactor, "Check the spelling and the name capitalization")).value;
            }


            // (v) Set the search criteria 
            table.search.Clear();
            foreach (var factor in table.factors)
            {
                if (factor == keyFactor)
                {
                    table.search.Add(factor, inValue);
                }
                else
                {
                    table.search.Add(factor, Value.Wildcard);
                }
            }

            // Do the search for each table row 
            foreach (var item in table.data)
            {
                if (table.Match(valueFactorIndex, item))
                {
                    interpreter.SetVar("FOUND", Value.True);
                    return item[valueFactorIndex].value;
                }
            }

            interpreter.SetVar("FOUND", Value.False);
            return table.mapperOtherwise;
        }


        private void DecisionTableFind(Interpreter interpreter)
        {
            // Syntax   DTFIND name, resultFactor, VariableForTheFoundValue, [factor1], [factor2], ..., [factorN]
            //          DTFIND CreditCheck, Eligible, result, 30, 50000, "Permanent"
            //          if a * provided as factor, it is acting as Wildcard, and practical means to igone the factor on search
            //

            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string dtName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.Match(Token.Comma);
            interpreter.GetNextToken();

            // (v) argument: resultFactor
            interpreter.Match(Token.Identifier);
            string resultFactor = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.Match(Token.Comma);
            interpreter.GetNextToken();

            // (v) argument: VariableForTheFoundValue
            interpreter.Match(Token.Identifier);
            string resultVariable = interpreter.lex.Identifier;
            interpreter.GetNextToken();


            // (v) arguments: factor1 ... factorN
            var factorsValues = new List<Value>();
            while (true)
            {
                interpreter.Match(Token.Comma);
                interpreter.GetNextToken();

                Value factorValue;
                switch (interpreter.lastToken)
                {
                    case Token.Value:
                        factorValue = interpreter.lex.Value;
                        break;
                    case Token.Identifier:
                        factorValue = interpreter.GetValue(interpreter.lex.Identifier);
                        break;
                    case Token.Asterisk:
                        factorValue = Value.Wildcard;
                        break;

                    default:
                        interpreter.Match(Token.Value); // (<) will raise error
                        return;
                };
                factorsValues.Add(factorValue);

                if (interpreter.GetNextToken() == Token.NewLine) break;
            }

            if (factorsValues.Count == 0)
            {
                interpreter.Match(Token.Comma); // (<) will raise error
                return; // just to satisfy the compiler
            }

            // (v) process arguments
            if (!decTables.ContainsKey(dtName))
            {
                interpreter.Error("DTFIND", Errors.E112_UndeclaredEntity($"Decision table", dtName));
                return;
            }

            var table = decTables[dtName]; // reference to the dictionary item for performance 

            if ((table.count - 1) != factorsValues.Count)
            {
                interpreter.Error("DTROW", Errors.E112_UndeclaredEntity("Factors of Decision Table", dtName, $", Expecting:{ (table.count-1) } got {factorsValues.Count}"));
                return;
            }


            // (v) set the search values
            table.PrepareSearch();
            int valuesInx=0;
            for (int inx=0;inx<table.factors.Length;inx++)
            {
                var factorName = table.factors[inx];
                if (factorName == resultFactor) continue; // skip the result factor
                
                if (table.search.ContainsKey(factorName))
                {
                    table.search[factorName] = factorsValues[valuesInx];
                }
                else
                {
                    table.search.Add(factorName, factorsValues[valuesInx]);
                }
                valuesInx++;
            }

            if ( table.TryFindMatchingRow(resultFactor, out var foundValue))
            {
                interpreter.SetVar("FOUND",Value.True);
                interpreter.SetVar(resultVariable,foundValue.value);
            }
            else
            {
                interpreter.SetVar("FOUND", Value.False);
                interpreter.SetVar(resultVariable,table.otherwise);
            }
        }

        private void DecisionTableRow(Interpreter interpreter)
        {
            // Syntax DTROW name, [factor1], [factor2], ..., [factorN], value
            //

            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string dtName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            // (v) arguments: value, factor1 ... factorN
            bool secondValue=false;
            var factorsValues = new List<cellValue>();
            operType oper = operType.Equal;
            while (true)
            {
                interpreter.Match(Token.Comma);
                interpreter.GetNextToken();

                cellValue factorValue=null;
                int valueFactor = 1;

                CheckNextToken:
                switch (interpreter.lastToken)
                {
                    #region (+) CASE Operators
                    case Token.Equal:
                        if (secondValue) interpreter.Match(Token.Value); // (<) will raise error
                        oper = operType.Equal;
                        interpreter.GetNextToken();
                        goto CheckNextToken;
                    case Token.MoreEqual:
                        if (secondValue) interpreter.Match(Token.Value); // (<) will raise error
                        oper = operType.GreaterOrEqual;
                        interpreter.GetNextToken();
                        goto CheckNextToken;
                    case Token.LessEqual:
                        if (secondValue) interpreter.Match(Token.Value); // (<) will raise error
                        oper = operType.LessOrEqual;
                        interpreter.GetNextToken();
                        goto CheckNextToken;
                    case Token.Less:
                        if (secondValue) interpreter.Match(Token.Value); // (<) will raise error
                        oper = operType.LessThan;
                        interpreter.GetNextToken();
                        goto CheckNextToken;
                    case Token.More:
                        if (secondValue) interpreter.Match(Token.Value); // (<) will raise error
                        oper = operType.GraterThan;
                        interpreter.GetNextToken();
                        goto CheckNextToken;
                    #endregion (+) CASE Operators

                    case Token.Minus:
                        valueFactor=(-1);
                        interpreter.GetNextToken();
                        goto CheckNextToken;
                    case Token.Value:
                        if (secondValue)
                        {
                            factorValue.value2=interpreter.lex.Value;
                            if (factorValue.value2.Type == ValueType.Real) factorValue.value2.Real *= valueFactor;
                            factorValue.oper = operType.Between;
                            break;
                        }
                        else
                        {
                            factorValue = new cellValue(interpreter.lex.Value); // new instance
                            if (factorValue.value.Type == ValueType.Real) factorValue.value.Real *= valueFactor;
                            break;
                        }
                    case Token.Identifier:
                        if (secondValue)
                        {
                            factorValue.value2 = interpreter.GetValue(interpreter.lex.Identifier);
                            factorValue.oper = operType.Between;
                            break;
                        }
                        else
                        {
                            factorValue = new cellValue(interpreter.GetValue(interpreter.lex.Identifier)); // new instance
                            break;
                        }
                    default:
                        interpreter.Match(Token.Value); // (<) will raise error
                        return;
                }

                // (v) add the value
                if (!secondValue) // factorValue is already added by the first value
                {
                    factorValue.oper = oper;
                    factorsValues.Add(factorValue);
                }
                // (v) reset values
                oper = operType.Equal;
                secondValue=false;

                var prevToken = interpreter.lastToken;
                if (interpreter.GetNextToken() == Token.NewLine) break;
                if (interpreter.lastToken == Token.Colon && (prevToken == Token.Identifier || prevToken == Token.Value)  )
                {
                    interpreter.GetNextToken();
                    secondValue = true;
                    goto CheckNextToken; 
                }
            }

            if (factorsValues.Count == 0)
            {
                interpreter.Match(Token.Comma); // (<) will raise error
                return; // just to satisfy the compiler
            }

            // (v) process arguments
            if (!decTables.ContainsKey(dtName))
            {
                interpreter.Error("DTROW", Errors.E133_AlreadyDefined($"Decision table {dtName}"));
                return;
            }

            var table = decTables[dtName]; // reference to the dictionary item for performance 


            if ((table.count) != factorsValues.Count)
            {
                interpreter.Error("DTROW", Errors.E112_UndeclaredEntity("Factors of Decision Table",dtName,$", Expecting:{table.count} got {factorsValues.Count}"));
                return;
            }
        
            table.data.Add(factorsValues.ToArray());
        }

        private void DecisionTableDim(Interpreter interpreter)
        {
            // Syntax DTDIM name, not_found_Value  ,[factor1], [factor2], ..., [factorN]
            //
            Value otherwise;

            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string dtName = interpreter.lex.Identifier;
            interpreter.GetNextToken();
            interpreter.Match(Token.Comma);
            interpreter.GetNextToken();

            // (v) argument: not_found_Value
            switch (interpreter.lastToken)
            {
                case Token.Value:
                    otherwise = interpreter.lex.Value;
                    interpreter.GetNextToken();
                    break;
                case Token.Identifier:
                    otherwise = interpreter.GetValue(interpreter.lex.Identifier);
                    interpreter.GetNextToken();
                    break;
                case Token.Comma: // (<) the value is omitted
                    otherwise = Value.Zero;
                    break;
                default:
                    interpreter.Match(Token.Value); // (<) will raise error
                    return;
            }

            // (v) arguments: factor1 ... factorN
            var factors = new List<string>();
            while (true)
            {
                interpreter.Match(Token.Comma);

                interpreter.GetNextToken();
                interpreter.Match(Token.Identifier);
                var factor = interpreter.lex.Identifier;

                factors.Add(factor);

                if (interpreter.GetNextToken() == Token.NewLine) break;
            }

            if (factors.Count == 0)
            {
                interpreter.Match(Token.Comma); // (<) will raise error
                return; // just to satisfy the compiler
            }


            // (v) process arguments
            if (decTables.ContainsKey(dtName))
            {
                interpreter.Error("DTDIM",Errors.E133_AlreadyDefined($"Decision table {dtName}") );
                return;
            }

            decTable table = new();
            table.otherwise= otherwise;
            table.factors = factors.ToArray();
            table.count = factors.Count;
            table.search = new();
            table.data = new();
            table.mapperKeyFactor = table.factors[0];
            table.mapperValueFactor = table.factors[1];
            table.mapperKeyFactorIndex = Array.IndexOf(table.factors, table.mapperKeyFactor);
            table.mapperValueFactorIndex= Array.IndexOf(table.factors, table.mapperValueFactor);
            table.mapperOtherwise = table.otherwise;
            decTables.Add(dtName, table);
        }

        private void DecisionTableMapper(Interpreter interpreter)
        {
            // Syntax DTMAPPER name, not_found_value, key_factor, value_factor
            //        example: dtmapper M1, "", inValue, BuyerPower
            //
            Value otherwise;

            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string dtName = interpreter.lex.Identifier;
            interpreter.GetNextToken();
            interpreter.Match(Token.Comma);
            interpreter.GetNextToken();

            // (v) argument: not_found_Value
            switch (interpreter.lastToken)
            {
                case Token.Value:
                    otherwise = interpreter.lex.Value;
                    interpreter.GetNextToken();
                    break;
                case Token.Identifier:
                    otherwise = interpreter.GetValue(interpreter.lex.Identifier);
                    interpreter.GetNextToken();
                    break;
                case Token.Comma: // (<) the value is omitted
                    otherwise = Value.Zero;
                    break;
                default:
                    interpreter.Match(Token.Value); // (<) will raise error
                    return;
            }

            interpreter.Match(Token.Comma);

            interpreter.GetNextToken();
            interpreter.Match(Token.Identifier);
            var keyFactor = interpreter.lex.Identifier;

            interpreter.GetNextToken();
            interpreter.Match(Token.Comma);

            interpreter.GetNextToken();
            interpreter.Match(Token.Identifier);
            var valueFactor = interpreter.lex.Identifier;

            interpreter.GetNextToken();


            // (v) process arguments
            if (!decTables.ContainsKey(dtName))
            {
                interpreter.Error("DTMAPPER", Errors.E133_AlreadyDefined($"Decision table {dtName}"));
                return;
            }

            if (!decTables.TryGetValue(dtName, out decTable table))
            {
                interpreter.Error("DTMAPPER", Errors.E112_UndeclaredEntity("DecisionTable", dtName));
                return; // just to satisfy the compiler
            }

            int keyFactorIndex = Array.IndexOf(table.factors, keyFactor);
            int valueFactorIndex = Array.IndexOf(table.factors, valueFactor);

            if ( (valueFactorIndex < 0) || (keyFactorIndex < 0) )
            {
                interpreter.Error("DTMAPPER", Errors.E112_UndeclaredEntity("Key or Value factor", keyFactor+"/"+valueFactor, "Check the spelling and the name capitalization"));
                return; // just to satisfy the compiler
            }


            table.mapperKeyFactor = keyFactor;
            table.mapperValueFactor = valueFactor;
            table.mapperKeyFactorIndex = Array.IndexOf(table.factors, table.mapperKeyFactor);
            table.mapperValueFactorIndex = Array.IndexOf(table.factors, table.mapperValueFactor);
            table.mapperOtherwise = otherwise;
        }



        /// <summary>
        /// Clear the stack on program restart/execute
        /// </summary>
        public void ClearMemory()
        {
            decTables.Clear();
        }
    }

}