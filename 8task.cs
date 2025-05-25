using System;
using System.Collections.Generic;

namespace lab1_compiler
{
    /// <summary>
    /// Рекурсивный спуск для варианта 14:
    ///
    /// STMT ::= if "(" EXPR ")" STMT [ else STMT ]
    ///        | ε
    ///
    /// EXPR ::= VARIABLE OPERATOR VALUE
    /// OPERATOR ::= "==" | "<=" | ">=" | "!=" | "<" | ">"
    /// VARIABLE ::= letter { letter | digit }
    /// VALUE    ::= digit { digit }
    ///
    /// ERROR RECOVERY:
    ///   — при ошибке в STMT: SyncTo("else", ")") и попытаться распарсить ветку else
    ///   — при ошибке в EXPR: SyncTo(")"), а затем Expect(')')
    /// </summary>
    public class Parser
    {
        private readonly string _input;
        private int _pos;
        private readonly List<string> _steps = new List<string>();

        private static readonly string[] Operators = { "==", "<=", ">=", "!=", "<", ">" };

        public Parser(string input)
        {
            _input = input;
            _pos = 0;
        }

        public IReadOnlyList<string> Steps => _steps;

        public void Parse()
        {
            _steps.Clear();
            ParseSTMT();

            SkipWS();
            if (!IsAtEnd())
                _steps.Add($"ParseERROR → не обработан остаток \"{_input.Substring(_pos)}\"");
        }

        private void ParseSTMT()
        {
            SkipWS();

            // 1) если начинается с "if" — разбираем конструкцию  
            if (LookAheadIs("if"))
            {
                try
                {
                    _steps.Add("ParseSTMT → if");
                    MatchKeyword("if");
                    Expect('(');
                    ParseEXPR();
                    Expect(')');
                    // тело then
                    ParseSTMT();

                    SkipWS();
                    if (MatchKeyword("else"))
                    {
                        _steps.Add("ParseSTMT → else");
                        ParseSTMT();
                    }
                }
                catch (Exception ex)
                {
                    // лог ошибки
                    _steps.Add($"ParseSTMT → ERROR: {ex.Message}");
                    // пропускаем вперёд до else или ')'
                    SyncTo("else", ")");
                    // если нашли else — разбираем ветку else
                    SkipWS();
                    if (MatchKeyword("else"))
                    {
                        _steps.Add("ParseSTMT → else");
                        ParseSTMT();
                    }
                }
                return;
            }

            // 2) если следующий токен в FOLLOW(STMT) = { "else", ")", EOF } → ε-производство
            if (IsFollowStmt())
            {
                _steps.Add("ParseSTMT → ε");
                return;
            }

            // 3) иначе — неожиданный токен, восстанавливаемся
            {
                string bad = PeekToken();
                _steps.Add($"ParseSTMT → ERROR: unexpected token \"{bad}\" at {_pos}");
                SyncTo("else", ")");
                SkipWS();
                if (MatchKeyword("else"))
                {
                    _steps.Add("ParseSTMT → else");
                    ParseSTMT();
                }
            }
        }

        private void ParseEXPR()
        {
            SkipWS();
            try
            {
                _steps.Add("ParseEXPR → VARIABLE OPERATOR VALUE");
                ParseVARIABLE();
                SkipWS();
                ParseOPERATOR();
                SkipWS();
                ParseVALUE();
            }
            catch (Exception ex)
            {
                _steps.Add($"ParseEXPR → ERROR: {ex.Message}");
                // пропускаем вперёд до ')', чтобы закрыть скобку
                SyncTo(")");
            }
        }

        private void ParseVARIABLE()
        {
            SkipWS();
            if (_pos < _input.Length && char.IsLetter(_input[_pos]))
            {
                int start = _pos;
                _pos++;
                while (_pos < _input.Length && char.IsLetterOrDigit(_input[_pos]))
                    _pos++;
                var name = _input.Substring(start, _pos - start);
                _steps.Add($"  ParseVARIABLE → \"{name}\"");
            }
            else
                throw new Exception($"ParseVARIABLE: ожидалась буква в позиции {_pos}");
        }

        private void ParseOPERATOR()
        {
            SkipWS();
            foreach (var op in Operators)
            {
                if (_input.Substring(_pos).StartsWith(op))
                {
                    _steps.Add($"  ParseOPERATOR → \"{op}\"");
                    _pos += op.Length;
                    return;
                }
            }
            throw new Exception($"ParseOPERATOR: ожидался оператор в позиции {_pos}");
        }

        private void ParseVALUE()
        {
            SkipWS();
            if (_pos < _input.Length && char.IsDigit(_input[_pos]))
            {
                int start = _pos;
                while (_pos < _input.Length && char.IsDigit(_input[_pos]))
                    _pos++;
                var val = _input.Substring(start, _pos - start);
                _steps.Add($"  ParseVALUE → \"{val}\"");
            }
            else
                throw new Exception($"ParseVALUE: ожидалось число в позиции {_pos}");
        }

        private void Expect(char ch)
        {
            SkipWS();
            if (Peek() == ch)
            {
                _steps.Add($"  Expect → saw '{ch}'");
                _pos++;
            }
            else
                throw new Exception($"Expect: ожидался '{ch}' в позиции {_pos}");
        }

        private bool MatchKeyword(string kw)
        {
            SkipWS();
            if (_input.Substring(_pos).StartsWith(kw))
            {
                _pos += kw.Length;
                return true;
            }
            return false;
        }

        private bool LookAheadIs(string kw)
        {
            SkipWS();
            return _input.Substring(_pos).StartsWith(kw);
        }

        private bool IsFollowStmt()
        {
            SkipWS();
            if (_pos >= _input.Length) return true;          // EOF
            if (_input.Substring(_pos).StartsWith("else")) return true;
            if (Peek() == ')') return true;
            return false;
        }

        private string PeekToken()
        {
            SkipWS();
            if (_pos >= _input.Length) return "EOF";
            if (char.IsLetter(_input[_pos]))
            {
                int j = _pos;
                while (j < _input.Length && char.IsLetterOrDigit(_input[j])) j++;
                return _input.Substring(_pos, j - _pos);
            }
            return _input[_pos].ToString();
        }

        private void SkipWS()
        {
            while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos]))
                _pos++;
        }

        private char Peek() => _pos < _input.Length ? _input[_pos] : '\0';
        private bool IsAtEnd() => _pos >= _input.Length;

        private void SyncTo(params string[] syncTokens)
        {
            while (_pos < _input.Length)
            {
                SkipWS();
                foreach (var tok in syncTokens)
                    if (_input.Substring(_pos).StartsWith(tok))
                        return;
                _pos++;
            }
        }
    }
}
