using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diamond
{
    internal class Program
    {
        /// <summary>
        /// Создаёт двумерный массив символов, представляющий ромб из символов 'X'
        /// с пустым центром. Длина каждой диагонали равна n.
        /// </summary>
        /// <param name="n">Положительное нечётное число (больше 3)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Проверка правильно ли введено число</exception>
        static char[,] CreateDiamond(int n)
        {
            if (n <= 3)
                throw new ArgumentException("n должно быть положительным числом (от 3)", nameof(n));

            if (n % 2 == 0)
                throw new ArgumentException("n должно быть нечётным числом", nameof(n));

            char[,] diamond = new char[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    diamond[i, j] = ' ';

            int center = n / 2;

            // заполняем ромб
            for (int row = 0; row < n; row++)
            {
                for (int colm = 0; colm < n; colm++)
                {
                    // манхэттенское расстояние от центра
                    int distance = Math.Abs(row - center) + Math.Abs(colm - center);

                    if (distance == center)
                    {
                        diamond[row, colm] = 'X';
                    }
                }
            }

            return diamond;
        }

        /// <summary>
        /// Выводит ромб на консоль.
        /// </summary>
        /// <param name="diamond">Массив с данными ромба</param>
        static void PrintDiamond(char[,] diamond)
        {
            int size = diamond.GetLength(0);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Console.Write(diamond[i, j]);
                }
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Console.Write("Введите нечётное положительное число больше 3 (длина диагонали): ");
                string input = Console.ReadLine();

                int n = int.Parse(input);
                char[,] diamond = CreateDiamond(n);

                Console.WriteLine($"\nРомб размером {n}x{n}:\n");
                PrintDiamond(diamond);
                Console.WriteLine();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}
