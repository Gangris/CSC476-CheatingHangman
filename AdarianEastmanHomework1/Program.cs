using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace AdarianEastmanHomework1
{
    public class Program
    {
        private static List<string> _allWords;
        private static Dictionary<int, List<string>> _wordsByLength;
        private static GameConfiguration _gc;
        private static GameState _gs;
        private static bool _replay = false;

        public static void Main(string[] args)
        { 
            Console.WriteLine(@"Welcome to Hangman!");

            LoadDictionary();

            StartGame();
        }

        private static void LoadDictionary()
        {
            var words = System.IO.File.ReadAllLines("./dictionary.txt");
            _allWords = new List<string>(System.IO.File.ReadLines("./dictionary.txt"));
            _wordsByLength = new Dictionary<int, List<string>>();

            // Sort out the words by length.
            foreach (var word in _allWords)
            {
                // Ensure dictionary has the length specified, if not, create it.
                if (!_wordsByLength.ContainsKey(word.Length))
                {
                    _wordsByLength.Add(word.Length, new List<string>());
                }

                // To ensure accurate matching, put the word in all upper case to the list.
                _wordsByLength[word.Length].Add(word.ToUpper());
            }
        }

        private static void StartGame()
        {
            SetupGame();

            PlayGame();
        }

        private static void SetupGame()
        {
            // Create default configuration of game
            _gc = new GameConfiguration();
            _gs = new GameState();

            EnterWordLength();
            _gs.SetWordPotentials(_wordsByLength[_gc.WordLength]);
            _gs.DisplayWord = SetStartingDisplayWord();

            EnterGuesses();
            _gs.Guesses = _gc.GuessLength;

            EnterDebug();
        }

        private static void EnterWordLength()
        {
            try
            {
                Console.WriteLine(@"Please enter a word length:");
                var wordLength = Console.ReadLine();

                if (wordLength != null && StringIsDigits(wordLength))
                {
                    _gc.WordLength = int.Parse(wordLength);
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"ERROR: Please enter a numerical value.");
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
                EnterWordLength();
            }
        }

        private static string SetStartingDisplayWord()
        {
            var rs = "";

            for (var i = 0; i < _gc.WordLength; i++)
            {
                rs = rs + "_";
            }

            return rs;
        }

        private static void EnterGuesses()
        {
            try
            {
                Console.WriteLine(@"Please enter the number of incorrect guesses for this game:");
                var guesses = Console.ReadLine();

                if (guesses != null && StringIsDigits(guesses))
                {
                    _gc.GuessLength = int.Parse(guesses);
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"ERROR: Please enter a numerical value.");
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
                EnterGuesses();
            }
        }

        private static void EnterDebug()
        {
            try
            {
                Console.WriteLine(@"Is this game in debug mode (y/N)?");
                var debug = Console.ReadLine();

                _gc.ShowDebug = !string.IsNullOrEmpty(debug) && BooleanInputCleanup(debug);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
                EnterDebug();
            }
        }

        // Helper function to detect non digit input
        private static bool StringIsDigits(string s)
        {
            foreach (var c in s)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        // Helper function to clean up true/false input
        private static bool BooleanInputCleanup(string s)
        {
            switch (s.ToUpper())
            {
                case "N":
                    return false;
                case "Y":
                    return true;
                default:
                    throw new ArgumentOutOfRangeException($"ERROR: Please enter a valid choice.");
            }
        }

        // Game Loop
        private static void PlayGame()
        {
            Console.WriteLine(@"Number of guesses remaining: {0}", _gs.Guesses);

            if (_gc.ShowDebug)
                Console.WriteLine(@"[# of words possible: {0}]", _gs.WordPotentials.Count);

            Console.WriteLine(_gs.DisplayWord);

            OutputExistingGuesses();

            var guess = GatherGuess();
            
            ProcessGuess(guess);
        }

        // Display guesses
        private static void OutputExistingGuesses()
        {
            Console.Write(@"Used Letters: ");
            if (_gs.UsedGuesses.Count == 0)
            {
                Console.WriteLine(@"None");
            }
            else
            {
                foreach (char c in _gs.UsedGuesses)
                {
                    Console.Write(c + @" ");
                }
                Console.WriteLine();
            }
        }

        // Get guess from player
        private static char GatherGuess()
        {
            try
            {
                Console.WriteLine(@"Please enter your guess:");
                var debug = Console.ReadLine();

                if (debug == null || debug.Length != 1)
                    throw new ArgumentOutOfRangeException($"ERROR: Please enter a single letter for a guess.");

                var d = debug.ToUpper().ToCharArray()[0];

                if (d < 'A' || d > 'Z')
                    throw new ArgumentOutOfRangeException($"ERROR: Please enter a single letter for a guess.");

                return d;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
                return GatherGuess();
            }
        }

        // Finds largest 'word family'
        private static void ProcessGuess(char g)
        {
            var wordPotentials = MapPotentialWords(g);
            var winningMask = "";
            var highestCount = 0;

            _gs.AddGuess(g);

            // Finds largest word family
            foreach (var entry in wordPotentials)
            {
                if (highestCount < entry.Value.Count)
                {
                    highestCount = entry.Value.Count;
                    winningMask = entry.Key;
                }
            }

            // Sets current winning pool
            _gs.SetWordPotentials(wordPotentials[winningMask]);

            if (winningMask.Contains("1"))
            {
                Console.WriteLine(@"Correct Guess!");
                ChangeDisplayWord(winningMask, g);

                if (!_gs.DisplayWord.Contains("_"))
                    EndGame(true);
            }
            else
            {
                Console.WriteLine(@"Incorrect Guess!");
                _gs.Guesses = _gs.Guesses - 1;

                if (_gs.Guesses == 0)
                    EndGame(false);
            }

            PlayGame();
        }

        // Gives a way to make 'word families'
        private static Dictionary<string, List<string>> MapPotentialWords(char g)
        {
            Dictionary<string, List<string>> rd = new Dictionary<string, List<string>>();
            foreach (var word in _gs.WordPotentials)
            {
                var mask = MaskWord(word, g);

                if (!rd.ContainsKey(mask))
                    rd.Add(mask, new List<string>());

                rd[mask].Add(word);
            }
            return rd;
        }

        // Maps words to word families
        private static string MaskWord(string word, char g)
        {
            var rs = "";

            foreach (var c in word)
            {
                if (c == g)
                {
                    rs = rs + "1";
                }
                else
                {
                    rs = rs + "0";
                }
            }

            return rs;
        }

        // Change the display word to show number of letters missing
        private static void ChangeDisplayWord(string mask, char g)
        {
            var rs = new StringBuilder(_gs.DisplayWord);

            for (var i = 0; i < mask.Length; i++)
            {
                var c = mask[i];
                if (c == '1')
                {
                    rs.Remove(i, 1);
                    rs.Insert(i, g);
                }
            }

            _gs.DisplayWord = rs.ToString();
        }

        // End game
        private static void EndGame(bool win)
        {
            if (win)
            {
                Console.WriteLine(@"You found the correct word! It was {0}!", _gs.DisplayWord);
            }
            else
            {
                Console.WriteLine(@"You ran out of guesses! The word was {0}.", SelectRandomWord());
            }

            EnterReplay();

            if (_replay)
            {
                StartGame();
                _replay = false;
            }
            else
            {
                Console.WriteLine(@"Thank you for playing Hangman! Press any key to close the application.");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        private static string SelectRandomWord()
        {
            var r = new Random();
            return _gs.WordPotentials[r.Next(0, _gs.WordPotentials.Count)];
        }

        private static void EnterReplay()
        {
            try
            {
                Console.WriteLine(@"Would you like to play again (Y/n)?");
                var replay = Console.ReadLine();

                _replay = string.IsNullOrEmpty(replay) || BooleanInputCleanup(replay);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
                EnterReplay();
            }
        }
    }
}
