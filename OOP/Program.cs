using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP
{
    internal class Program
    {
        // левый верхний угол, ширина, высота, периметр, площадь
        // отрицательные значения недопустимы
        public class Rectangle
        {
            private int x, y, wight, height;

            public Rectangle(int x, int y, int wight, int height)
            {
                List<string> error = new List<string>();

                if (x < 0) error.Add($"Отрицательное значение x!!!");
                if (y < 0) error.Add($"Отрицательное значение y!!!");
                if (wight < 0) error.Add($"Отрицательное значение ширины!!!");
                if (height < 0) error.Add($"Отрицательное значение высоты!!!");

                if (error.Count > 0)
                    throw new ArgumentException(string.Join("\n", error));

                this.x = x;
                this.y = y;
                this.wight = wight;
                this.height = height;
            }

            public int X
            {
                get { return x; }
                set { x = value; }
            }

            public int Y
            {
                get { return y; }
                set { y = value; }
            }

            public int Wight
            {
                get { return wight; }
                set { wight = value; }
            }

            public int Height
            {
                get { return height; }
                set { height = value; }
            }

            public int Perimeter
            {
                get { return 2 * (wight + height); }
            }

            public int Square
            {
                get { return wight * height; }
            }

            public (int X, int Y) TopRight()
            {
                return (x + wight, y);
            }

            public (int X, int Y) BottomLeft()
            {
                return (x, y + height);
            }

            public (int X, int Y) BottomRight()
            {
                return (x + wight, y + height);
            }

            public void Print()
            {
                Console.WriteLine($"Ширина: {Wight}");
                Console.WriteLine($"Высота: {Height}");
                Console.WriteLine($"Периметр: {Perimeter}");
                Console.WriteLine($"Площадь: {Square}");

                var topRight = TopRight();
                var bottomLeft = BottomLeft();
                var bottomRight = BottomRight();

                Console.WriteLine($"Левый верхний угол: ({x}, {y})");
                Console.WriteLine($"Правый верхний угол: ({topRight.X}, {topRight.Y})");
                Console.WriteLine($"Левый нижний угол: ({bottomLeft.X}, {bottomLeft.Y})");
                Console.WriteLine($"Правый нижний угол: ({bottomRight.X}, {bottomRight.Y})\n");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Прямоугольник с параметрами (10, 20, 30, 40):");
            Rectangle rect1 = new Rectangle(10, 20, 30, 40);
            rect1.Print();

            Console.WriteLine("\nИзменение ширины на 50 и высоты на 60:");
            rect1.Wight = 50;
            rect1.Height = 60;
            rect1.Print();

            Console.WriteLine("\nИзменение координат на (100, 200):");
            rect1.X = 100;
            rect1.Y = 200;
            rect1.Print();

            Console.WriteLine("\nПопытка создания прямоугольника с отрицательными значениями:");
            try
            {
                Rectangle rect2 = new Rectangle(-10, -20, -30, -40);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            
        }
    }
}
