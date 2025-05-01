namespace lab1_compiler.ExpressionCompiler
{
    public enum TokenType
    {
        Id,        // идентификатор: letter{letter|digit}*
        Plus,      // +
        Minus,     // -
        Mul,       // *
        Div,       // /
        LParen,    // (
        RParen,    // )
        End,       // конец входа
        Unknown    // любой другой символ
    }

    public class ExprToken
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public int Position { get; }

        public ExprToken(TokenType type, string lexeme, int pos)
        {
            Type = type;
            Lexeme = lexeme;
            Position = pos;
        }
    }
}

