class InputOutput
{
    const byte _errMax = 9;
    public static char Ch
    { 
        get; 
        set;
    }
    public static TextPosition PositionNow;
    static string _line;
    static byte _lastInLine;
    public static List<Err> _err;
    static StreamReader _file;
    static Dictionary<int, string> _errorTable;
    static uint _errCount;

    static public bool Init(string filePath)
    {
        if (File.Exists(filePath))
        {
            PositionNow = new TextPosition();
            _line = "";
            _lastInLine = 0;
            _err = new List<Err>();
            _file = new StreamReader(filePath);
            _errCount = 0;
            _errorTable = new Dictionary<int, string>();

            StreamReader errorTableFile = new StreamReader("ErrorTable.txt");  
            string[] tableLine = new string[2];  
            while (!errorTableFile.EndOfStream)
            {
                tableLine = errorTableFile.ReadLine().Split(' ', 2);
                _errorTable.Add(int.Parse(tableLine[0]), tableLine[1]);
            }

            errorTableFile.Close();

            return true;
        }
        else
        {
            Console.WriteLine("Файл не найден");
            return false;
        }
    }
    static public bool NextCh()
    {
        if (PositionNow.CharNumber == _lastInLine)
        {
            ListThisLine();

            if (_err.Count > 0)
            {
                ListErrors();
            }

            if (ReadNextLine())
            {
                PositionNow.LineNumber++;
                PositionNow.CharNumber = 0;
                Ch = _line[PositionNow.CharNumber];
                return true;
            }
            else
            {
                Ch = '\0';
                return false;
            }
        }
        else 
        {
            ++PositionNow.CharNumber;
            Ch = _line[PositionNow.CharNumber];
            return true;
        }
    }

    private static void ListThisLine()
    {
        Console.WriteLine("      " + _line);
    }

    private static bool ReadNextLine()
    {
        while (!_file.EndOfStream)
        {
            _line = _file.ReadLine();
            if (!string.IsNullOrWhiteSpace(_line))
            {
                _err = new List<Err>();
                _lastInLine = (byte)(_line.Length - 1);
                return true;
            }
        }

        End();
        return false;
    }

    static void End()
    {
        Console.WriteLine($"Компиляция завершена. Ошибок — {_errCount}!");
    }

    static void ListErrors()
    {
        string s;
        foreach (Err item in _err)
        {
            ++_errCount;
            s = "**";
            if (_errCount < 10) 
            {
                s += "0";
            }
            s += $"{_errCount}**";

            while (s.Length < 6 + item.ErrorPosition.CharNumber) 
            {
                s += " ";
            }
            
            s += $"^ ошибка: {_errorTable[item.ErrorCode]}";
            Console.WriteLine(s);
        }
    }

    static public void Error(byte errorCode, TextPosition position)
    {
        Err e;
        if (_err.Count <= _errMax)
        {
            e = new Err(position, errorCode);
            _err.Add(e);
        }
    }
}