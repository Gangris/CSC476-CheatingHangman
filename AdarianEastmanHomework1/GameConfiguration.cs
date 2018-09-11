using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdarianEastmanHomework1
{
    class GameConfiguration
    {
        private int _wordLength = 2;
        private int _guessLength = 3;

        public int WordLength
        {
            get => _wordLength;
            set
            {
                if (value < 2)
                    throw new ArgumentOutOfRangeException($"ERROR: The minimum length for a word is 2. Please enter a larger word length.");
                if (value == 23 || value == 25 || value == 26 || value == 27)
                    throw new ArgumentOutOfRangeException($"ERROR: There are no words of length {value}, please select a different length.");
                if (value > 29)
                    throw new ArgumentOutOfRangeException($"ERROR: The maximum length for a word is 29. Please enter a smaller word length");

                _wordLength = value;
            }
        }

        public int GuessLength
        {
            get => _guessLength;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException($"ERROR: You need some guesses to play this game. Enter more than 0 guesses");

                _guessLength = value;
            }
        }

        public bool ShowDebug { get; set; }
    }
}
