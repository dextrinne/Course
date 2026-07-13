using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStack
{
    public class SmartStack<T> : IEnumerable<T>
    {
        private T[] _items;
        private int _count;

        /// <summary>
        /// Создаёт пустой стек с начальной ёмкостью 4 элемента
        /// </summary>
        public SmartStack()
        {
            _items = new T[4];
            _count = 0;
        }

        /// <summary>
        /// Создаёт пустой стек с указанной пользователем ёмкостью N
        /// </summary>
        /// <param name="n">Ёмкость стека</param>
        public SmartStack(int n)
        {
            _items = new T[n];
            _count = 0;
        }

        /// <summary>
        /// Создаёт стек из коллекции, в котором последний элемент коллекции станет вершиной стека
        /// </summary>
        /// <param name="collection">Исходная коллекция</param>
        public SmartStack(IEnumerable<T> collection)
        {
            List<T> sourceList = new List<T>(collection);
            _items = new T[sourceList.Count];

            // Последний элемент коллекции становится вершиной стека
            for (int i = 0; i < sourceList.Count; i++)
            {
                _items[i] = sourceList[i];
            }
            _count = sourceList.Count;
        }

        /// <summary>
        /// Увеличивает размер внутреннего массива
        /// </summary>
        /// <param name="newN">Новый размер массива</param>
        private void Resize(int newN)
        {
            T[] newArray = new T[newN];
            Array.Copy(_items, newArray, _count);
            _items = newArray;
        }

        /// <summary>
        /// Добавляет элемент на вершину стека
        /// </summary>
        /// <param name="item">Добавляемый элемент</param>
        public void Push(T item)
        {
            if (_count == _items.Length)
            {
                Resize(_items.Length * 2);
            }
            _items[_count] = item;
            _count++;
        }

        /// <summary>
        /// Добавляет на вершину стека содержимое коллекции.
        /// Элементы добавляются в обратном порядке: последний элемент коллекции станет новой вершиной.
        /// </summary>
        /// <param name="collection">Коллекция, элементы которой добавляются в стек</param>
        public void PushRange(IEnumerable<T> collection)
        {
            int newItemsCount = 0;
            if (collection is ICollection<T> col)
            {
                newItemsCount = col.Count;
            }
            else
            {
                foreach (var item in collection)
                    newItemsCount++;
            }

            if (newItemsCount == 0) return;

            // Метод должен корректно учитывать число элементов.
            int totalCount = _count + newItemsCount;
            int newCapacity = _items.Length;
            while (newCapacity < totalCount)
                newCapacity *= 2;

            if (newCapacity != _items.Length)
                Resize(newCapacity);

            foreach (var item in collection)
                _items[_count++] = item;
        }

        /// <summary>
        /// Удаляет элемент с вершины стека и возвращает его
        /// </summary>
        /// <returns>Сохранённый элемент вершины</returns>
        /// <exception cref="InvalidOperationException">Проверяет пустоту стека</exception>
        public T Pop()
        {
            if (_count == 0)
                throw new InvalidOperationException("Стек пуст");

            _count--;
            T item = _items[_count];
            _items[_count] = default(T);
            return item;
        }

        /// <summary>
        /// Возвращает элемент с вершины стека без его удаления
        /// </summary>
        /// <returns>Сохранённый элемент вершины</returns>
        /// <exception cref="InvalidOperationException">Проверяет пустоту стека</exception>
        public T Peek()
        {
            if (_count == 0)
                throw new InvalidOperationException("Стек пуст");

            return _items[_count - 1];
        }

        /// <summary>
        /// Проверяет, содержится ли указанный элемент в стеке
        /// </summary>
        /// <param name="item">Проверяеммый элемент</param>
        /// <returns>Булевый параментр (да/нет)</returns>
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

        /// <summary>
        /// Возвращает количество элементов в стеке
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Возвращает ёмкость стека
        /// </summary>
        public int Capacity => _items.Length;

        /// <summary>
        /// Реализует интерфейс IEnumerable<T>, обеспечивая 
        /// обход элементов стека от вершины к основанию
        /// </summary>
        /// <returns>Каждый элемент возвращается по требованию перечислителя</returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = _count - 1; i >= 0; i--)
            {
                yield return _items[i];
            }
        }

        /// <summary>
        /// Явная реализация интерфейса IEnumerable для поддержки неуниверсального перечисления
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Обеспечивает доступ к элементам стека по глубине
        /// </summary>
        /// <param name="deep">Индекс вершины (глубина стека)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
