using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rectangle
{
    /// <summary>
    /// Главный класс для работы программы
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Прямоугольник с параметрами (10, 20, 30, 40):");
            Rectangle rectangle = new Rectangle(10, 20, 30, 40);
            rectangle.PrintRectangle();

            Console.WriteLine("\nИзменение ширины на 50 и высоты на 60:");
            rectangle.Width = 50;
            rectangle.Height = 60;
            rectangle.PrintRectangle();

            Console.WriteLine("\nИзменение координат на (100, 200):");
            rectangle.X = 100;
            rectangle.Y = 200;
            rectangle.PrintRectangle();

            Console.WriteLine("\nПопытка создания прямоугольника с отрицательными значениями:");
            try
            {
                Rectangle rectangleError = new Rectangle(-10, -20, -30, -40);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
    }
}