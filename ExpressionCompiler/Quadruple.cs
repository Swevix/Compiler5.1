namespace lab1_compiler.ExpressionCompiler
{
    public class Quadruple
    {
        public string Op { get; }
        public string Arg1 { get; }
        public string Arg2 { get; }
        public string Result { get; }

        public Quadruple(string op, string arg1, string arg2, string result)
        {
            Op = op;
            Arg1 = arg1;
            Arg2 = arg2;
            Result = result;
        }

        public override string ToString()
            => $"({Op}, {Arg1}, {Arg2}, {Result})";
    }
}
