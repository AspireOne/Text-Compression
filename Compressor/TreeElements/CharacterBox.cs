namespace HuffmanCompression.TreeElements
{
    internal class CharacterBox : Box
    {
        internal char Character;
        private readonly int _sum;
        internal override Box Parent { get; set; }
        internal override int Number { get; set; }
        internal override int Sum => _sum;
        internal override int Depth => 0;

        internal CharacterBox(char character, int sum)
        {
            Character = character;
            _sum = sum;
        }
    }
}