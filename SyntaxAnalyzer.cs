using System.Collections.Generic;

class SyntaxAnalyzer
{
    private static byte _sym;
    private static IdentifierTable _table;

    private static void NextSym()
    {
        _sym = LexicalAnalyzer.NextSym();
    }

    private static void Accept(byte expected, byte errorCode)
    {
        if (_sym == expected)
        {
            NextSym();
        }
        else
        {
            InputOutput.Error(errorCode, LexicalAnalyzer.Token);
        }
    }

    private static void SkipTo(params byte[] stopSymbols)
    {
        bool found = false;
        while (_sym != LexicalAnalyzer.eofsym && !found)
        {
            foreach (byte s in stopSymbols)
            {
                if (_sym == s)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                NextSym();
            }
        }
    }

    public static void Analyze()
    {
        _table = new IdentifierTable();
        NextSym();
        Program();
    }
    private static void Program()
    {
        Accept(LexicalAnalyzer.programsy, 12);
        Accept(LexicalAnalyzer.ident, 13);
        Accept(LexicalAnalyzer.semicolon, 14);

        if (_sym == LexicalAnalyzer.constsy)
        {
            ConstDeclarations();
        }

        if (_sym == LexicalAnalyzer.varsy)
        {
            VarDeclarations();
        }

        CompoundStatement();
        Accept(LexicalAnalyzer.point, 15);
    }

    private static void VarDeclarations()
    {
        Accept(LexicalAnalyzer.varsy, 16);

        List<string> names = new List<string>();
        string type = "";

        while (_sym == LexicalAnalyzer.ident)
        {
            names = IdentifierList();
            Accept(LexicalAnalyzer.colon, 17);
            
            type = ParseType();

            foreach (string name in names)
            {
                if (!_table.AddVariable(name, type ?? "unknown"))
                {
                    InputOutput.Error(23, LexicalAnalyzer.Token);
                }
            }

            if (_sym != LexicalAnalyzer.semicolon)
            {
                InputOutput.Error(14, LexicalAnalyzer.Token);
                SkipTo(LexicalAnalyzer.semicolon, LexicalAnalyzer.beginsy);
            }
            else
            {
                NextSym();
            }
        }
    }

    private static List<string> IdentifierList()
    {
        List<string> names = new List<string>();
        
        if (_sym == LexicalAnalyzer.ident)
        {
            names.Add(LexicalAnalyzer.AddrName);
            NextSym();
        }
        else Accept(LexicalAnalyzer.ident, 13);

        while (_sym == LexicalAnalyzer.comma)
        {
            NextSym();
            if (_sym == LexicalAnalyzer.ident)
            {
                names.Add(LexicalAnalyzer.AddrName);
                NextSym();
            }
            else Accept(LexicalAnalyzer.ident, 13);
        }
        return names;
    }

    private static string ParseType()
    {
        if (_sym == LexicalAnalyzer.arraysy)
        {
            NextSym();
            Accept(LexicalAnalyzer.lbracket, 18);
            Accept(LexicalAnalyzer.intc, 19);
            Accept(LexicalAnalyzer.twopoints, 20);
            Accept(LexicalAnalyzer.intc, 19);
            Accept(LexicalAnalyzer.rbracket, 21);
            Accept(LexicalAnalyzer.ofsy, 22);
            string baseType = SimpleType();
            return baseType != null ? "array_" + baseType : null;
        }
        else
        {
            return SimpleType();
        }
    }

    private static string SimpleType()
    {
        if (_sym == LexicalAnalyzer.ident)
        {
            string name = LexicalAnalyzer.AddrName.ToLower();
            if (name == "integer" || name == "real" || name == "boolean")
            {
                NextSym();
                return name;
            }
        }
        InputOutput.Error(24, LexicalAnalyzer.Token);
        NextSym();
        return null;
    }

    private static void CompoundStatement()
    {
        Accept(LexicalAnalyzer.beginsy, 25);
        StatementList();
        Accept(LexicalAnalyzer.endsy, 26);
    }

    private static void StatementList()
    {
        Statement();
        while (_sym == LexicalAnalyzer.semicolon || 
              (_sym == LexicalAnalyzer.ident && _sym != LexicalAnalyzer.endsy) ||
               _sym == LexicalAnalyzer.ifsy || _sym == LexicalAnalyzer.casesy || _sym == LexicalAnalyzer.beginsy)
        {
            if (_sym == LexicalAnalyzer.semicolon)
            {
                NextSym();
            }
            else
            {
                InputOutput.Error(14, LexicalAnalyzer.Token);
            }
            
            if (_sym == LexicalAnalyzer.endsy || _sym == LexicalAnalyzer.untilsy) 
            {
                break;
            }
            Statement();
        }
    }

