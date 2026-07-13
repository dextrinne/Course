using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompoundInterest
{
    internal class Program
    {
        /// <summary>
        /// Вычисляет накопления по сложным процентам по годам
        /// </summary>
        /// <param name="initialDeposit">Начальный вклад</param>
        /// <param name="interestRate">Годовая процентная ставка</param>
        /// <param name="years">Количество лет</param>
        /// <returns></returns>
        public static Dictionary<int, double> CalculateCompoundInterest(double initialDeposit, double interestRate, int years)
        {
            var result = new Dictionary<int, double>();
            double rate = 1 + interestRate / 100;

            for (var i = 1; i <= years; i++)
            {
                result[i] = initialDeposit * Math.Pow(rate, i);
            }

            return result;
        }

        /// <summary>
        /// Проверяет является ли введённое вещественное число положительным
        /// </summary>
        /// <param name="request">Строка подсказки для пользователя 
        /// (Что пользователь вводит в консоль)</param>
        /// <returns></returns>
        private static double ReadPositiveDouble(string request)
        {
            while (true)
            {
                Console.Write(request);
                string input = Console.ReadLine()?.Replace('.', ',');

                if (double.TryParse(input, out double value) && value > 0)
                {
                    return value;
                }

                Console.WriteLine("Введите положительное число.");
            }
        }

        /// <summary>
        /// Проверяет является ли введённое целое число положительным
        /// </summary>
        /// <param name="request">Строка подсказки для пользователя (Что пользователь вводит в консоль)</param>
        /// <returns></returns>
        private static int ReadPositiveInt(string request)
        {
            while (true)
            {
                Console.Write(request);
                string input = Console.ReadLine();

                if (int.TryParse(input, out int value) && value > 0)
                {
                    return value;
                }

                Console.WriteLine("Введите положительное целое число.");
            }
        }

        static void Main(string[] args)
        {
            double initialDeposit = ReadPositiveDouble("Начальный вклад: ");
            double interestRate = ReadPositiveDouble("Годовая процентная ставка: ");
            int years = ReadPositiveInt("Количество лет: ");

            Dictionary<int, double> money = CalculateCompoundInterest(initialDeposit, interestRate, years);

            var output = new StringBuilder();

            foreach (var entry in money)
            {
                output.AppendLine($"Год {entry.Key}: {entry.Value:F2} руб.");
            }

            Console.WriteLine(output.ToString().TrimEnd());

        }
    }
}
