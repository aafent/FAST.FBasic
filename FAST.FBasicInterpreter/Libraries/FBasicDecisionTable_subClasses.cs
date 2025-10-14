namespace FAST.FBasicInterpreter
{
    public partial class FBasicDecisionTables 

    {
        private enum boType { decisionTable }

        private enum operType {  Equal, GraterThan, LessThan, GreaterOrEqual, LessOrEqual, Between}

        private class cellValue
        { 
            public Value value;
            public operType  oper = operType.Equal;
            public Value value2; 

            public cellValue(Value value)
            {
                this.value=value;
                this.oper = operType.Equal; // default
            }

            public override string ToString()
            {
                if (oper == operType.Equal) return value.ToString();
                return oper.ToString() + " " + value.ToString();
            }

        }


        private class  decTable
        {
            public string[] factors;
            public int count; 
            public Value otherwise;
            public Dictionary<string,Value> search;
            public List<cellValue[]> data;
            


            public bool Match(string requestedFactorName, int dataIndex)
            {
                if (dataIndex < 0 || dataIndex >= data.Count) return false;
                return Match(requestedFactorName, data[dataIndex]);
            }

            public bool Match(string requestedFactorName, cellValue[] dataToCheck)
            {
                int requestedFactorIndex = Array.IndexOf(factors, requestedFactorName);
                if (requestedFactorIndex == -1) return false; // requested factor not found
                return Match(requestedFactorIndex, dataToCheck);
            }

            public bool Match(int requestedFactorIndex, cellValue[] dataToCheck)
            {
                if (requestedFactorIndex < 0) return false; // requested factor not found

                // Check all factors except the requested one
                bool match = false;
                for (int i = 0; i < factors.Length; i++)
                {
                    if (i == requestedFactorIndex) continue; // skip the requested factor

                    // Check if current factor's value in this row matches search dictionary or criteria
                    if (!search.TryGetValue(factors[i], out var searchValue))
                    {
                        match = false;  // no search value for this factor, can't compare
                        break;
                    }

                    bool exitForLoop = false;
                    switch (dataToCheck[i].oper)
                    {
                        case operType.Equal:
                            if (searchValue.Equals(dataToCheck[i].value))
                            {
                                match = true;
                            }
                            else
                            {
                                match = false;
                                exitForLoop = true; // once False always False....
                            }
                            break;

                        case operType.LessOrEqual:
                        case operType.GreaterOrEqual:
                        case operType.GraterThan:
                        case operType.LessThan:
                            var comp = searchValue.CompareTo(dataToCheck[i].value);
                            if (comp == 0) // (<) Equal values
                            {
                                if (dataToCheck[i].oper == operType.GreaterOrEqual || dataToCheck[i].oper == operType.LessOrEqual)
                                {
                                    match = true;
                                }
                                else
                                {
                                    match = false;
                                    exitForLoop = true; // once False always False....
                                }
                            }
                            else if (comp < 0) // (<) searchValue < row[i].value
                            {
                                if (dataToCheck[i].oper == operType.LessThan || dataToCheck[i].oper == operType.LessOrEqual)
                                {
                                    match = true;
                                }
                                else
                                {
                                    match = false;
                                    exitForLoop = true; // once False always False....
                                }
                            }
                            else // (comp > 0) (> ) Greater Than, searchValue > row[i].value
                            {
                                if (dataToCheck[i].oper == operType.GraterThan || dataToCheck[i].oper == operType.GreaterOrEqual)
                                {
                                    match = true;
                                }
                                else
                                {
                                    match = false;
                                    exitForLoop = true; // once False always False....
                                }
                            }

                            break;


                        case operType.Between:
                            var comp1 = searchValue.CompareTo(dataToCheck[i].value);
                            var comp2 = searchValue.CompareTo(dataToCheck[i].value2);
                            if (comp1 >= 0 && comp2 <= 0)
                            {
                                match = true;
                            }
                            else
                            {
                                match = false;
                                exitForLoop = true; // once False always False....
                            }
                            break;

                        default:
                            throw new Exception($"Unsupported operator {dataToCheck[i].oper} in decision table");
                    }
                    if (exitForLoop) break;
                } // next factor

                if (match)
                {
                    //var foundValue = dataToCheck[requestedIndex];
                    return true;
                }
                
                return false;
            }

            // Search method:
            // - requestedFactorName is the factor to be found as the "result"
            // - out foundValue returns the Value at requested factor in the matched row
            // - returns true if a matching row is found, false otherwise
            public bool TryFindMatchingRow(string requestedFactorName, out cellValue foundValue)
            {
                foundValue = default;

                int requestedFactorIndex = Array.IndexOf(factors, requestedFactorName);
                if (requestedFactorIndex == -1) return false; // requested factor not found

                // Iterate over rows in data
                foreach (var row in data)
                {
                    bool match = Match(requestedFactorName, row);
                    if (match)
                    {
                        foundValue = row[requestedFactorIndex];
                        return true;
                    }
                }
                return false;
            }

            /*
            public bool XTryFindMatchingRow(string requestedFactorName, out cellValue foundValue)
            {
                foundValue = default;

                int requestedIndex = Array.IndexOf(factors, requestedFactorName);
                if (requestedIndex == -1) return false; // requested factor not found

                // Iterate over rows in data
                foreach (var row in data)
                {
                    // Check all factors except the requested one
                    bool match = false;
                    for (int i = 0; i < factors.Length; i++)
                    {
                        if (i == requestedIndex) continue; // skip the requested factor

                        // Check if current factor's value in this row matches search dictionary or criteria
                        if (!search.TryGetValue(factors[i], out var searchValue))
                        {
                            match = false;  // no search value for this factor, can't compare
                            break;
                        }

                        bool exitForLoop = false;
                        switch (row[i].oper )
                        {
                            case operType.Equal: 
                                if (searchValue.Equals(row[i].value))
                                {
                                    match = true;
                                }
                                else
                                {
                                    match=false;
                                    exitForLoop=true; // once False always False....
                                }
                                break;

                            case operType.LessOrEqual:
                            case operType.GreaterOrEqual:
                            case operType.GraterThan:
                            case operType.LessThan:
                                var comp = searchValue.CompareTo(row[i].value);
                                if (comp == 0 ) // (<) Equal values
                                {
                                    if (row[i].oper == operType.GreaterOrEqual || row[i].oper == operType.LessOrEqual)
                                    {
                                        match = true;
                                    }
                                    else
                                    {
                                        match = false;
                                        exitForLoop = true; // once False always False....
                                    }
                                } 
                                else if (comp < 0) // (<) searchValue < row[i].value
                                {
                                    if (row[i].oper == operType.LessThan || row[i].oper == operType.LessOrEqual)
                                    {
                                        match = true;
                                    }
                                    else
                                    {
                                        match = false;
                                        exitForLoop = true; // once False always False....
                                    }
                                }
                                else // (comp > 0) (> ) Greater Than, searchValue > row[i].value
                                {
                                    if (row[i].oper == operType.GraterThan || row[i].oper == operType.GreaterOrEqual)
                                    {
                                        match = true;
                                    }
                                    else
                                    {
                                        match = false;
                                        exitForLoop = true; // once False always False....
                                    }
                                }

                                break;  
                           

                            case operType.Between:
                                var comp1 = searchValue.CompareTo(row[i].value);
                                var comp2 = searchValue.CompareTo(row[i].value2);
                                if (comp1 >= 0 && comp2<= 0)
                                {
                                    match = true;
                                }
                                else
                                {
                                    match = false;
                                    exitForLoop = true; // once False always False....
                                }
                                break;

                            default:
                                throw new Exception($"Unsupported operator {row[i].oper} in decision table");
                        }
                        if (exitForLoop) break;
                    } // next factor

                    if (match)
                    {
                        foundValue = row[requestedIndex];
                        return true;
                    }
                }
                return false;
            }
            */
            public void PrepareSearch()
            {
                search = new();
            }
        }

    }
}
