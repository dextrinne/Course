using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collections
{
    public class SmartStack<T> : IEnumerable<T>
    {
        private T[] _items;
        private int _count;

        // создаётся массив ёмкостью 4 элемента
        public SmartStack()
        {
            _items = new T[4];
            _count = 0;
        }

        // создаётся массив указанной ёмкости
        public SmartStack(int n)
        {
            _items = new T[n];
            _count = 0;
        }

        // реализация IEnumerable<T>, создаёт массив (последний элемент коллекции будет вершиной стека)
        public SmartStack(IEnumerable<T> collection)
        {
            List<T> A = new List<T>(collection);
            _items = new T[A.Count];
            
            for (int i = 0; i < A.Count; i++)
            {
                _items[i] = A[i];
            }
            _count = A.Count;
        }

        // Метод Push, добавляющий элемент на вершину стека. При нехватке места, ёмкость удваиваться
        private void Resize(int newN)
        {
            T[] newArray = new T[newN];
            Array.Copy(_items, newArray, _count);
            _items = newArray;
        }
        public void Push(T collection)
        {
            if (_count == _items.Length)
            {
                Resize(_items.Length * 2);
            }
            _items[_count] = collection;
            _count++;
        }

        // Метод PushRange, добавляющий на вершину содержимое, реализующей IEnumerable<T>
        public void PushRange(IEnumerable<T> collection)
        {
            int fCount = 0;
            if (collection is ICollection<T> col)
            {
                fCount = col.Count;
            }
            else
            {
                foreach (var item in collection)
                    fCount++;
            }

            if (fCount == 0) return;

            // Метод должен корректно учитывать число элементов.
            int sCount = _count + fCount;
            int nCount = _items.Length;
            while (nCount < sCount)
                nCount *= 2;

            if (nCount != _items.Length)
                Resize(nCount);

            //T[] A = new T[fCount];
            //int index = 0;
            //foreach (var item in collection)
            //    A[index++] = item;

            //for (int i = fCount - 1; i >= 0; i--)
            //    _items[_count++] = A[i];

            foreach (var item in collection)
                _items[_count++] = item;
        }

        // Метод Pop, удаляющий и возвращающий элемент с вершины стека.Метод должен генерировать исключение InvalidOperationException, если стек пуст
        public T Pop()
        {
            if (_count == 0)
                throw new InvalidOperationException("Стек пуст");

            _count--;
            T item = _items[_count];
            _items[_count] = default(T);
            return item;
        }

        // Метод Peek, возвращающий элемент с вершины стека без его удаления. Должен генерировать исключение InvalidOperationException, если стек пуст.
        public T Peek()
        {
            if (_count == 0)
                throw new InvalidOperationException("Стек пуст");

            return _items[_count - 1];
        }

        // Метод Contains, проверяющий наличие элемента в стеке. Метод должен возвращать true, если элемент найден и false в противном случае
        public bool Contains(T item)
        {
            EqualityComparer<T> compare = EqualityComparer<T>.Default;
            for (int i = 0; i < _count; i++)
            {
                if (compare.Equals(_items[i], item))
                    return true;
            }
            return false;
        }

        // Свойство Count — получение количества элементов в стеке
        public int Count => _count;

        // Свойство Capacity — получение ёмкости: длины внутреннего массива
        public int Capacity => _items.Length;

        // Методы, реализующие интерфейсы IEnumerable и IEnumerable<T> (обход должен быть от вершины стека к основанию)
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = _count - 1; i>=0; i--)
            {
                yield return _items[i]; // Ключевое слово yield для отложенного выполнения
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        // реализовать индексатор (0 - вершина стека, Count-1 - основание). При выходе за границы должно генерироваться исключение ArgumentOutOfRangeException
        public T this[int deep]
        {
            get
            {
                if (deep < 0 || deep >= _count)
                    throw new ArgumentOutOfRangeException(null, "Выход за границы диапозона");
                return _items[_count - 1 - deep];
            }
            set
            {
                if (deep < 0 || deep >= _count)
                    throw new ArgumentOutOfRangeException(null, "Выход за границы диапозона");
                _items[_count - 1 - deep] = value;
            }
        }

    }

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
