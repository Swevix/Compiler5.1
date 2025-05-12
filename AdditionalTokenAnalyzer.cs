using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace lab1_compiler
{
    public class AdditionalTokenAnalyzer
    {
        // 1. Bitcoin-адрес (P2PKH/P2SH или Bech32)
        private static readonly Regex ReBtcAddress = new Regex(
            @"^(?:[13][a-km-zA-HJ-NP-Z1-9]{25,34}|bc1[ac-hj-np-z02-9]{39,59})$",
            RegexOptions.Compiled);

        // 2. ИНН (10 или 12 цифр)
        private static readonly Regex ReInn = new Regex(
            @"^(?:\d{10}|\d{12})$",
            RegexOptions.Compiled);

        // 3. Дата DD/MM/YYYY с учётом високосных годов
        private static readonly Regex ReDate = new Regex(
            @"^(?:(?:31/(?:0[13578]|1[02])|(?:29|30)/(?:0[1,3-9]|1[0-2]))/\d{4}"
          + @"|29/02/(?:(?:\d\d(?:0[48]|[2468][048]|[13579][26]))|(?:[02468][048]|[13579][26])00)"
          + @"|(?:0[1-9]|1\d|2[0-8])/(?:0[1-9]|1[0-2])/\d{4})$",
            RegexOptions.Compiled);

        public class Token
        {
            public string Type { get; }
            public string Value { get; }
            public int Line { get; }
            public int Column { get; }

            public Token(string type, string value, int line, int column)
            {
                Type = type;
                Value = value;
                Line = line;
                Column = column;
            }

            public override string ToString() =>
                $"Line {Line}, Col {Column}: {Type} → {Value}";
        }

        public class TokenResult
        {
            public List<Token> Tokens { get; } = new List<Token>();
            public List<Error> Errors { get; } = new List<Error>();

            public record Error(string Lexeme, int Line, int Column, string Expected);
        }

        /// <summary>
        /// Анализирует весь текст, возвращая корректные токены и ошибки с объяснением.
        /// </summary>
        public static TokenResult AnalyzeAll(string text)
        {
            var result = new TokenResult();
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i]
                    .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                int offset = 0;
                foreach (var part in parts)
                {
                    // вычисляем номер колонки (1-based)
                    int col = lines[i].IndexOf(part, offset, StringComparison.Ordinal) + 1;
                    offset = col - 1 + part.Length;

                    if (ReBtcAddress.IsMatch(part))
                    {
                        result.Tokens.Add(new Token("BtcAddress", part, i + 1, col));
                    }
                    else if (ReInn.IsMatch(part))
                    {
                        result.Tokens.Add(new Token("INN", part, i + 1, col));
                    }
                    else if (ReDate.IsMatch(part))
                    {
                        result.Tokens.Add(new Token("Date", part, i + 1, col));
                    }
                    else
                    {
                        // Подбираем, что, вероятно, ожидалось
                        string expected;
                        if (part.Contains("/"))
                            expected = "Date DD/MM/YYYY";
                        else if (part.All(char.IsDigit))
                            expected = "INN (10 или 12 цифр)";
                        else if (part.StartsWith("1")
                              || part.StartsWith("3")
                              || part.StartsWith("bc1"))
                            expected = "Bitcoin address";
                        else
                            expected = "Bitcoin address, INN или Date";

                        result.Errors.Add(new TokenResult.Error(
                            Lexeme: part,
                            Line: i + 1,
                            Column: col,
                            Expected: expected
                        ));
                    }
                }
            }

            return result;
        }
    }
}
