using System;
using System.Collections.Generic;
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
            string test = "abfacadabfa";

            long char_numbers = test.Length;

            string[] suffixes = new string[char_numbers];

            for (int i = 0; i < char_numbers; i++)
            {
                suffixes[i] = test.Substring(i) + test.Substring(0, i);
            }

            foreach (var item in suffixes)
            {
                Console.WriteLine(item);
            }
        }
    }
    
}

/*using System;
using System.Collections.Generic;
using System.Linq;

class BurrowsWheelerTransform
{
    class Suffix
    {
        public string Text { get; }
        public int Index { get; }

        public Suffix(string text, int index)
        {
            Text = text;
            Index = index;
        }
    }

    public static string Transform(string text)
    {
        var suffixes = GenerateSuffixes(text);
        suffixes.Sort((a, b) => string.Compare(a.Text, b.Text));

        int originalIndex = -1;
        for (int i = 0; i < suffixes.Count; i++)
        {
            if (suffixes[i].Index == 0)
            {
                originalIndex = i;
                break;
            }
        }

        return string.Concat(suffixes.Select(s => s.Text[s.Text.Length - 1])) + " " + originalIndex;
    }

    private static List<Suffix> GenerateSuffixes(string text)
    {
        var suffixes = new List<Suffix>();
        for (int i = 0; i < text.Length; i++)
        {
            suffixes.Add(new Suffix(text.Substring(i) + text.Substring(0, i), i));
        }
        return suffixes;
    }

    static void Main()
    {
        string text = "abfacadabfa";
        string transformed = Transform(text);
        Console.WriteLine("Burrows-Wheeler Transform: " + transformed);
    }
}*/
