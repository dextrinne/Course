using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    /// <summary>
    /// Главный класс для работы программы
    /// </summary>
    internal class Program
    {
        static async Task Main(string[] args)
        {
            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("Работа с таблицей news.");
                Console.WriteLine("1. Использовать ADO.NET");
                Console.WriteLine("2. Использовать Entity Framework");
                Console.WriteLine("3. Выход");
                Console.Write("Выберите нужное: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await RunAdoNet();
                        break;
                    case "2":
                        await RunEntityFramework();
                        break;
                    case "3":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Неверный выбор. Нажмите любую клавишу...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Создаёт экземпляр класса AdoNet и запускает меню для работы через ADO.NET
        /// </summary>
        static async Task RunAdoNet()
        {
            var ado = new AdoNet();
            await MenuAdoNet(ado);
        }

        /// <summary>
        /// Создаёт экземпляр класса EntityFramework и запускает меню для работы через Entity Framework
        /// </summary>
        static async Task RunEntityFramework()
        {
            var ef = new EntityFramework();
            await MenuEntityFramework(ef);
        }

        /// <summary>
        /// Отображает меню CRUD-операций для ADO.NET и обрабатывает выбор пользователя
        /// </summary>
        /// <param name="ado">Объект класса AdoNet, через который выполняются операции с БД</param>
        static async Task MenuAdoNet(AdoNet ado)
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("ADO.NET");
                Console.WriteLine("1. Показать все новости");
                Console.WriteLine("2. Добавить новость");
                Console.WriteLine("3. Изменить новость");
                Console.WriteLine("4. Удалить новость");
                Console.WriteLine("5. Назад");
                Console.Write("Выберите нужное: ");
                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            var items = await ado.ReadNews();
                            foreach (var item in items)
                                Console.WriteLine(item.ToString());
                            break;
                        case "2":
                            Console.Write("ID пользователя: ");
                            int userId = int.Parse(Console.ReadLine());
                            Console.Write("Заголовок: ");
                            string title = Console.ReadLine();
                            Console.Write("Текст: ");
                            string text = Console.ReadLine();
                            await ado.CreateNews(title, text, userId);
                            Console.WriteLine("Новость добавлена");
                            break;
                        case "3":
                            Console.Write("ID новости для изменения: ");
                            int updateId = int.Parse(Console.ReadLine());
                            Console.Write("Новый заголовок: ");
                            string newTitle = Console.ReadLine();
                            Console.Write("Новый текст: ");
                            string newText = Console.ReadLine();
                            await ado.UpdateNews(updateId, newTitle, newText);
                            Console.WriteLine("Новость обновлена");
                            break;
                        case "4":
                            Console.Write("ID новости для удаления: ");
                            int deleteId = int.Parse(Console.ReadLine());
                            await ado.DeleteNews(deleteId);
                            Console.WriteLine("Новость удалена");
                            break;
                        case "5":
                            back = true;
                            continue;
                        default:
                            Console.WriteLine("Неверный выбор!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }

                if (!back)
                {
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Отображает меню CRUD-операций для Entity Framework и обрабатывает выбор пользователя
        /// </summary>
        /// <param name="ef">Объект класса Entity Framework, через который выполняются операции с БД</param>
        static async Task MenuEntityFramework(EntityFramework ef)
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("Entity Framework");
                Console.WriteLine("1. Показать все новости");
                Console.WriteLine("2. Добавить новость");
                Console.WriteLine("3. Изменить новость");
                Console.WriteLine("4. Удалить новость");
                Console.WriteLine("5. Назад");
                Console.Write("Выберите нужное: ");
                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            var items = await ef.ReadNews();
                            foreach (var item in items)
                                Console.WriteLine(item.ToString());
                            break;
                        case "2":
                            Console.Write("ID пользователя: ");
                            int userId = int.Parse(Console.ReadLine());
                            Console.Write("Заголовок: ");
                            string title = Console.ReadLine();
                            Console.Write("Текст: ");
                            string text = Console.ReadLine();
                            await ef.CreateNews(title, text, userId);
                            Console.WriteLine("Новость добавлена.");
                            break;
                        case "3":
                            Console.Write("ID новости для изменения: ");
                            int updateId = int.Parse(Console.ReadLine());
                            Console.Write("Новый заголовок: ");
                            string newTitle = Console.ReadLine();
                            Console.Write("Новый текст: ");
                            string newText = Console.ReadLine();
                            await ef.UpdateNews(updateId, newTitle, newText);
                            Console.WriteLine("Новость обновлена.");
                            break;
                        case "4":
                            Console.Write("ID новости для удаления: ");
                            int deleteId = int.Parse(Console.ReadLine());
                            await ef.DeleteNews(deleteId);
                            Console.WriteLine("Новость удалена.");
                            break;
                        case "5":
                            back = true;
                            continue;
                        default:
                            Console.WriteLine("Неверный выбор!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }

                if (!back)
                {
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }
    }
}