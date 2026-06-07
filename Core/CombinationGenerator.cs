using System;
using System.Collections.Generic;
using System.Threading;

namespace BruteforceApp1.Core
{
    // Generates every possible combination of characters
    // Think of it like counting in base-36 (a-z0-9)
    // Example length 2: aa, ab, ac ... z9, 99
    public class CombinationGenerator
    {
        private readonly char[] _charset;

        public CombinationGenerator(char[] charset)
        {
            _charset = charset;
        }

        // startCharIndex and endCharIndex split work between threads
        // Thread 0 gets first chars a-e, Thread 1 gets f-j, etc.
        public IEnumerable<string> GenerateCombinations(
            int length,
            int startCharIndex,
            int endCharIndex,
            CancellationToken cancellationToken)
        {
            int charsetSize = _charset.Length;
            int[] indices = new int[length];
            indices[0] = startCharIndex;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (indices[0] >= endCharIndex)
                    yield break;

                // Build the candidate string
                char[] candidate = new char[length];
                for (int i = 0; i < length; i++)
                    candidate[i] = _charset[indices[i]];
                yield return new string(candidate);

                // Increment from the right (like counting)
                int position = length - 1;
                while (position >= 0)
                {
                    indices[position]++;
                    if (position == 0 && indices[0] >= endCharIndex)
                        yield break;
                    if (indices[position] < charsetSize)
                        break;
                    indices[position] = 0;
                    position--;
                }

                if (position < 0)
                    yield break;
            }
        }

        public long CountCombinations(int length)
        {
            long count = 1;
            for (int i = 0; i < length; i++)
                count *= _charset.Length;
            return count;
        }

        public long CountAllCombinations(int maxLength)
        {
            long total = 0;
            for (int len = 1; len <= maxLength; len++)
                total += CountCombinations(len);
            return total;
        }
    }
}