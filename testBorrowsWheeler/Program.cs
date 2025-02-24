﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Net.Mime.MediaTypeNames;

namespace Borrow_Wheeler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Console.WriteLine("Enter \n1 to Compress\n2 to DeCompress:\n");
            string userInput = Console.ReadLine();
            int number = int.Parse(userInput);

            if (number == 1)
            {
                #region WritingToBin
                string fileContent = "";

                try
                {
                    // Path to the file
                    string filePath = @"Tests\dickens.txt";

                    fileContent = File.ReadAllText(filePath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error reading the file: " + e.Message);
                    return;
                }

                int n = fileContent.Length;
                int numofchunks = n / 200000;

                if (numofchunks == 0)
                {
                    numofchunks = 1;
                }

                int chunkSize = (n / numofchunks) + 1;

                for (int i = 0; i < n; i += chunkSize)
                {
                    string chunk = fileContent.Substring(i, Math.Min(chunkSize, n - i));
                    // Compress the chunk
                    ((Node, string), int) outputbinaryFile = fileCompress(chunk);
                    (Node, string) tree = outputbinaryFile.Item1;
                    Node root = tree.Item1;
                    string compressedText = tree.Item2;
                    int rowNumber = outputbinaryFile.Item2;

                    // Encode the compressed text as Base64 before writing to the file



                   byte[] compressedBytes = Encoding.UTF8.GetBytes(compressedText);

                    // Apply gzip compression to the chunk
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                        {
                            gzipStream.Write(compressedBytes, 0, compressedBytes.Length);
                        }
                        byte[] compressedData = memoryStream.ToArray();
                        string base64EncodedText = Convert.ToBase64String(compressedData);

                        // Write encoding, compressed text, and row number to the file
                        Dictionary<char, string> encoding = HuffmanEncoder1.Encode(root);
                        StringBuilder encodingString = new StringBuilder();
                        foreach (var kvp in encoding)
                        {
                            encodingString.Append($"{kvp.Key}:{kvp.Value},");
                        }
                        //Console.WriteLine("rowNumber : " + rowNumber);
                        using (BinaryWriter writer = new BinaryWriter(File.Open(@"Output\data.bin", FileMode.Append)))
                        {
                            writer.Write(encodingString.ToString());
                            writer.Write(base64EncodedText);
                            writer.Write(rowNumber);
                        }
                    }

                    //Console.WriteLine("Processed chunk " + (i / chunkSize + 1) + " of " + (n / chunkSize + 1));
                }
                #endregion
            }

            else if (number == 2)
            {
                #region ReadingFromBin
                var time_before = System.Environment.TickCount;
                int i = 1;
                string origString = "";
                using (BinaryReader reader = new BinaryReader(File.Open(@"Output\data.bin", FileMode.Open)))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        // Read the encoding dictionary
                        string encodingText = reader.ReadString();
                        Dictionary<char, string> encoding = new Dictionary<char, string>();
                        foreach (var pair in encodingText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var parts = pair.Split(':');
                            encoding[parts[0][0]] = parts[1];
                        }

                        // Read the compressed text as Base64 string
                        string compressedBase64 = reader.ReadString();
                        byte[] compressedBytes = Convert.FromBase64String(compressedBase64);

                        // Decompress the chunk using gzip
                        using (MemoryStream memoryStream = new MemoryStream(compressedBytes))
                        {
                            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                            {
                                using (StreamReader streamReader = new StreamReader(gzipStream))
                                {
                                    string compressedText = streamReader.ReadToEnd();

                                    // Read the row number
                                    int rowNumber = reader.ReadInt32();

                                    // Reconstruct the Huffman tree from the encoding dictionary
                                    Node reconstructedRoot = ReconstructTree(encoding);

                                    // Decompress the text
                                    string decompressedText = DecompressChunk(Encoding.UTF8.GetBytes(compressedText), reconstructedRoot);

                                    List<int> hufdecList = decompressedText.Split(' ').Select(int.Parse).ToList();
                                    string decString = Decode(hufdecList);
                                    string inversed_text = Inverse(decString, rowNumber);
                                    // Append the decompressed text to the original string
                                    origString += inversed_text;
                                }
                            }
                        }

                        Console.WriteLine(i);
                        i++;
                    }
                }
                using (StreamWriter writer = new StreamWriter(@"Output\text.txt"))
                {
                    writer.Write(origString);
                }
                #endregion
                /*if (origString.Equals(fileContent))
                {
                    Console.WriteLine("Success");
                }
                else
                {
                    Console.WriteLine("Failed");
                }*/
            }

            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            Console.WriteLine("Time Taken in MilliSeconds = " + elapsedMilliseconds);
            Console.WriteLine("Time Taken in Seconds = " + (elapsedMilliseconds / 1000));


        }
        static string RunLengthEncode(string input)
        {
            StringBuilder encoded = new StringBuilder();
            int count = 1;
            char currentChar = input[0];

            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] == currentChar)
                {
                    count++;
                }
                else
                {
                    encoded.Append(currentChar);
                    encoded.Append(count);
                    count = 1;
                    currentChar = input[i];
                }
            }

            // Append the last character and its count
            encoded.Append(currentChar);
            encoded.Append(count);

            return encoded.ToString();
        }
        static string DecompressChunk(byte[] data, Node rootNode)
        {
            // Decompress the chunk using Huffman decoding
            string decompressedText = Decompress(rootNode, Encoding.UTF8.GetString(data));

            return decompressedText;
        }



        static Node ReconstructTree(Dictionary<char, string> encoding)
        {
            Node root = new Node('\0', 0); // Dummy root node
            foreach (var kvp in encoding)
            {
                Node current = root;
                foreach (char bit in kvp.Value)
                {
                    if (bit == '0')
                    {
                        if (current.Left == null)
                        {
                            current.Left = new Node('\0', 0);
                        }
                        current = current.Left;
                    }
                    else if (bit == '1')
                    {
                        if (current.Right == null)
                        {
                            current.Right = new Node('\0', 0);
                        }
                        current = current.Right;
                    }
                }
                current.Data = kvp.Key;
            }
            return root;
        }
        static ((Node, string), int) fileCompress(string test)
        {
            (string, int) transformed_text = Transform(test);
            int rowNumber = transformed_text.Item2;
            List<int> encoded_List = Encode(transformed_text.Item1);
            string encListSrt = string.Join(" ", encoded_List);
            (Node, string) hufenc = HuffmanEncoder(encListSrt);

            return (hufenc, rowNumber);
        }
        static string fileDeCompress(((Node, string), int) input)
        {
            (Node, string) tree = input.Item1;
            int rowNumber = input.Item2;


            string hufdec = Decompress(tree.Item1, tree.Item2);

            List<int> hufdecList = hufdec.Split(' ').Select(int.Parse).ToList();
            string decString = Decode(hufdecList);
            string inversed_text = Inverse(decString, rowNumber);

            return inversed_text;
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
        static void QuickSort(List<int> indexes, string text, int left, int right)
        {
            if (left < right)
            {
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
        public class alphabetClass
        {
            public char value;
            public int cout;
            public string path;

        }

        public class Node
        {
            public char Data { get; set; }
            public int Frequency { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }
            public Node Parent { get; set; }

            public Node(char data, int freq)
            {
                Data = data;
                Frequency = freq;
                Left = null;
                Right = null;
                Parent = null;
            }

        }
        public static class HuffmanEncoder1
        {
            public static Dictionary<char, string> Encode(Node root)
            {
                Dictionary<char, string> encoding = new Dictionary<char, string>();
                EncodeHelper(root, "", encoding);
                return encoding;
            }

            private static void EncodeHelper(Node node, string path, Dictionary<char, string> encoding)
            {
                if (node == null)
                {
                    return;
                }

                if (node.Left == null && node.Right == null)
                {
                    encoding[node.Data] = path;
                    return;
                }

                EncodeHelper(node.Left, path + "0", encoding);
                EncodeHelper(node.Right, path + "1", encoding);
            }
        }


        public class FrequencyComparer : IComparer<Node>
        {
            public int Compare(Node x, Node y)
            {

                return x.Frequency.CompareTo(y.Frequency);
            }
        }



        public class PriorityQueue<T>
        {
            private List<Tuple<T, int>> heap = new List<Tuple<T, int>>();

            // Enqueue method adds an item with its priority to the priority queue
            public void Enqueue(T item, int priority)
            {
                heap.Add(Tuple.Create(item, priority));
                HeapifyUp(heap.Count - 1);
            }

            // Dequeue method removes and returns the item with the lowest priority
            public T Dequeue()
            {
                if (IsEmpty())
                    throw new InvalidOperationException("Priority queue is empty");

                T item = heap[0].Item1;
                heap[0] = heap[heap.Count - 1];
                heap.RemoveAt(heap.Count - 1);
                HeapifyDown(0);
                return item;
            }

            // Peek method returns the item with the lowest priority without removing it
            public T Peek()
            {
                if (IsEmpty())
                    throw new InvalidOperationException("Priority queue is empty");

                return heap[0].Item1;
            }

            // Checks if the priority queue is empty
            public bool IsEmpty()
            {
                return heap.Count == 0;
            }

            // Returns the number of elements in the priority queue
            public int Count => heap.Count;

            // Restores the heap property by moving the element up
            private void HeapifyUp(int index)
            {
                while (index > 0)
                {
                    int parentIndex = (index - 1) / 2;
                    if (heap[index].Item2 < heap[parentIndex].Item2)
                    {
                        Swap(index, parentIndex);
                        index = parentIndex;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Restores the heap property by moving the element down
            private void HeapifyDown(int index)
            {
                int leftChildIndex = 2 * index + 1;
                int rightChildIndex = 2 * index + 2;
                int smallestIndex = index;

                if (leftChildIndex < heap.Count && heap[leftChildIndex].Item2 < heap[smallestIndex].Item2)
                {
                    smallestIndex = leftChildIndex;
                }

                if (rightChildIndex < heap.Count && heap[rightChildIndex].Item2 < heap[smallestIndex].Item2)
                {
                    smallestIndex = rightChildIndex;
                }

                if (smallestIndex != index)
                {
                    Swap(index, smallestIndex);
                    HeapifyDown(smallestIndex);
                }
            }

            // Helper method to swap two elements in the heap
            private void Swap(int index1, int index2)
            {
                Tuple<T, int> temp = heap[index1];
                heap[index1] = heap[index2];
                heap[index2] = temp;
            }
        }

        static Dictionary<char, alphabetClass> Chars = new Dictionary<char, alphabetClass>();
        public static (Node, string) HuffmanEncoder(string t)
        {
            PriorityQueue<Node> PQueue = new PriorityQueue<Node>();
            StringBuilder HuffmanencodedTxt = new StringBuilder();
            foreach (char c in t)
            {
                if (!Chars.ContainsKey(c))
                {
                    Chars[c] = new alphabetClass();
                    Chars[c].cout = 0;
                    Chars[c].value = c;
                }

                Chars[c].cout += 1;

            }
            foreach (var C in Chars)
            {
                PQueue.Enqueue(new Node(C.Key, C.Value.cout), C.Value.cout);

            }


            while (PQueue.Count > 1)
            {
                Node x = PQueue.Dequeue();
                Node y = PQueue.Dequeue();
                Node parent = new Node('\0', x.Frequency + y.Frequency);

                parent.Left = x;
                parent.Right = y;
                x.Parent = parent;
                y.Parent = parent;

                PQueue.Enqueue(parent, parent.Frequency);
            }

            Node root = PQueue.Peek();
            GenerateCodes(root);

            foreach (char c in t)
            {
                HuffmanencodedTxt.Append(Chars[c].path);
            }

            return (root, HuffmanencodedTxt.ToString());

        }
        public static void GenerateCodes(Node node, string code = "")
        {
            if (node == null)
                return;

            if (node.Left == null && node.Right == null)
            {
                Chars[node.Data].path = code;
                return;
            }

            GenerateCodes(node.Left, code + "0");
            GenerateCodes(node.Right, code + "1");
        }

        static byte[] ConvertBinaryStringToBytes(string binaryString)
        {
            int length = binaryString.Length;
            int byteCount = (length + 7) / 8; // Calculate the number of bytes required

            // Create an array to store the bytes
            byte[] bytes = new byte[byteCount];

            // Loop through each byte
            for (int i = 0; i < byteCount; i++)
            {
                int startIndex = i * 8;
                int endIndex = Math.Min(startIndex + 8, length);
                string byteString = binaryString.Substring(startIndex, endIndex - startIndex).PadRight(8, '0');
                bytes[i] = Convert.ToByte(byteString, 2);
            }

            return bytes;
        }

        public static string Decompress(Node root, string input)
        {
            StringBuilder decodedString = new StringBuilder();
            Node current = root;

            foreach (char bit in input)
            {
                if (bit == '0')
                    current = current.Left;
                else
                    current = current.Right;

                if (current.Left == null && current.Right == null)
                {
                    decodedString.Append(current.Data);
                    current = root;
                }
            }

            return decodedString.ToString();
        }



    }
}