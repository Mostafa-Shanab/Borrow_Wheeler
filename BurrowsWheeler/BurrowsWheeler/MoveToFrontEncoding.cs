using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurrowsWheeler
{
    internal class MoveToFrontEncoding
    {
        public static List<char> ASCII()
        {
            List<char> ascii = new List<char>(); //put it to use for decode & encode 
            for (int i = 0; i < 256; i++)
            {
                ascii.Add((char)i);
            }
            return ascii;

        }
        public static List<int> Encode(string input)
        {
            List<char> ASCII_List = ASCII();

            List<int> encoded = new List<int>();

            foreach (char symbol in input)
            {
                int index = ASCII_List.IndexOf(symbol);

                encoded.Add(index);

                ASCII_List.RemoveAt(index);
                ASCII_List.Insert(0, symbol);
            }

            return encoded;
        }

        public static string Decode(List<int> encoded)
        {
            int size = encoded.Count;

            List<char> ASCII_List = new List<char>();

            char[] decoded = new char[size];

            for (int i = 0; i < size; i++)
            {
                char symbol = ASCII_List[encoded[i]];

                decoded[i] = symbol;

                ASCII_List.RemoveAt(encoded[i]);
                ASCII_List.Insert(0, symbol);
            }

            return new string(decoded);
        }

        public static void MMain()
        {
            string input = "abbbaaabbbbaacccaabbaaaabc";
            List<int> encoded = MoveToFrontEncoding.Encode(input);
            Console.WriteLine("Encoded:");
            foreach (int index in encoded)
            {
                Console.Write(index + " ");
            }
            Console.WriteLine();

            string decoded = MoveToFrontEncoding.Decode(encoded);
            Console.WriteLine("Decoded:");
            Console.WriteLine(decoded);
        }

    }
}
