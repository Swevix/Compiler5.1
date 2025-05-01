using System;
using System.Collections.Generic;

namespace lab1_compiler.Bar
{
    public class LexicalToken
    {
        public int Code { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Position { get; set; }
    }

    internal class LexicalAnalyzer
    {
        // 1 = однострочный комментарий
        // 2 = многострочный комментарий """…"""
        // 3 = многострочный комментарий '''…'''
        // 4 = новая строка
        // 5 = текст комментария
        private readonly Dictionary<string, int> _tokenTypes = new Dictionary<string, int>
        {
            { "SingleLineComment", 1 },
            { "MultiLineDouble",   2 },
            { "MultiLineSingle",   3 },
            { "NewLine",           4 },
            { "CommentText",       5 }
        };

        public List<LexicalToken> Tokens { get; } = new List<LexicalToken>();
        public List<string> Errors { get; } = new List<string>();

        public void Analyze(string text)
        {
            Tokens.Clear();
            Errors.Clear();

            int i = 0, line = 1, col = 1, length = text.Length;

            while (i < length)
            {
                char current = text[i];

                // Новая строка
                if (current == '\n')
                {
                    Tokens.Add(new LexicalToken
                    {
                        Code = _tokenTypes["NewLine"],
                        Type = "Новая строка",
                        Value = "\\n",
                        Position = $"Строка {line}, Позиция {col}"
                    });
                    line++;
                    col = 1;
                    i++;
                    continue;
                }

                // Однострочный комментарий #
                if (current == '#')
                {
                    int startLine = line, startCol = col;
                    Tokens.Add(new LexicalToken
                    {
                        Code = _tokenTypes["SingleLineComment"],
                        Type = "Однострочный комментарий",
                        Value = "#",
                        Position = $"Строка {startLine}, Позиция {startCol}"
                    });

                    i++; col++;
                    int commentStart = i;
                    // Читаем до перевода строки или до конца текста
                    while (i < length && text[i] != '\n')
                    {
                        i++;
                        col++;
                    }
                    string commentText = text.Substring(commentStart, i - commentStart);
                    Tokens.Add(new LexicalToken
                    {
                        Code = _tokenTypes["CommentText"],
                        Type = "Текст комментария",
                        Value = commentText,
                        Position = $"Строка {startLine}, Позиция {startCol + 1}"
                    });
                    continue;
                }

                // Многострочный комментарий """…"""
                if (i + 2 < length
                    && text[i] == '"' && text[i + 1] == '"' && text[i + 2] == '"')
                {
                    int startLine = line, startCol = col;
                    Tokens.Add(new LexicalToken
                    {
                        Code = _tokenTypes["MultiLineDouble"],
                        Type = " multi-line (двойные кавычки)",
                        Value = "\"\"\"",
                        Position = $"Строка {startLine}, Позиция {startCol}"
                    });

                    i += 3; col += 3;
                    int bodyStart = i;
                    bool endFound = false;
                    // Читаем до следующей тройной кавычки
                    while (i + 2 < length)
                    {
                        if (text[i] == '\n')
                        {
                            line++;
                            col = 1;
                            i++;
                            continue;
                        }
                        if (text[i] == '"' && text[i + 1] == '"' && text[i + 2] == '"')
                        {
                            endFound = true;
                            break;
                        }
                        i++;
                        col++;
                    }
                    string body = text.Substring(bodyStart, i - bodyStart);
                    Tokens.Add(new LexicalToken
                    {
                        Code = _tokenTypes["CommentText"],
                        Type = "Текст комментария",
                        Value = body,
                        Position = $"Строка {startLine}, Позиция {startCol + 3}"
                    });

                    if (endFound)
                    {
                        // Пропускаем закрывающую тройную кавычку
                        i += 3; col += 3;
                    }
                    else
                    {
                        Errors.Add($"Не найден конец многострочного \"\"\" комментария, начатого на строке {startLine}, позицией {startCol}");
                    }
                    continue;
                }

                // Многострочный комментарий '''…'''
                if (i + 2 < length
                    && text[i] == '\'' && text[i + 1] == '\'' && text[i + 2] == '\'')
                {
                    int startLine = line, startCol = col;
                    Tokens.Add(new LexicalToken
                    {
                        Code = _tokenTypes["MultiLineSingle"],
                        Type = " multi-line (одинарные кавычки)",
                        Value = "'''",
                        Position = $"Строка {startLine}, Позиция {startCol}"
                    });

                    i += 3; col += 3;
                    int bodyStart = i;
                    bool endFound = false;
                    while (i + 2 < length)
                    {
                        if (text[i] == '\n')
                        {
                            line++;
                            col = 1;
                            i++;
                            continue;
                        }
                        if (text[i] == '\'' && text[i + 1] == '\'' && text[i + 2] == '\'')
                        {
                            endFound = true;
                            break;
                        }
                        i++;
                        col++;
                    }
                    string body = text.Substring(bodyStart, i - bodyStart);
                    Tokens.Add(new LexicalToken
                    {
                        Code = _tokenTypes["CommentText"],
                        Type = "Текст комментария",
                        Value = body,
                        Position = $"Строка {startLine}, Позиция {startCol + 3}"
                    });

                    if (endFound)
                    {
                        i += 3; col += 3;
                    }
                    else
                    {
                        Errors.Add($"Не найден конец многострочного ''' комментария, начатого на строке {startLine}, позицией {startCol}");
                    }
                    continue;
                }

                // Любой другой символ — просто пропускаем
                i++; col++;
            }
        }
    }
}
