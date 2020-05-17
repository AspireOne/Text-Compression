namespace HuffmanCompression.TreeElements
{
    internal class BoxesBox : Box
    {
        internal const byte LeftIndex = 0;
        internal const byte RightIndex = 1;
        internal readonly Box Left;
        internal readonly Box Right;
        internal override int Depth => (Left.Depth > Right.Depth ? Left.Depth : Right.Depth) + 1;
        internal override int Sum => Left.Sum + Right.Sum;
        internal override int Number { get; set; }
        internal override Box Parent { get; set; }

        internal BoxesBox(Box left, Box right)
        {
            left.Parent = this;
            right.Parent = this;

            left.Number = LeftIndex;
            right.Number = RightIndex;

            Left = left;
            Right = right;
        }
    }
}