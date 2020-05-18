namespace HuffmanCompression.TreeElements
{
    internal abstract class Box
    {
        internal abstract Box Parent { get; set; }
        internal abstract int Number { get; set; }
        internal abstract int Depth { get; }
        internal abstract int Sum { get; }
    }
}