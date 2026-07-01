using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes1
{
    internal class Program
    {
        // сумма = нач вкл * (1 + ставка) в степени i
        static string Calculate(double initial_deposit, double interest_rate, int years)
        {
            var res = new StringBuilder();

            double rate = 1 + interest_rate / 100;

            for (int i = 1; i <= years; i++)
            {
                double sum = initial_deposit * Math.Pow(rate, i);
                res.AppendLine($"Год {i}: {sum:F2} руб.");
            }

            return res.ToString().TrimEnd();
        }

        static int GetPosInt(string a)
        {
            int value;
            while (true)
            {
                Console.Write(a);
                string input = Console.ReadLine();

                if (int.TryParse(input, out value) && value > 0)
                {
                    return value;
                }

                Console.WriteLine("Введите положительное целое число!!!");
            }
        }

        static int GetPosDouble(string a)
        {
            int value;
            while (true)
            {
                Console.Write(a);
                string input = Console.ReadLine();
                input = input.Replace('.', ',');

                if (int.TryParse(input, out value) && value > 0)
                {
                    return value;
                }

                Console.WriteLine("Введите положительное целое число!!!");
            }
        }

        static void Main(string[] args)
        {
            double initial_deposit = GetPosDouble("Начальный вклад: "),
                interest_rate = GetPosDouble("Годовая процентная ставка: ");
            int years = GetPosInt("Количество лет: ");
            string res = Calculate(initial_deposit, interest_rate, years);
            Console.WriteLine(res);
        }
    }
}
