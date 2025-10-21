using System.Text;

namespace FAST.FBasicInterpreter
{
    public class storedProcedureBuilder : sourceCodeBuilderAbstract
    {
        private string source=null;
        protected override ProgramContainer program { get; set; }

        public override void Build(ProgramContainer program)
        {
            this.program=program;
            this.source=toSource();
        }

        public override string GetSource()
        {
            return source;
        }


        private string sourceOf(Token token)
        {
            switch (token)
            {
                case Token.Equal:
                    return "=";
                case Token.Plus:
                    return "+";
                case Token.Minus:
                    return "-";
                case Token.Slash:
                    return "/";
                case Token.Asterisk:
                    return "*";
                case Token.Caret:
                    return "^";
                case Token.Less:
                    return "<";
                case Token.More:
                    return ">";
                case Token.NotEqual:
                    return "<>";
                case Token.LessEqual:
                    return "<=";
                case Token.MoreEqual:
                    return ">=";


                case Token.LParen:
                    return "(";
                case Token.RParen:
                    return ")";

                case Token.Colon:
                    return ":";
                case Token.Semicolon:
                    return ";";
                case Token.Comma:
                    return ",";



                default:
                    return string.Empty;
            }
        }

        private string toSource()
        {
            string spacer = " ";
            string quote = "\'";
            StringBuilder src = new();

            // (v) starting elements
            src.AppendLine("BEGIN --program");
            src.AppendLine("declare @ASSERTVALUE int,");
            src.AppendLine("        @RESULTVALUE int,");
            src.AppendLine("        @msg varchar(255)");
            src.AppendLine("set @ASSERTVALUE=null");
            src.AppendLine("set @RESULTVALUE=0");
            src.AppendLine();

            // (v) parsing the program
            bool done = false;
            int tokensCount = 0;
            ProgramElement prevItem = new() {  token= Token.Unknown };
            foreach (var item in program.elements)
            {
                tokensCount++;

                if (tokensCount==1) // (<) on the first time
                {
                }

                if (InterpreterHelper.IsStatement(item.token))
                {
                    lineAnalysis(tokensCount);
                    if (hasLogicalOperators) src.Append("LOGICAL: ");
                    src.Append(numOfEqualSigns);
                    src.Append(": ");
                }
                switch (item.token)
                {
                    case Token.Colon:
                        bool colonFound=false;
                        if (prevItem.token==Token.Goto ||
                            prevItem.token==Token.Identifier
                            )
                        {
                            src.AppendLine(":");
                            colonFound = true;
                        }
                        if (!colonFound) src.Append(readableElement(item));
                        break;

                    case Token.Semicolon:
                        src.Append(readableElement(item));  
                        break;

                    case Token.Equal:
                        src.Append(" = ");
                        if (hasLogicalOperators)
                        {
                            src.Append("case when ");
                        }
                        break;


                    case Token.Plus:
                    case Token.Minus:
                    case Token.Slash:
                    case Token.Asterisk:
                    case Token.Caret:
                    case Token.Less:
                    case Token.More:
                    case Token.NotEqual:
                    case Token.LessEqual:
                    case Token.MoreEqual:
                    case Token.Or:
                    case Token.And:
                    case Token.Not:
                    //
                    case Token.LParen:
                    case Token.RParen:
                        //
                        var text = sourceOf(item.token);
                        if (string.IsNullOrEmpty(text)) throw new Exception($"No source for: {item.token}");
                        src.Append(spacer);
                        src.Append(text);
                        src.Append(spacer);
                        break;


                    case Token.If:
                        src.AppendLine();
                        src.Append("if ");
                        break;

                    case Token.Then:
                        src.AppendLine();
                        src.AppendLine("begin --{");
                        break;

                    case Token.Else:
                        src.AppendLine("end --}");
                        src.AppendLine("else begin --{");
                        break;

                    case Token.EndIf:
                        src.AppendLine("end --}");
                        src.AppendLine();
                        break;

                    case Token.Goto:
                        src.Append("goto ");
                        break;

                    case Token.End:  // END, HALT
                        src.AppendLine("goto proc_exit");
                        break;

                    case Token.Let:
                        src.Append("set ");
                        break;

                    case Token.Result:
                        src.Append("set @RESULTVALUE=");
                        break;

                    case Token.Assert:
                        makeReference(item.token);
                        src.Append("set @ASSERTVALUE=");
                        break;

                    case Token.Print:
                        src.Append("print ");
                        break;

                    case Token.EOF:
                        done = true;
                        break;

                    case Token.NewLine:
                        if (hasLogicalOperators)
                        {
                            src.Append(" then 0 else 1 end ");
                        }
                        if (isReferenced(Token.Assert))
                        {
                            src.AppendLine();
                            src.AppendLine("set @msg='ASSERT:'+case @ASSERTVALUE when 1 then 'Ok' else 'Fail' end");
                            src.AppendLine("print @msg");
                            removeReference(Token.Assert);
                        }
                        if (prevItem.token!=Token.NewLine) src.AppendLine();
                        break;

                    // (v) not implemented in SPs
                    case Token.Comma:
                    case Token.Rem:
                    case Token.Dump:
                        break;



                    case Token.For:
                        src.Append(readableElement(item));
                        break;

                    case Token.To:
                        src.Append(readableElement(item));
                        break;

                    case Token.Next:
                        src.Append(readableElement(item));
                        break;

                    case Token.Gosub:
                    case Token.Return:
                        src.Append(readableElement(item));
                        break;


                    case Token.Call:
                    case Token.Input:
                    case Token.Identifier:
                        if (item.isDoted)
                        {
                            src.Append('[');
                            src.Append(item.identifier);
                            src.Append(']');
                        }
                        else
                        {
                            if ( program.variables.ContainsKey(item.identifier))
                            {
                                src.Append("@");
                                src.Append(item.identifier);
                            }
                            else
                            { 
                                src.Append(item.identifier);
                            }
                        }
                        break;

                    case Token.AddOn:
                        src.Append(item.identifier);
                        src.Append(spacer);
                        break;


                    case Token.Value:
                        switch (item.value.Type)
                        {
                            case ValueType.String:
                                src.Append(quote);
                                src.Append(item.value);
                                src.Append(quote);
                                break;
                            case ValueType.Real:
                                src.Append(item.value);
                                break;
                            default:
                                throw new Exception($"Uknown value type: {item.value.Type}");
                        }

                        break;


                    default:
                        throw new Exception(string.Format($"Uknown Token: "+readableElement(item)));
                }
                if (done) break;


                // (v) before reloop
                prevItem.CopyFrom(item);
            }


            // (v) ending elements
            src.AppendLine("proc_exit:");
            src.AppendLine("return @RESULTVALUE");
            src.AppendLine("END --program");


            return src.ToString(); ;

        }


    }
}
