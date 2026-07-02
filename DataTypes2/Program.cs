using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes2
{
    internal class Program
    {
        static void Romb(int n)
        {
            int mid = n / 2;

            // симметрия относительно d
            for (int i = 0; i < n; i++)
            {
                int d = Math.Abs(i - mid);
                int left = d;
                int right = n - 1 - d;

                //char[] row = new char[n];
                //for (int j = 0; j < n; j++)
                //{
                //    row[j] = ' ';
                //}

                //row[left] = 'X';
                //if (left != right)
                //{
                //    row[right] = 'X';
                //}
                //Console.WriteLine(new string(row));

                for (int j = 0; j < n; j++)
                {
                    if ((j == left || j == right))
                        Console.Write('Х');
                    else
                        Console.Write(' ');
                }
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Длина диагонали: ");
            int n = int.Parse(Console.ReadLine());
            if (n % 2 == 0 || n <= 0)
            {
                Console.WriteLine("Нужно положительное нечетное число!!!!");
                return;
            }
            Romb(n);
        }
    }
}
