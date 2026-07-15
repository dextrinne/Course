using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rectangle
{
    /// <summary>
    /// Класс для представления прямоугольника
    /// </summary>
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
            set => SetValue(ref _x, value, "x");
        }

        /// <summary>
        /// Получение или изменение координаты Y левого верхнего угла
        /// </summary>
        public int Y
        {
            get => _y;
            set => SetValue(ref _y, value, "y");
        }

        /// <summary>
        /// Получение или изменение ширины прямоугольника
        /// </summary>
        public int Width
        {
            get => _width;
            set => SetValue(ref _width, value, "ширины");
        }

        /// <summary>
        /// Получение или изменение высоты прямоугольника
        /// </summary>
        public int Height
        {
            get => _height;
            set => SetValue(ref _height, value, "высоты");
        }

        /// <summary>
        /// Вспомогательный метод для проверки и установки значений
        /// </summary>
        /// <param name="field">Ссылка на поле, которое нужно изменить</param>
        /// <param name="value">Новое значение для установки</param>
        /// <param name="propertyName">Имя свойства (используется в сообщении об ошибке)</param>
        /// <exception cref="ArgumentException">Выбрасывается, если значение отрицательное</exception>
        private void SetValue(ref int field, int value, string propertyName)
        {
            if (value < 0)
                throw new ArgumentException($"Отрицательное значение {propertyName}!!!");
            field = value;
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
}