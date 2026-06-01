using System;

class LexicalAnalyzer
{
    public const byte ident = 1;
    public const byte intc = 2;
    
    public const byte dosy = 3; public const byte ifsy = 4; public const byte insy = 5;
    public const byte ofsy = 6; public const byte orsy = 7; public const byte tosy = 8;
    
    public const byte endsy = 9; public const byte varsy = 10; public const byte divsy = 11;
    public const byte andsy = 12; public const byte notsy = 13; public const byte forsy = 14;
    public const byte modsy = 15; public const byte nilsy = 16; public const byte setsy = 17;
    
    public const byte thensy = 18; public const byte elsesy = 19; public const byte casesy = 20;
    public const byte filesy = 21; public const byte gotosy = 22; public const byte typesy = 23;
    public const byte withsy = 24;
    
    public const byte beginsy = 25; public const byte whilesy = 26; public const byte arraysy = 27;
    public const byte constsy = 28; public const byte labelsy = 29; public const byte untilsy = 30;
    
    public const byte downtosy = 31; public const byte packedsy = 32; public const byte recordsy = 33;
    public const byte repeatsy = 34;
    
    public const byte programsy = 35; public const byte functionsy = 36; public const byte procedurensy = 37;


    public const byte leftpar = 38;
    public const byte rightpar = 39;
    public const byte slash = 40;
    public const byte semicolon = 41;
    public const byte comma = 42;
    public const byte equal = 43;
    public const byte plus = 44;
    public const byte minus = 45;
    public const byte star = 46;
    public const byte lbracket = 47;
    public const byte rbracket = 48;
    public const byte arrow = 49;       
    public const byte assign = 50;       
    public const byte colon = 51;        
    public const byte point = 52;        
    public const byte twopoints = 53;    
    public const byte later = 54;        
    public const byte greater = 55;      
    public const byte laterequal = 56;   
    public const byte greaterequal = 57;  
    public const byte latergreater = 58;
    public const byte stringc = 83;
    
    public const byte eofsym = 99;


    private static byte _symbol;
    private static TextPosition _token;
    private static string _addrName;
    private static int _nmbInt;
    private static bool _isEof = false;

    private static Keywords _keywords = new Keywords();


    public static byte Symbol => _symbol;
    public static TextPosition Token => _token;
    public static string AddrName => _addrName;
    public static Int32 NmbInt => _nmbInt;

    private static TextPosition _parPos;
    private static TextPosition _stringcPos;
    private static bool _stringcOpen = false;
    private static int _parLevel = 0;
    private static bool _foundEnd = true;



    private static void Advance()
    {
        if (!_isEof)
        {
            if (!InputOutput.NextCh())
            {
                _isEof = true;
            }
        }
    }

