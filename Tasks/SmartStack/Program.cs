using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStack
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SmartStack<int> stack = new SmartStack<int>();
            Console.WriteLine($"Стек создан. Count = {stack.Count}, Capacity = {stack.Capacity} \n");

            stack.Push(10);
            stack.Push(20);
            stack.Push(30);
            stack.Push(40);
            stack.Push(50);
            Console.WriteLine($"После 5 пушей: Count = {stack.Count}, Capacity = {stack.Capacity}\n");

            Console.WriteLine($"Вершина: {stack.Peek()} \n");

            Console.WriteLine($"Pop: {stack.Pop()}");
            Console.WriteLine($"После Pop: Count = {stack.Count}, Capacity = {stack.Capacity}\n");

            Console.WriteLine($"Contains 30: {stack.Contains(30)}");
            Console.WriteLine($"Contains 100: {stack.Contains(100)}\n");

            Console.WriteLine($"Верщина по индексатору = {stack[0]}");
            Console.WriteLine($"Осование по индексатору = {stack[stack.Count - 1]}\n");

            int[] i = { 1, 2, 3, 4, 5 };
            SmartStack<int> collection = new SmartStack<int>(i);
            Console.WriteLine("\nСтек из коллекции [1,2,3,4,5]:");
            foreach (int item in collection)
                Console.Write(item + " ");
            Console.WriteLine();

            int[] range = { 10, 20, 30 };
            collection.PushRange(range);
            Console.WriteLine("\nПосле PushRange [10,20,30]:");
            foreach (int item in collection)
                Console.Write(item + " ");
            Console.WriteLine();

            SmartStack<string> s = new SmartStack<string>(10);
            Console.WriteLine($"\nЁмкость 10: Capacity = {s.Capacity}");

            try
            {
                SmartStack<double> empty = new SmartStack<double>();
                empty.Pop();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"\nИсключение при Pop из пустого стека: {ex.Message}");
            }

            try
            {
                SmartStack<int> alot = new SmartStack<int>();
                alot.Push(5);
                int value = alot[1];
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"\nИсключение индексатора: {ex.Message}");
            }
        }
    }
}
