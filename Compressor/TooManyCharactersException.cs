using System;

namespace HuffmanCompression
{
    internal class TooManyCharactersException : Exception
    {
        public TooManyCharactersException()
        {
        }

        public TooManyCharactersException(string message)
            : base(message) { }

        public TooManyCharactersException(string message, Exception inner)
            : base(message, inner) { }
    }
}