using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurrowsWheeler
{
    internal class Huffman
    {
        public class HuffmanNode
        {
            public char Character { get; set; }
            public int Frequency { get; set; }
            public HuffmanNode Left { get; set; }
            public HuffmanNode Right { get; set; }
        }

        public class HuffmanCoding
        {

            private static Dictionary<char, int> BuildFrequencyTable(string input) // return Dictionary with Char as the Key / Frequency as the Value  
            {
                var result = new Dictionary<char, int>();
                foreach (char c in input)
                {
                    if (!result.ContainsKey(c))
                    {
                        result[c] = 0;
                    }
                    result[c]++;
                }
                return result;
            }

            private static HuffmanNode BuildHuffmanTree(Dictionary<char, int> frequencyTable)  // return the Root of the Huffman tree
            {
                var priorityQueue = new PriorityQueue<HuffmanNode, int/*PriorityValue*/>();
                foreach (var pair in frequencyTable)
                {
                    priorityQueue.Enqueue(new HuffmanNode { Character = pair.Key, Frequency = pair.Value }, pair.Value);
                }

                while (priorityQueue.Count > 1)
                {
                    var left = priorityQueue.Dequeue();
                    var right = priorityQueue.Dequeue();
                    var sum = left.Frequency + right.Frequency;
                    var node = new HuffmanNode { Left = left, Right = right, Frequency = sum };
                    priorityQueue.Enqueue(node, sum);
                }

                return priorityQueue.Dequeue(); // Root of the tree
            }

            private static Dictionary<char, string> GenerateCodes(HuffmanNode node, string code = "")
            {
                var codes = new Dictionary<char, string>();
                if (node.Left == null && node.Right == null) // Leaf node
                {
                    codes[node.Character] = code;
                    return codes;
                }

                if (node.Left != null)
                {
                    var leftCodes = GenerateCodes(node.Left, code + "0");
                    foreach (var pair in leftCodes)
                    {
                        codes.Add(pair.Key, pair.Value);
                    }
                }

                if (node.Right != null)
                {
                    var rightCodes = GenerateCodes(node.Right, code + "1");
                    foreach (var pair in rightCodes)
                    {
                        codes.Add(pair.Key, pair.Value);
                    }
                }

                return codes;
            }

            public static void MMain()
            {
                string input = "your input string";
                var frequencyTable = BuildFrequencyTable(input);
                var root = BuildHuffmanTree(frequencyTable);
                var huffmanCodes = GenerateCodes(root);

                // Display the codes
                foreach (var pair in huffmanCodes)
                {
                    Console.WriteLine($"Character: {pair.Key}, Code: {pair.Value}");
                }
            }
        }
    }
}
