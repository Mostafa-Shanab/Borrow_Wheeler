/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class BurrowsWheeler
{
    // Burrows-Wheeler Transform
    public static (string, int) Transform(string input)
    {
        int n = input.Length;
        string[] table = new string[n];

        for (int i = 0; i < n; i++)
        {
            table[i] = input.Substring(i) + input.Substring(0, i);
        }

        Array.Sort(table);

        string transformedText = "";
        int rowNumber = 0;

        for (int i = 0; i < n; i++)
        {
            transformedText += table[i][n - 1];
            if (table[i] == input)
            {
                rowNumber = i;
            }
        }

        return (transformedText, rowNumber);
    }

    // Burrows-Wheeler Inverse
    public static string Inverse(string transformedText, int rowNumber)
    {
        int n = transformedText.Length;
        List<string> table = new List<string>();

        for (int i = 0; i < n; i++)
        {
            table.Add("");
            for (int j = 0; j < n; j++)
            {
                table[i] = transformedText[j] + table[i];
            }
            transformedText = transformedText.Substring(1) + transformedText[0];
        }

        table.Sort();

        string originalText = table[rowNumber];

        return originalText;
    }

    // Move-to-front Encoding
    public static List<int> MoveToFrontEncode(string input)
    {
        List<int> result = new List<int>();
        List<char> symbols = Enumerable.Range(0, 256).Select(i => (char)i).ToList();

        foreach (char c in input)
        {
            int index = symbols.IndexOf(c);
            result.Add(index);
            symbols.RemoveAt(index);
            symbols.Insert(0, c);
        }

        return result;
    }

    // Move-to-front Decoding
    public static string MoveToFrontDecode(List<int> input)
    {
        List<char> symbols = Enumerable.Range(0, 256).Select(i => (char)i).ToList();
        string result = "";

        foreach (int index in input)
        {
            char c = symbols[index];
            result += c;
            symbols.RemoveAt(index);
            symbols.Insert(0, c);
        }

        return result;
    }

    // Huffman Coding
    public static Dictionary<char, string> HuffmanEncode(string input)
    {
        Dictionary<char, int> freq = input.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
        List<Node> nodes = freq.Select(pair => new Node { Symbol = pair.Key, Frequency = pair.Value }).ToList();

        while (nodes.Count > 1)
        {
            nodes = nodes.OrderBy(n => n.Frequency).ToList();
            Node parent = new Node { Symbol = '\0', Frequency = nodes[0].Frequency + nodes[1].Frequency, Left = nodes[0], Right = nodes[1] };
            nodes.RemoveRange(0, 2);
            nodes.Add(parent);
        }

        Node root = nodes.FirstOrDefault();
        Dictionary<char, string> codes = new Dictionary<char, string>();

        void Traverse(Node node, string code)
        {
            if (node.Symbol != '\0')
            {
                codes[node.Symbol] = code;
                return;
            }

            Traverse(node.Left, code + "0");
            Traverse(node.Right, code + "1");
        }

        if (root != null)
        {
            Traverse(root, "");
        }

        return codes;
    }

    // Node class for Huffman coding
    public class Node
    {
        public char Symbol { get; set; }
        public int Frequency { get; set; }
        public Node Left { get; set; }
        public Node Right { get; set; }
    }
}

class Program
{
    static void Main()
    {
        // Example input
        string input = "abfacadabfa";

        // Burrows-Wheeler Transform
        (string transformedText, int rowNumber) = BurrowsWheeler.Transform(input);
        Console.WriteLine("Transformed Text: " + transformedText);
        Console.WriteLine("Row Number: " + rowNumber);

        // Burrows-Wheeler Inverse
        string originalText = BurrowsWheeler.Inverse(transformedText, rowNumber);
        Console.WriteLine("Original Text: " + originalText);

        // Move-to-front Encoding
        List<int> encoded = BurrowsWheeler.MoveToFrontEncode(input);
        Console.WriteLine("Move-to-front Encoding: " + string.Join(" ", encoded));

        // Move-to-front Decoding
        string decoded = BurrowsWheeler.MoveToFrontDecode(encoded);
        Console.WriteLine("Move-to-front Decoding: " + decoded);

        // Huffman Encoding
        Dictionary<char, string> codes = BurrowsWheeler.HuffmanEncode(input);
        foreach (var pair in codes)
        {
            Console.WriteLine("Huffman Code for " + pair.Key + ": " + pair.Value);
        }
    }
}*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
                string filePath = @"Test Files\Large Cases\Large\dickens.txt";

                fileContent = File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading the file: " + e.Message);
                return;
            }

            //Console.WriteLine("OriginalText = " + fileContent);

            (string, int) transformed_text = Transform(fileContent);

            //Console.WriteLine("TransformedText = " + transformed_text.Item1);

            string inversed_text = Inverse(transformed_text.Item1, transformed_text.Item2);

            //Console.WriteLine("InversedText = " + inversed_text);

            if (inversed_text.Equals(fileContent))
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failed");
            }
            var time_after = System.Environment.TickCount;
            Console.WriteLine("Time Taken = " +  (time_after - time_before));
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

            /*Original_Indexes.Sort((a, b) =>
            {
                int length = test.Length;
                for (int i = 0; i < length; i++)
                {
                    char aChar = test[(a + i) % length];
                    char bChar = test[(b + i) % length];
                    if (aChar < bChar) return -1;
                    if (aChar > bChar) return 1;
                }
                return 0;
            });*/
            QuickSort(Original_Indexes, test, 0, Original_Indexes.Count - 1);
            //ParallelQuickSort(Original_Indexes, test, 0, Original_Indexes.Count - 1);



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
        static void QuickSort(List<int> indexes, string text, int left, int right)
        {
            //Console.WriteLine("hi3");
            if (left < right)
            {
                //Console.WriteLine("hi2");
                int partitionIndex = Partition(indexes, text, left, right);
                //QuickSort(indexes, text, left, partitionIndex - 1);
                //QuickSort(indexes, text, partitionIndex + 1, right);
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

    }
}