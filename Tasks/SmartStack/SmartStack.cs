using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStack
{
    /// <summary>
    /// Реализует стек
    /// </summary>
    /// <typeparam name="T">Тип элементов в стеке</typeparam>
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
        /// Проверяет стект на пустоту
        /// </summary>
        /// <exception cref="InvalidOperationException">Стек пуст</exception>
        private void EnsureNotEmpty()
        {
            if (_count == 0)
                throw new InvalidOperationException("Стек пуст");
        }

        /// <summary>
        /// Удаляет элемент с вершины стека и возвращает его
        /// </summary>
        /// <returns>Сохранённый элемент вершины</returns>
        /// <exception cref="InvalidOperationException">Проверяет пустоту стека</exception>
        public T Pop()
        {
            EnsureNotEmpty();
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
            EnsureNotEmpty();
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
        /// Проверка глубины на корректность
        /// </summary>
        /// <param name="deep">Индекс вершины (глубина стека)</param>
        /// <exception cref="ArgumentOutOfRangeException">Выход за границы диапазона</exception>
        private void ValidateDepth(int deep)
        {
            if (deep < 0 || deep >= _count)
                throw new ArgumentOutOfRangeException(nameof(deep), "Выход за границы диапазона");
        }

        /// <summary>
        /// Обеспечивает доступ к элементам стека по глубине
        /// </summary>
        /// <param name="deep">Индекс вершины (глубина стека)</param>
        public T this[int deep]
        {
            get
            {
                ValidateDepth(deep);
                return _items[_count - 1 - deep];
            }
            set
            {
                ValidateDepth(deep);
                _items[_count - 1 - deep] = value;
            }
        }

    }

}
