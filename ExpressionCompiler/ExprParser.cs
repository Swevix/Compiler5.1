using lab1_compiler.Bar;
using System.Collections.Generic;

namespace lab1_compiler.ExpressionCompiler
{
    public class ExprParser
    {
        private readonly List<ExprToken> _tokens;
        private int _idx;
        private int _tempCount;

        public List<string> SyntaxErrors { get; } = new List<string>();
        public List<Quadruple> Quads { get; } = new List<Quadruple>();

        private ExprToken Current => _tokens[_idx];

        public ExprParser(List<ExprToken> tokens)
        {
            _tokens = tokens;
            _idx = 0;
            _tempCount = 0;
        }

        // Вспомогательный «съедатель» токена
        private void Eat(TokenType expected)
        {
            if (Current.Type == expected)
            {
                _idx++;
            }
            else
            {
                SyntaxErrors.Add(
                  $"Ожидался {expected} на позиции {Current.Position}, но встретилось '{Current.Lexeme}'");
                _idx++; // попытка восстановления
            }
        }

        private string NewTemp() => "t" + (_tempCount++);

        // E → T A
        public string ParseE()
        {
            string left = ParseT();
            return ParseA(left);
        }

        // A → ε | + T A | - T A
        private string ParseA(string inh)
        {
            if (Current.Type == TokenType.Plus || Current.Type == TokenType.Minus)
            {
                string op = Current.Lexeme;
                Eat(Current.Type);
                string right = ParseT();
                string tmp = NewTemp();
                Quads.Add(new Quadruple(op, inh, right, tmp));
                return ParseA(tmp);
            }
            return inh;
        }

        // T → O B
        private string ParseT()
        {
            string left = ParseO();
            return ParseB(left);
        }

        // B → ε | * O B | / O B
        private string ParseB(string inh)
        {
            if (Current.Type == TokenType.Mul || Current.Type == TokenType.Div)
            {
                string op = Current.Lexeme;
                Eat(Current.Type);
                string right = ParseO();
                string tmp = NewTemp();
                Quads.Add(new Quadruple(op, inh, right, tmp));
                return ParseB(tmp);
            }
            return inh;
        }

        // O → id | ( E )
        private string ParseO()
        {
            if (Current.Type == TokenType.Id)
            {
                string name = Current.Lexeme;
                Eat(TokenType.Id);
                return name;
            }
            if (Current.Type == TokenType.LParen)
            {
                Eat(TokenType.LParen);
                string inner = ParseE();
                Eat(TokenType.RParen);
                return inner;
            }

            SyntaxErrors.Add($"Ожидался идентификатор или '(', найден '{Current.Lexeme}'");
            Eat(Current.Type);
            return "err";
        }
    }
}
