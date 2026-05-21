class Program
{
    static void Main()
    {
        string filePath = Program.ReadFilePath("Введите имя файла для компиляции: ");
        Random rnd = new Random();
        uint lNum = 0;

        if (InputOutput.Init(filePath))
        {
            while (InputOutput.NextCh())
            {
                if (InputOutput._ch == '_')
                {
                    InputOutput.Error(1, InputOutput._positionNow);
                }
                if (InputOutput._ch == ';')
                {
                    InputOutput.Error(2, InputOutput._positionNow);
                }
                if (InputOutput._ch == '=')
                {
                    InputOutput.Error(3, InputOutput._positionNow);
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