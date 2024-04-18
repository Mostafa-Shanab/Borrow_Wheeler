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

            string transform_Output = "";
            int row_number = 0;

            int length_of_text = test.Length;

            List<String> Original_Suffixes = new List<string>();
            

            int [] next_array = new int[length_of_text];

            //Constructing Original Suffixes
            for (int i = 0; i < length_of_text; i++)
            {
                Original_Suffixes.Add(test.Substring(i) + test.Substring(0, i));
            }
            Console.WriteLine("\nBefore Sorting\n");
            foreach (var item in Original_Suffixes)
            {
                Console.WriteLine(item);
            }

            //Sorting Suffixes
            List<String> Sorted_Suffixes = new List<string>(Original_Suffixes);
            Sorted_Suffixes.Sort((a, b) => string.Compare(a, b));

            Console.WriteLine("\nAfter Sorting\n");
            foreach (var item in Sorted_Suffixes)
            {
                Console.WriteLine(item);
                transform_Output += item.Substring(length_of_text - 1);
            }
            for (int i = 0; i < length_of_text; i++)
            {
                if (Sorted_Suffixes[i] == test)
                {
                    row_number = i;
                    break;
                }
            }
            Console.WriteLine("\nTransform Output: " + transform_Output + "\nRow Number: " + row_number + "\n");
        }
    }
    
}