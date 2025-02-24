using System.Text;

namespace Borrow_Wheeler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var time_before = System.Environment.TickCount;
            string fileContent = "";
            try
            {
                // Path to the file
                string filePath = @"D:\College\6th TERM\Algorithm Analysis & Design\5 Project\MATERIALS\[2] Burrow-Wheeler Compression\BurrowsWheeler\BurrowsWheeler\Test Files\Large Cases\Small\aesop-4copies.txt";

                fileContent = File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading the file: " + e.Message);
                return;
            }

            int chunkSize = 50000;
            int n = fileContent.Length;
            List<string> chunks = new List<string>();

            for (int i = 0; i < n; i += chunkSize)
            {
                chunks.Add(fileContent.Substring(i, Math.Min(chunkSize, n - i)));
            }

            foreach (string chunk in chunks)
            {
                (string, int) transformed_text = Transform(chunk);
                List<int> encoded_text = Encode(transformed_text.Item1);
                string decoded_text = Decode(encoded_text);
                if (decoded_text.Equals(transformed_text.Item1))
                {
                    Console.WriteLine("Success");
                }
                else
                {
                    Console.WriteLine("Failed");
                }
            }
            var time_after = System.Environment.TickCount;
            Console.WriteLine("Time Taken in MilliSeconds = " + (time_after - time_before));
        }

        static string Inverse(string transformedText, int rowNumber)
        {
            int n = transformedText.Length;

            List<char> firstColumn = new List<char>(transformedText);
            List<char> lastColumn = new List<char>(transformedText);

            Dictionary<char, Queue<int>> chars_in_text_with_index = new Dictionary<char, Queue<int>>();

            for (int i = 0; i < n; i++)
            {
                if (!chars_in_text_with_index.ContainsKey(lastColumn[i]))
                {
                    chars_in_text_with_index[lastColumn[i]] = new Queue<int>();
                }
                chars_in_text_with_index[lastColumn[i]].Enqueue(i);
            }

            firstColumn.Sort();

            int[] next_array = new int[n];

            for (int i = 0; i < n; i++)
            {
                next_array[i] = chars_in_text_with_index[firstColumn[i]].Dequeue();
            }

            int index = rowNumber;
            StringBuilder inversed_text = new StringBuilder();

            for (int i = 0; i < n; i++)
            {
                index = next_array[index];
                inversed_text.Append(transformedText[index]);
            }

            return inversed_text.ToString();
        }

        static (string, int) Transform(string test)
        {
            if (string.IsNullOrEmpty(test))
            {
                throw new ArgumentException("Input text cannot be null or empty.");
            }

            StringBuilder transform_Output = new StringBuilder();
            int row_number = 0;

            int length_of_text = test.Length;

            List<int> Original_Indexes = new List<int>();
            for (int i = 0; i < length_of_text; i++)
            {
                Original_Indexes.Add(i);
            }

            QuickSort(Original_Indexes, test, 0, Original_Indexes.Count - 1);
            
            foreach (var index in Original_Indexes)
            {
                if (index == 0)
                {
                    row_number = transform_Output.Length;
                }
                transform_Output.Append(test[(index + length_of_text - 1) % length_of_text]);
            }

            return (transform_Output.ToString(), row_number);
        }
        static void RadixSort(List<int> indexes, string text, int left, int right, int position = 0)
        {
            if (left < right && position < text.Length)
            {
                List<List<int>> buckets = new List<List<int>>();
                for (int i = 0; i < 256; i++) // Assuming ASCII characters
                {
                    buckets.Add(new List<int>());
                }

                foreach (int index in indexes)
                {
                    int charIndex = (index + position) % text.Length;
                    char c = text[charIndex];
                    int bucketIndex = (int)c; // Ensure bucketIndex is within the valid range
                    buckets[bucketIndex].Add(index);
                }

                int currentIndex = left;
                foreach (var bucket in buckets)
                {
                    RadixSort(bucket, text, currentIndex, currentIndex + bucket.Count - 1, position + 1);
                    foreach (int index in bucket)
                    {
                        indexes[currentIndex] = index;
                        currentIndex++;
                    }
                }
            }
        }

        static void QuickSort(List<int> indexes, string text, int left, int right)
        {
            if (left < right)
            {
                //Console.WriteLine("hi2");
                int partitionIndex = Partition(indexes, text, left, right);
                Parallel.Invoke(
                    () =>
                    {

                        QuickSort(indexes, text, left, partitionIndex - 1);
                    }
                    , () =>
                    {

                        QuickSort(indexes, text, partitionIndex + 1, right);
                    }
                    );
            }
        }

        static int Partition(List<int> indexes, string text, int left, int right)
        {
            int pivotIndex = left;
            int pivotValue = indexes[pivotIndex];
            int i = left + 1;

            for (int j = left + 1; j <= right; j++)
            {
                if (Compare(indexes[j], pivotValue, text) < 0)
                {
                    Swap(indexes, i, j);
                    i++;
                }
            }

            Swap(indexes, pivotIndex, i - 1);
            return i - 1;
        }

        static int Compare(int index1, int index2, string text)
        {
            int length = text.Length;
            for (int i = 0; i < length; i++)
            {
                char char1 = text[(index1 + i) % length];
                char char2 = text[(index2 + i) % length];
                if (char1 < char2) return -1;
                if (char1 > char2) return 1;
            }
            return 0;
        }

        static void Swap(List<int> indexes, int i, int j)
        {
            int temp = indexes[i];
            indexes[i] = indexes[j];
            indexes[j] = temp;
        }

        public static List<int> Encode(string input)
        {
            //Unicode Function Part Instead of ASCII to Extend Chachters range
            int unicodeRange = 65536; 
            char[] unicodeChars = new char[unicodeRange];
            int[] charPositions = new int[unicodeRange];
            for (int i = 0; i < unicodeRange; i++)
            {
                unicodeChars[i] = (char)i;
                charPositions[i] = i;
            }

            List<int> encoded = new List<int>();

            foreach (char symbol in input)
            {
                //Check for Exception Part 
                int unicodeValue = symbol;
                if (unicodeValue < 0 || unicodeValue >= unicodeRange)
                {
                    throw new IndexOutOfRangeException($"Unicode value of character '{symbol}' is out of range: {unicodeValue}");
                }

                //Adding the Charachter unicode position to The encoded list
                int charIndex = charPositions[unicodeValue];
                encoded.Add(charIndex);

                //Shifting
                for (int i = charIndex; i > 0; i--)
                {
                    //Adding 1 to free Space for the Charachter at the Front 
                    charPositions[unicodeChars[i - 1]]++;
                    //Shifting Charachters to make room at the Front
                    unicodeChars[i] = unicodeChars[i - 1];
                }
                //Moving To Front
                unicodeChars[0] = symbol;
                charPositions[symbol] = 0;
            }

            return encoded;
        }

        public static string Decode(List<int> encoded)
        {
            //Unicode Function Part Instead of ASCII to Extend Chachters range
            int unicodeRange = 65536; 
            char[] unicodeChars = new char[unicodeRange];
            int[] charPositions = Enumerable.Range(0, unicodeRange).ToArray();
            for (int i = 0; i < unicodeRange; i++)
            {
                unicodeChars[i] = (char)i;
            }

            //Decoding
            StringBuilder decoded = new StringBuilder();
            foreach (int position in encoded)
            {
                // Find the character by its position
                char symbol = unicodeChars[position];
                decoded.Append(symbol);

                // Shift Charachters 
                Array.Copy(unicodeChars, 0, unicodeChars, 1, position);
                unicodeChars[0] = symbol;

                // Update the positions of the characters that have been shifted
                for (int i = 1; i <= position; i++)
                {
                    charPositions[unicodeChars[i]] = i;
                }
                charPositions[symbol] = 0;
            }

            return decoded.ToString();
        }
    }
}