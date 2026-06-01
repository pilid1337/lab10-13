using System;
using System.Collections.Generic;

class SyntaxAnalyzer
{
    private static byte _sym;

    // Синхронизирующие множества для восстановления после ошибок
    private static readonly HashSet<byte> StatementSync = new HashSet<byte> 
    { 
        LexicalAnalyzer.semicolon, LexicalAnalyzer.endsy, LexicalAnalyzer.eofsym,
        LexicalAnalyzer.ifsy, LexicalAnalyzer.whilesy, LexicalAnalyzer.beginsy
    };

    private static readonly HashSet<byte> DeclarationSync = new HashSet<byte>
    {
        LexicalAnalyzer.semicolon, LexicalAnalyzer.beginsy, LexicalAnalyzer.eofsym
    };

    public static void Parse()
    {
        NextToken();
        
        // Простейшая структура программы: Сначала переменные, потом блок кода
        if (_sym == LexicalAnalyzer.varsy)
        {
            ParseVarDeclarations();
        }

        if (_sym == LexicalAnalyzer.beginsy)
        {
            ParseCompoundStatement();
        }
        else
        {
            Error(63); // Ожидался begin
        }
    }

    private static void NextToken()
    {
        _sym = LexicalAnalyzer.NextSym();
    }

    private static void Error(byte errorCode)
    {
        InputOutput.Error(errorCode, LexicalAnalyzer.Token);
    }

    private static bool Accept(byte expectedSym, byte errorCode)
    {
        if (_sym == expectedSym)
        {
            NextToken();
            return true;
        }
        Error(errorCode);
        return false;
    }

    // Нейтрализация ошибок (Panic Mode)
    private static void SkipTo(HashSet<byte> syncTokens)
    {
        while (!syncTokens.Contains(_sym) && _sym != LexicalAnalyzer.eofsym)
        {
            NextToken();
        }
    }

    // --- ОПИСАНИЕ ПЕРЕМЕННЫХ И МАССИВОВ ---
    private static void ParseVarDeclarations()
    {
        Accept(LexicalAnalyzer.varsy, 50); // Пропускаем var

        while (_sym == LexicalAnalyzer.ident)
        {
            try
            {
                // Список переменных (a, b, c)
                Accept(LexicalAnalyzer.ident, 50);
                while (_sym == LexicalAnalyzer.comma)
                {
                    NextToken();
                    Accept(LexicalAnalyzer.ident, 50);
                }

                Accept(LexicalAnalyzer.colon, 57); // Ожидаем ':'

                // Разбор типа (простой или массив)
                if (_sym == LexicalAnalyzer.arraysy)
                {
                    NextToken();
                    Accept(LexicalAnalyzer.lbracket, 9); // Ожидаем '['
                    Accept(LexicalAnalyzer.intc, 62);    // Левая граница
                    Accept(LexicalAnalyzer.twopoints, 60); // Ожидаем '..'
                    Accept(LexicalAnalyzer.intc, 62);    // Правая граница
                    Accept(LexicalAnalyzer.rbracket, 59); // Ожидаем ']'
                    Accept(LexicalAnalyzer.ofsy, 55);    // Ожидаем 'of'
                    Accept(LexicalAnalyzer.ident, 61);   // Тип элементов массива
                }
                else if (_sym == LexicalAnalyzer.ident)
                {
                    NextToken(); // Простой тип (integer, char и т.д.)
                }
                else
                {
                    Error(61); // Ожидался тип
                }

                Accept(LexicalAnalyzer.semicolon, 52);
            }
            catch
            {
                // Если внутри объявления произошла каша, прыгаем к следующей строке или begin
                SkipTo(DeclarationSync);
                if (_sym == LexicalAnalyzer.semicolon) NextToken();
            }
        }
    }

    // --- ОПЕРАТОРЫ ---
    private static void ParseStatement()
    {
        switch (_sym)
        {
            case LexicalAnalyzer.ident:
                ParseAssignment();
                break;
            case LexicalAnalyzer.ifsy:
                ParseIf();
                break;
            case LexicalAnalyzer.casesy:
                ParseCase();
                break;
            case LexicalAnalyzer.beginsy:
                ParseCompoundStatement();
                break;
            default:
                // Пустой оператор или непредвиденный токен
                break;
        }
    }

