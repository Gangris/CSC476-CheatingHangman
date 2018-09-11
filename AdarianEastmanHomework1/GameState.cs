using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdarianEastmanHomework1
{
    class GameState
    {
        private List<string> _wordPotentials = new List<string>();
        private List<char> _usedGuesses = new List<char>();

        public string DisplayWord { get; set; }
        public int Guesses { get; set; }

        public List<string> WordPotentials
        {
            get => _wordPotentials;
            set => throw new Exception($"Cannot overwrite WordPotentials");
        }

        public List<char> UsedGuesses
        {
            get => _usedGuesses;
            set => throw new Exception($"Cannot overwrite UsedGuesses");
        }

        public void AddGuess(char n)
        {
            _usedGuesses.Add(n);
        }

        public void SetWordPotentials(List<string> n)
        {
            _wordPotentials.Clear();
            _wordPotentials.AddRange(n);
        }
    }
}
