using FAST.FBasicInterpreter.Libraries;

namespace FAST.FBasicInterpreter
{
    internal class BuiltInsForCollections : IFBasicLibrary
    {
        public void InstallAll(Interpreter interpreter)
        {
            interpreter.AddFunction("EOD", EOD);
            interpreter.AddFunction("SCI", StaticCollectionItem);
            interpreter.AddFunction("SCNT", StaticCollectionCount);
            interpreter.AddStatement("FETCH",FETCH);
            interpreter.AddStatement("RESET", RESET);
        }
 
        public static Value EOD(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 1) throw new ArgumentException();
            string collection = args[0].String;
            if (!interpreter.collections.ContainsKey(collection)) interpreter.Error("EOD", $"Collection:{collection} not found");
            return new Value(interpreter.collections[collection].endOfData ? 1 : 0);
        }

        public static void FETCH(Interpreter interpreter)
        {
            // Syntax: FECH collection
            //  Used to FETCH next item of a collection
            //
            interpreter.Match(Token.Identifier);
            string collectionName = interpreter.lex.Identifier;

            interpreter.GetNextToken();

            // (v) do the work
            if (!interpreter.collections.ContainsKey(collectionName) ) 
                interpreter.Error("FETCH",$"Collection: ${collectionName} not found.");
            var collection = interpreter.collections[collectionName];

            collection.endOfData=!collection.MoveNext();
        }

        public static void RESET(Interpreter interpreter)
        {
            // Syntax: RESET collection
            //  Used to RESET a collection
            //
            interpreter.Match(Token.Identifier);
            string collectionName = interpreter.lex.Identifier;

            interpreter.lastToken = Token.NewLine;



            // (v) implementation
            if (!interpreter.collections.ContainsKey(collectionName))
                interpreter.Error("RESET", $"Collection: ${collectionName} not found.");
            var collection = interpreter.collections[collectionName];

            collection.Reset();
            collection.endOfData=false;
        }

        /// <summary>
        /// SCI() : Static Collection Item
        /// </summary>
        /// <returns>Value, the selected value</returns>
        public static Value StaticCollectionItem(Interpreter interpreter, List<Value> args)
        {
            const string syntax= "SCI(SCollectionName,Item)";
            if (args.Count < 2)
                return interpreter.Error("SCI", Errors.E125_WrongNumberOfArguments(2, syntax)).value;

            if (args[0].Type != ValueType.String ||
                args[1].Type != ValueType.Real )
                return interpreter.Error("SCI", Errors.E126_WrongArgumentType(syntax:syntax)).value;

            string collectionName = args[0].String;
            int item = args[1].ToInt();
            
            var collection=interpreter.collections[collectionName];
            if (!(collection is staticDataCollection))
            {
                return interpreter.Error("SCI",Errors.E127_WrongArgumentReferredType("SDATA Collection")).value;
            }
            var value = ((staticDataCollection)interpreter.collections[collectionName]).data[item];
            return new Value(value);
        }

        /// <summary>
        /// SCNT() : Static Collection Count
        /// </summary>
        /// <returns>Number, the number of items in collection</returns>
        public static Value StaticCollectionCount(Interpreter interpreter, List<Value> args)
        {
            const string syntax = "SCNT(SCollectionName)";
            if (args.Count != 1)
                return interpreter.Error("SCNT", Errors.E125_WrongNumberOfArguments(1, syntax)).value;

            if (args[0].Type != ValueType.String )
                return interpreter.Error("SCNT", Errors.E126_WrongArgumentType(syntax: syntax)).value;

            string collectionName = args[0].String;

            var collection = interpreter.collections[collectionName];
            if (!(collection is staticDataCollection))
            {
                return interpreter.Error("SCNT", Errors.E127_WrongArgumentReferredType("SDATA Collection")).value;
            }
            var count = ((staticDataCollection)interpreter.collections[collectionName]).data.Count;
            return new Value(count);
        }

    }


}
