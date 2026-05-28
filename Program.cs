using System;
using System.IO;

class Program
{
    static void Main()
    {
        string filePath = Program.ReadFilePath("Введите имя файла для компиляции: ");
        string outPath = Program.ReadFilePath("Введите имя файла для сохранения: ");

        if (InputOutput.Init(filePath))
        {
            using (StreamWriter writer = new StreamWriter(outPath))
            {
                InputOutput.NextCh(); 
                
                byte sym;
                while ((sym = LexicalAnalyzer.NextSym()) != LexicalAnalyzer.eofsym)
                {
                    writer.WriteLine($"{sym} {LexicalAnalyzer.Token.LineNumber} {LexicalAnalyzer.Token.CharNumber}");
                }
            }
        }
        else
        {
            Console.WriteLine("Ошибка инициализации модуля ввода-вывода");
        }
    }

    private static string ReadFilePath(string prompt)
    {
        Console.Write(prompt);
        string input = "";
        while (true)
        {
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                input = input.Trim();
                if (File.Exists(input))
                {
                    return input;
                }
                Console.Write("Ошибка. Файл не найден. Введите значение: ");
            }
            else
            {
                Console.Write("Ошибка. Строка не может быть пустой. Введите значение: ");
            }
        }
    }
}