    private static void ParseCompoundStatement()
    {
        Accept(LexicalAnalyzer.beginsy, 63);

        ParseStatement();
        while (_sym == LexicalAnalyzer.semicolon)
        {
            NextToken();
            if (_sym == LexicalAnalyzer.endsy) break; // Защита от лишней точки с запятой перед end
            ParseStatement();
        }

        if (!Accept(LexicalAnalyzer.endsy, 56))
        {
            SkipTo(StatementSync); // Нейтрализация: ищем следующий блок
        }
    }

    private static void ParseAssignment()
    {
        Accept(LexicalAnalyzer.ident, 50);

        // Поддержка элементов массива (a[i] := ...)
        if (_sym == LexicalAnalyzer.lbracket)
        {
            NextToken();
            ParseExpression();
            Accept(LexicalAnalyzer.rbracket, 59);
        }

        if (Accept(LexicalAnalyzer.assign, 51))
        {
            ParseExpression();
        }
        else
        {
            SkipTo(StatementSync);
        }
    }

    private static void ParseIf()
    {
        Accept(LexicalAnalyzer.ifsy, 0); // already checked
        ParseExpression();
        
        if (Accept(LexicalAnalyzer.thensy, 53))
        {
            ParseStatement();
            if (_sym == LexicalAnalyzer.elsesy)
            {
                NextToken();
                ParseStatement();
            }
        }
        else
        {
            SkipTo(StatementSync);
        }
    }

    private static void ParseCase()
    {
        Accept(LexicalAnalyzer.casesy, 0);
        ParseExpression();
        Accept(LexicalAnalyzer.ofsy, 55);

        while (_sym == LexicalAnalyzer.intc || _sym == LexicalAnalyzer.ident)
        {
            NextToken(); // Константа выбора
            Accept(LexicalAnalyzer.colon, 57);
            ParseStatement();
            
            if (_sym == LexicalAnalyzer.semicolon)
            {
                NextToken();
            }
            else
            {
                break;
            }
        }

        Accept(LexicalAnalyzer.endsy, 56);
    }

    // --- АНАЛИЗ ВЫРАЖЕНИЙ ---
    private static void ParseExpression()
    {
        ParseSimpleExpression();

        // Операции отношения (=, <>, <, >, <=, >=)
        if (_sym == LexicalAnalyzer.equal || _sym == LexicalAnalyzer.latergreater ||
            _sym == LexicalAnalyzer.later || _sym == LexicalAnalyzer.greater ||
            _sym == LexicalAnalyzer.laterequal || _sym == LexicalAnalyzer.greaterequal)
        {
            NextToken();
            ParseSimpleExpression();
        }
    }

    private static void ParseSimpleExpression()
    {
        if (_sym == LexicalAnalyzer.plus || _sym == LexicalAnalyzer.minus)
        {
            NextToken();
        }
        
        ParseTerm();

        while (_sym == LexicalAnalyzer.plus || _sym == LexicalAnalyzer.minus || _sym == LexicalAnalyzer.orsy)
        {
            NextToken();
            ParseTerm();
        }
    }

    private static void ParseTerm()
    {
        ParseFactor();

        while (_sym == LexicalAnalyzer.star || _sym == LexicalAnalyzer.slash || 
               _sym == LexicalAnalyzer.divsy || _sym == LexicalAnalyzer.modsy || 
               _sym == LexicalAnalyzer.andsy)
        {
            NextToken();
            ParseFactor();
        }
    }

    private static void ParseFactor()
    {
        if (_sym == LexicalAnalyzer.ident)
        {
            NextToken();
            // Обработка массива в выражении (например, x + a[i])
            if (_sym == LexicalAnalyzer.lbracket)
            {
                NextToken();
                ParseExpression();
                Accept(LexicalAnalyzer.rbracket, 59);
            }
        }
        else if (_sym == LexicalAnalyzer.intc || _sym == LexicalAnalyzer.stringc)
        {
            NextToken();
        }
        else if (_sym == LexicalAnalyzer.leftpar)
        {
            NextToken();
            ParseExpression();
            Accept(LexicalAnalyzer.rightpar, 7); // Используем существующую ошибку №7
        }
        else if (_sym == LexicalAnalyzer.notsy)
        {
            NextToken();
            ParseFactor();
        }
        else
        {
            Error(62); // Ошибка в выражении
            SkipTo(new HashSet<byte> { LexicalAnalyzer.semicolon, LexicalAnalyzer.thensy, LexicalAnalyzer.dosy, LexicalAnalyzer.rightpar, LexicalAnalyzer.rbracket });
        }
    }
}