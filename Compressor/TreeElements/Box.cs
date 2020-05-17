namespace HuffmanCompression.TreeElements
{
    internal abstract class Box
    {
        abstract internal Box Parent { get; set; }
        abstract internal int Number { get; set; }
        abstract internal int Depth { get; }
        abstract internal int Sum { get; }
    }
}