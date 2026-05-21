class InputOutput
{
    const byte _errMax = 9;
    public static char _ch
    { 
        get; 
        set;
    }
    public static TextPosition _positionNow;
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
            _positionNow = new TextPosition();
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
        bool ans = false;
        if (_positionNow._charNumber == _lastInLine)
        {
            ListThisLine();

            if (_err.Count > 0)
            {
                ListErrors();
            }

            if (ReadNextLine())
            {
                _positionNow._lineNumber++;
                _positionNow._charNumber = 0;
                ans = true;
            }
            else
            {
                ans = false;
            }
        }
        else 
        {
            ++_positionNow._charNumber;
            ans = true;
        }

        _ch = _line[_positionNow._charNumber];
        return ans;
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
        int pos = 7 - $"{_positionNow._lineNumber} ".Length;
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
            while (s.Length - 1 < pos + item._errorPosition._charNumber) 
            {
                s += " ";
            }
            s += $"^ ошибка: {_errorTable[item._errorCode]}";
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