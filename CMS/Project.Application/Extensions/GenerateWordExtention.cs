namespace Project.Application.Extensions
{
    public static class GenerateWordExtention
    {
        public static List<string> GenerateWords(int length)
        {
            var words = new List<string>();
            var alphabet = "abcdefghijklmnopqrstuvwxyz";
            GenerateWordsRecursive(alphabet, length, "", words);
            return words;
        }

        private static void GenerateWordsRecursive(string alphabet, int length, string currentWord, List<string> words)
        {
            if (currentWord.Length == length)
            {
                words.Add(currentWord);
                return;
            }

            for (int i = 0; i < alphabet.Length; i++)
            {
                var random = new Random();
                char currentChar = alphabet[random.Next(0, alphabet.Length - 1)];
                GenerateWordsRecursive(alphabet, length, currentWord + currentChar, words);
            }
        }
    }
}
