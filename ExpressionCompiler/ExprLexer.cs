using lab1_compiler.Bar;
using System;
using System.Collections.Generic;

namespace lab1_compiler.ExpressionCompiler
{
    public class ExprLexer
    {
        private readonly string _text;
        private int _pos;

        public ExprLexer(string text)
        {
            _text = text + "\0";  // дополняем нуль-терминатором
            _pos = 0;
        }

        public List<ExprToken> Tokenize()
        {
            var tokens = new List<ExprToken>();

            while (true)
            {
                char c = _text[_pos];

                // пропускаем пробелы
                if (char.IsWhiteSpace(c))
                {
                    _pos++;
                    continue;
                }

                // идентификатор: буква + буквы/цифры
                if (char.IsLetter(c))
                {
                    int start = _pos;
                    while (char.IsLetterOrDigit(_text[_pos]))
                        _pos++;
                    string lex = _text.Substring(start, _pos - start);
                    tokens.Add(new ExprToken(TokenType.Id, lex, start));
                    continue;
                }

                // одиночные символы
                switch (c)
                {
                    case '+':
                        tokens.Add(new ExprToken(TokenType.Plus, "+", _pos));
                        _pos++;
                        break;
                    case '-':
                        tokens.Add(new ExprToken(TokenType.Minus, "-", _pos));
                        _pos++;
                        break;
                    case '*':
                        tokens.Add(new ExprToken(TokenType.Mul, "*", _pos));
                        _pos++;
                        break;
                    case '/':
                        tokens.Add(new ExprToken(TokenType.Div, "/", _pos));
                        _pos++;
                        break;
                    case '(':
                        tokens.Add(new ExprToken(TokenType.LParen, "(", _pos));
                        _pos++;
                        break;
                    case ')':
                        tokens.Add(new ExprToken(TokenType.RParen, ")", _pos));
                        _pos++;
                        break;
                    case '\0':
                        tokens.Add(new ExprToken(TokenType.End, string.Empty, _pos));
                        return tokens;
                    default:
                        tokens.Add(new ExprToken(TokenType.Unknown, c.ToString(), _pos));
                        _pos++;
                        break;
                }
            }
        }
    }
}
