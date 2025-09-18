using System.Text;

namespace FAST.FBasicInterpreter
{
    public class fBasicSourceBuilder : sourceCodeBuilderAbstract
    {
        private string source=null;

        protected override programContainer program { get; set; }

        public override void Build(programContainer program)
        {
            this.program = program; 
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
            string quote = "\"";
            StringBuilder src = new();
            bool done = false;
            foreach (var item in program.elements)
            {
                //src.Append(string.Format("[{0} ({1}, {2})]", item.token, item.identifier, item.value));
                switch (item.token)
                {
                    case Token.Colon:
                        src.Append(":");
                        break;
                    case Token.Semicolon:
                        src.Append(";");
                        break;
                    case Token.Comma:
                        src.Append(", ");
                        break;

                    case Token.Equal:
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

                    case Token.Else:
                        src.Append(item.token);
                        break;

                    case Token.If:
                    case Token.For:
                        src.Append(item.token);
                        src.Append(spacer);
                        break;

                    case Token.Then:
                    case Token.To:
                        src.Append(spacer);
                        src.Append(item.token);
                        src.Append(spacer);
                        break;

                    case Token.Next:
                        src.Append(item.token);
                        src.Append(spacer);
                        break;

                    case Token.End:  // END, HALT
                    case Token.EndIf:
                        src.Append(item.token);
                        break;

                    case Token.Goto:
                    case Token.Gosub:
                    case Token.Return:
                        src.Append(item.token);
                        src.Append(spacer);
                        break;

                    case Token.Rem:
                        //Console.WriteLine($"{item.identifier} {item.value}");
                        break;

                    case Token.Call:
                    case Token.Result:
                    case Token.Let:
                    case Token.Print:
                    case Token.Input:
                    case Token.Assert:
                    case Token.Dump:
                        src.Append(item.token); // or item.identifier
                        src.Append(spacer);
                        break;



                    case Token.EOF:
                        done = true;
                        break;

                    case Token.NewLine:
                        src.AppendLine();
                        break;

                    case Token.Identifier:
                        if (item.isDoted)
                        {
                            src.Append('[');
                            src.Append(item.identifier);
                            src.Append(']');
                        }
                        else
                        {
                            src.Append(item.identifier);
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
                        throw new Exception(string.Format("Uknown Token: [{0} ({1}, {2})]", item.token, item.identifier, item.value));
                }
                if (done) break;
            }


            return src.ToString(); ;

        }


    }
}