    public static byte NextSym()
    {
        uint currentLine = 0;
        while (!_isEof)
        {
            if (InputOutput.Ch == ' ' || InputOutput.Ch == '\t')
            {
                Advance();
                continue;
            }

            if (InputOutput.Ch == '{')
            {
                _foundEnd = false;
                TextPosition startPos = InputOutput.PositionNow;
                Advance();
                while (!_isEof && !_foundEnd)
                {
                    if (InputOutput.Ch == '}')
                    {
                        _foundEnd = true;
                    }
                    else
                    {
                        Advance();
                    }
                }
                if (_isEof && !_foundEnd)
                {
                    InputOutput.Error(6, startPos);
                }
                Advance();
                continue;
            }

            if (InputOutput.Ch == '(')
            {
                TextPosition startPos = InputOutput.PositionNow;
                Advance();
                if (!_isEof && InputOutput.Ch == '*')
                {
                    Advance();
                    _foundEnd = false;
                    while (!_isEof && !_foundEnd)
                    {
                        if (InputOutput.Ch == '*')
                        {
                            Advance();
                            if (!_isEof && InputOutput.Ch == ')')
                            {
                                Advance();
                                _foundEnd = true;
                            }
                        }
                        else
                        {
                            Advance();
                        }
                    }
                    if (_isEof && !_foundEnd)
                    {
                        InputOutput.Error(6, startPos);
                    }
                    continue;
                }
                else
                {
                    _parPos = startPos;
                    _parLevel++;
                    _token = startPos;
                    _symbol = leftpar;
                    return _symbol;
                }
            }

            if (InputOutput.Ch == '/')
            {
                TextPosition startPos = InputOutput.PositionNow;
                Advance();
                if (!_isEof && InputOutput.Ch == '/')
                {
                    currentLine = InputOutput.PositionNow.LineNumber;
                    while (!_isEof && InputOutput.PositionNow.LineNumber == currentLine)
                    {
                        Advance();
                    }
                    continue;
                }
                else
                {
                    _token = startPos;
                    _symbol = slash;
                    return _symbol;
                }
            }

            break;
        }

        if (_isEof)
        {
            if (_parLevel > 0)
            {
                InputOutput.Error(7, _parPos);
            }
            if (_stringcOpen)
            {
                InputOutput.Error(8, _stringcPos);
            }

            InputOutput.ListErrors();

            _symbol = eofsym;

            InputOutput.End();
            return _symbol;
        }

        _token = InputOutput.PositionNow;
        char ch = InputOutput.Ch;

        if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'))
        {
            _addrName = "";
            while (!_isEof && ((InputOutput.Ch >= 'a' && InputOutput.Ch <= 'z') || 
                               (InputOutput.Ch >= 'A' && InputOutput.Ch <= 'Z') || 
                               (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')))
            {
                _addrName += InputOutput.Ch;
                Advance();
            }

            string lowerName = _addrName.ToLower();
            byte len = (byte)lowerName.Length;

            if (_keywords.Kw.ContainsKey(len) && _keywords.Kw[len].ContainsKey(lowerName))
            {
                _symbol = _keywords.Kw[len][lowerName];
            }
            else
            {
                _symbol = ident;
            }
            return _symbol;
        }

        if (ch >= '0' && ch <= '9')
        {
            Int32 maxInt = 32767;
            _nmbInt = 0;
            bool overflow = false;
            TextPosition numPos = InputOutput.PositionNow;
            byte digit = 0;

            while (!_isEof && InputOutput.Ch >= '0' && InputOutput.Ch <= '9')
            {
                digit = (byte)(InputOutput.Ch - '0');
                if (!overflow)
                {
                    _nmbInt = 10 * _nmbInt + digit;
                    if (_nmbInt > maxInt)
                    {
                        InputOutput.Error(11, numPos);
                        _nmbInt = 0;
                        overflow = true;
                    }
                }
                Advance();
            }
            _symbol = intc;
            return _symbol;
        }


        switch (ch)
        {
            case '<':
                Advance();
                if (!_isEof && InputOutput.Ch == '=') 
                { 
                    Advance(); _symbol = laterequal; 
                }
                else if (!_isEof && InputOutput.Ch == '>') 
                { 
                    Advance(); _symbol = latergreater; 
                }
                else 
                {
                    _symbol = later;
                }
                break;

            case '>':
                Advance();
                if (!_isEof && InputOutput.Ch == '=') 
                { 
                    Advance(); _symbol = greaterequal; 
                }
                else 
                {
                    _symbol = greater;
                }
                break;

            case ':':
                Advance();
                if (!_isEof && InputOutput.Ch == '=') 
                { 
                    Advance(); _symbol = assign; 
                }
                else 
                {
                    _symbol = colon;
                }
                break;

            case '.':
                Advance();
                if (!_isEof && InputOutput.Ch == '.') 
                { 
                    Advance(); _symbol = twopoints; 
                }
                else 
                {
                    _symbol = point;
                }
                break;

            case ';': 
                Advance(); 
                _symbol = semicolon; 
                break;
            case ',': 
                Advance(); 
                _symbol = comma; 
                break;
            case '=': 
                Advance(); 
                _symbol = equal; 
                break;
            case '+': 
                Advance(); 
                _symbol = plus; 
                break;
            case '-': 
                Advance(); 
                _symbol = minus; 
                break;
            case '*': 
                Advance(); 
                _symbol = star; 
                break;
            case ')': 
                Advance(); 
                _symbol = rightpar; 
                if (_parLevel > 0)
                {
                    _parLevel--;
                }  
                else
                {
                    InputOutput.Error(9, InputOutput.PositionNow);
                }
                break;
            case '[': 
                Advance(); 
                _symbol = lbracket; 
                break;
            case ']': 
                Advance(); 
                _symbol = rbracket; 
                break;
            case '^': 
                Advance(); 
                _symbol = arrow; 
                break;
            
            case '\'':
                TextPosition stringStartPos = InputOutput.PositionNow;
                uint startLine = stringStartPos.LineNumber;

                Advance();
                bool isClosed = false;

                do
                {
                    if (InputOutput.Ch == '\'')
                    {
                        Advance();

                        if (InputOutput.PositionNow.LineNumber == startLine && InputOutput.Ch == '\'')
                        {
                            Advance(); 
                        }
                        else
                        {
                            isClosed = true;
                            break;
                        }
                    }
                    else
                    {
                        Advance();
                    }
                } while (InputOutput.PositionNow.LineNumber == startLine);

                if (!isClosed)
                {
                    InputOutput.Error(8, stringStartPos);
                }

                _symbol = stringc; 
                break;

            default:
                InputOutput.Error(5, InputOutput.PositionNow);
                Advance();
                return NextSym();
        }

        return _symbol;
    }
}