using System;
using System.Collections.Generic;
using System.Text;

namespace lab1_compiler.Bar
{
    public class ParsingError
    {
        public int NumberOfError { get; set; }
        public string Message { get; set; }
        public string ExpectedToken { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }

    public class RawTextParser
    {
        private List<ParsingError> Errors = new List<ParsingError>();
        private int _errorNumber = 1;

        public List<ParsingError> Parse(string text)
        {
            Correct(text);
            return Errors;
        }

        public (string correctedText, List<ParsingError> errors) Correct(string text)
        {
            Errors.Clear();
            _errorNumber = 1;

            int i = 0, line = 1, col = 1, length = text.Length;
            var sb = new StringBuilder();

            // Стек для начала многострочных комментариев
            var multiLineStack = new Stack<(int startLine, int startCol, char quoteChar)>();

            // Если встретили попытку закрыть <3 кавычек
            bool hasStrayClosingAttempt = false;
            int strayLine = 0, strayCol = 0;

            while (i < length)
            {
                char current = text[i];

                // перевод строки
                if (current == '\n')
                {
                    sb.Append('\n');
                    line++; col = 1; i++;
                    continue;
                }

                // однострочный комментарий
                if (current == '#' && multiLineStack.Count == 0)
                {
                    sb.Append('#');
                    i++; col++;
                    while (i < length && text[i] != '\n')
                    {
                        sb.Append(text[i]);
                        i++; col++;
                    }
                    continue;
                }

                // кавычки
                if (current == '\'' || current == '"')
                {
                    char qc = current;

                    // 1) мы внутри многострочного того же типа?
                    if (multiLineStack.Count > 0 && multiLineStack.Peek().quoteChar == qc)
                    {
                        // посчитаем, сколько подряд
                        int j = i, count = 0;
                        while (j < length && text[j] == qc)
                        {
                            count++; j++;
                        }

                        // если это полноценный закрыватель (>=3)
                        if (count >= 3)
                        {
                            // добавляем ровно три
                            sb.Append(new string(qc, 3));
                            multiLineStack.Pop();
                            i += count;
                            col += count;
                            continue;
                        }
                        else
                        {
                            // пометим первую «неполную» попытку и просто проглотаем кавычки
                            if (!hasStrayClosingAttempt)
                            {
                                hasStrayClosingAttempt = true;
                                strayLine = line;
                                strayCol = col;
                            }
                            // НЕ добавляем эти 1–2 кавычки в вывод
                            i += count;
                            col += count;
                            continue;
                        }
                    }

                    // 2) открываем новый многострочный комментарий
                    {
                        int tokenStartCol = col;
                        int j = i, count = 0;
                        while (j < length && text[j] == qc)
                        {
                            count++; j++;
                        }

                        if (count < 3)
                        {
                            AddError(
                                "Недостаточно кавычек для открытия многострочного комментария",
                                new string(qc, 3),
                                line,
                                tokenStartCol
                            );
                        }
                        else if (count > 3)
                        {
                            AddError(
                                "Лишние кавычки в открывающем токене многострочного комментария",
                                new string(qc, 3),
                                line,
                                tokenStartCol
                            );
                        }

                        // в любом случае вставляем ровно три, как начало
                        sb.Append(new string(qc, 3));
                        multiLineStack.Push((line, tokenStartCol, qc));

                        i += count;
                        col += count;
                        continue;
                    }
                }

                // всё остальное
                sb.Append(current);
                i++; col++;
            }

            // если остались незакрытые
            while (multiLineStack.Count > 0)
            {
                var (sLine, sCol, qc) = multiLineStack.Pop();
                int errLine = hasStrayClosingAttempt ? strayLine : sLine;
                int errCol = hasStrayClosingAttempt ? strayCol : sCol;

                AddError("Незакрытый многострочный комментарий",
                         new string(qc, 3),
                         errLine,
                         errCol);

                // добавляем ровно три закрывающие кавычки
                sb.Append(new string(qc, 3));
            }

            return (sb.ToString(), Errors);
        }


        private void AddError(string message, string expected, int line, int col)
        {
            Errors.Add(new ParsingError
            {
                NumberOfError = _errorNumber++,
                Message = message,
                ExpectedToken = expected,
                Line = line,
                Column = col
            });
        }
    }
}
