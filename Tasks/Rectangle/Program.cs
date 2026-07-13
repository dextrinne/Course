using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rectangle
{
    public class Rectangle
    {
        private int _x, _y, _width, _height;

        /// <summary>
        /// Создает новый объект прямоугольника с заданными параметрами
        /// </summary>
        /// <param name="x">Координата X верхнего левого угла</param>
        /// <param name="y">Координата Y верхнего левого угла</param>
        /// <param name="width">Ширина прямоугольника</param>
        /// <param name="height">Высота прямоугольника</param>
        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Получение или изменение координаты X левого верхнего угла
        /// </summary>
        public int X
        {
            get => _x;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Отрицательное значение x!!!");
                _x = value;
            }
        }

        /// <summary>
        /// Получение или изменение координаты Y левого верхнего угла
        /// </summary>
        public int Y
        {
            get => _y;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Отрицательное значение y!!!");
                _y = value;
            }
        }

        /// <summary>
        /// Получение или изменение ширины прямоугольника
        /// </summary>
        public int Width
        {
            get => _width;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Отрицательное значение ширины!!!");
                _width = value;
            }
        }

        /// <summary>
        /// Получение или изменение высоты прямоугольника
        /// </summary>
        public int Height
        {
            get => _height;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Отрицательное значение высоты!!!");
                _height = value;
            }
        }

        /// <summary>
        /// Вычисляет периметр прямоугольника
        /// </summary>
        /// <returns>Периметр прямоугольника</returns>
        public int Perimeter => 2 * (_width + _height);
        /// <summary>
        /// Вычисляет площадь прямоугольника
        /// </summary>
        /// <returns>Площадь прямоугольника</returns>
        public int Area => _width * _height;

        /// <summary>
        /// Вычисляет координаты правого верхнего угла прямоугольника
        /// </summary>
        /// <returns>Кортеж (X, Y) с координатами правого верхнего угла</returns>
        public (int X, int Y) TopRight() => (_x + _width, _y);
        /// <summary>
        /// Вычисляет координаты левого нижнего угла прямоугольника
        /// </summary>
        /// <returns>Кортеж (X, Y) с координатами левого нижнего угла</returns>
        public (int X, int Y) BottomLeft() => (_x, _y + _height);
        /// <summary>
        /// Вычисляет координаты правого нижнего угла прямоугольника
        /// </summary>
        /// <returns>Кортеж (X, Y) с координатами правого нижнего угла</returns>
        public (int X, int Y) BottomRight() => (_x + _width, _y + _height);

        /// <summary>
        /// Выводит в консоль полную информацию о прямоугольнике
        /// </summary>
        public void PrintRectangle()
        {
            Console.WriteLine($"Ширина: {Width}");
            Console.WriteLine($"Высота: {Height}");
            Console.WriteLine($"Периметр: {Perimeter}");
            Console.WriteLine($"Площадь: {Area}");

            var (trX, trY) = TopRight();
            var (blX, blY) = BottomLeft();
            var (brX, brY) = BottomRight();

            Console.WriteLine($"Левый верхний угол: ({X}, {Y})");
            Console.WriteLine($"Правый верхний угол: ({trX}, {trY})");
            Console.WriteLine($"Левый нижний угол: ({blX}, {blY})");
            Console.WriteLine($"Правый нижний угол: ({brX}, {brY})\n");
        }
    }

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