    private static void Statement()
    {
        if (_sym == LexicalAnalyzer.ident)
        {
            TextPosition startPos = LexicalAnalyzer.Token;
            string varType = ParseVariableAccess(out string varName);
            
            Accept(LexicalAnalyzer.assign, 27);
            string exprType = Expression();

            if (varType == null)
            {
                InputOutput.Error(28, startPos);
            }
            else if (exprType != null && !TypesCompatible(varType, exprType))
            {
                InputOutput.Error(29, startPos);
            }
        }
        else if (_sym == LexicalAnalyzer.ifsy)
        {
            NextSym();
            string condType = Expression();
            if (condType != "boolean" && condType != "unknown")
            {
                InputOutput.Error(30, LexicalAnalyzer.Token);
            }
            
            Accept(LexicalAnalyzer.thensy, 31);
            Statement();
            
            if (_sym == LexicalAnalyzer.elsesy)
            {
                NextSym();
                Statement();
            }
        }
        else if (_sym == LexicalAnalyzer.casesy)
        {
            NextSym();
            string exprType = Expression(); 
            Accept(LexicalAnalyzer.ofsy, 22);
            
            while (_sym == LexicalAnalyzer.intc || _sym == LexicalAnalyzer.ident)
            {
                NextSym(); 
                Accept(LexicalAnalyzer.colon, 17);
                Statement();
                
                if (_sym == LexicalAnalyzer.semicolon) 
                {
                    NextSym();
                }
                else 
                {
                    break;
                }
            }
            Accept(LexicalAnalyzer.endsy, 26);
        }
        else if (_sym == LexicalAnalyzer.beginsy)
        {
            CompoundStatement();
        }
        else if (_sym != LexicalAnalyzer.endsy && _sym != LexicalAnalyzer.semicolon && _sym != LexicalAnalyzer.eofsym)
        {
            InputOutput.Error(32, LexicalAnalyzer.Token);
            SkipTo(LexicalAnalyzer.semicolon, LexicalAnalyzer.endsy);
        }
    }

    private static string ParseVariableAccess(out string name)
    {
        name = LexicalAnalyzer.AddrName;
        string type = _table.GetVariableType(name);
        Accept(LexicalAnalyzer.ident, 13);

        if (_sym == LexicalAnalyzer.lbracket)
        {
            NextSym();
            string indexType = Expression();
            if (indexType != "integer" && indexType != "unknown")
            {
                InputOutput.Error(33, LexicalAnalyzer.Token);
            }
            Accept(LexicalAnalyzer.rbracket, 21);

            if (type != null && type.StartsWith("array_"))
            {
                return type.Substring(6);
            }
            else if (type != null)
            {
                InputOutput.Error(34, LexicalAnalyzer.Token);
                return "unknown";
            }
        }
        
        return type;
    }

    private static string Expression()
    {
        string leftType = SimpleExpression();

        if (_sym == LexicalAnalyzer.equal || _sym == LexicalAnalyzer.later ||
            _sym == LexicalAnalyzer.greater || _sym == LexicalAnalyzer.laterequal ||
            _sym == LexicalAnalyzer.greaterequal || _sym == LexicalAnalyzer.latergreater)
        {
            NextSym();
            string rightType = SimpleExpression();
            return "boolean";
        }
        return leftType;
    }

    private static string SimpleExpression()
    {
        if (_sym == LexicalAnalyzer.plus || _sym == LexicalAnalyzer.minus)
        {
            NextSym();
        }

        string type = Term();
        string rightType = "";
        byte op = 0;

        while (_sym == LexicalAnalyzer.plus || _sym == LexicalAnalyzer.minus || _sym == LexicalAnalyzer.orsy)
        {
            op = _sym;
            NextSym();
            rightType = Term();
            
            if (op == LexicalAnalyzer.orsy) 
            {
                type = "boolean";
            }
            else 
            {
                type = MergeTypes(type, rightType);
            }
        }
        return type;
    }

    private static string Term()
    {
        string type = Factor();

        string rightType = "";
        byte op = 0;

        while (_sym == LexicalAnalyzer.star || _sym == LexicalAnalyzer.slash || 
               _sym == LexicalAnalyzer.divsy || _sym == LexicalAnalyzer.modsy || 
               _sym == LexicalAnalyzer.andsy)
        {
            op = _sym;
            NextSym();
            rightType = Factor();
            
            if (op == LexicalAnalyzer.andsy) 
            {
                type = "boolean";
            }
            else 
            {
                type = MergeTypes(type, rightType);
            }
        }
        return type;
    }

    private static string Factor()
    {
        if (_sym == LexicalAnalyzer.ident)
        {
            return ParseVariableAccess(out string _);
        }
        else if (_sym == LexicalAnalyzer.intc)
        {
            NextSym();
            return "integer";
        }
        else if (_sym == LexicalAnalyzer.leftpar)
        {
            NextSym();
            string type = Expression();
            Accept(LexicalAnalyzer.rightpar, 35);
            return type;
        }
        else if (_sym == LexicalAnalyzer.notsy)
        {
            NextSym();
            Factor();
            return "boolean";
        }
        else
        {
            InputOutput.Error(36, LexicalAnalyzer.Token);
            SkipTo(LexicalAnalyzer.semicolon, LexicalAnalyzer.endsy, LexicalAnalyzer.thensy, LexicalAnalyzer.ofsy);
            return "unknown";
        }
    }

    private static bool TypesCompatible(string left, string right)
    {
        if (left == right) 
        {
            return true;    
        }
        if (left == "unknown" || right == "unknown") 
        {
            return true;
        }
        if (left == "real" && right == "integer") 
        {
            return true;
        }
        return false;
    }

    private static string MergeTypes(string left, string right)
    {
        if (left == "unknown" || right == "unknown") 
        {
            return "unknown";
        }
        if (left == right) 
        {
            return left;
        }
        if ((left == "real" && right == "integer") || (left == "integer" && right == "real")) 
        {
            return "real";
        }
        return left;
    }

    private static void ConstDeclarations()
    {
        Accept(LexicalAnalyzer.constsy, 16); 

        string constName = "";
        
        while (_sym == LexicalAnalyzer.ident)
        {
            constName = LexicalAnalyzer.AddrName; 
            NextSym();
            
            Accept(LexicalAnalyzer.equal, 8);
            
            if (_sym == LexicalAnalyzer.intc || _sym == LexicalAnalyzer.stringc) 
            {
                NextSym();
            }
            else
            {
                InputOutput.Error(37, LexicalAnalyzer.Token);
                
                SkipTo(LexicalAnalyzer.semicolon); 
            }
            
            Accept(LexicalAnalyzer.semicolon, 14);
        }
    }
}