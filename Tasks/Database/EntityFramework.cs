using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;


namespace Database
{
    internal class EntityFramework
    {
        /// <summary>
        /// Создаёт новую запись в таблице news
        /// </summary>
        /// <param name="title">Заголовок новости</param>
        /// <param name="text">Текст новости</param>
        /// <param name="userId">ID автора</param>
        /// <exception cref="InvalidOperationException"></exception>
        public Task CreateNews(string title, string text, int userId)
        {
            using (var context = new DB_tryEntities())
            {
                var user = context.users.Find(userId);
                if (user == null)
                    throw new InvalidOperationException($"Пользователь с ID {userId} не существует!!!");

                var news = new news
                {
                    user_id = userId,
                    news_title = title,
                    news_text = text,
                    publication_date = DateTime.Now
                };
                context.news.Add(news);
                context.SaveChanges();
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Получает все новости из таблицы news
        /// </summary>
        /// <returns>Список новостей</returns>
        public Task<List<NewsViewModel>> ReadNews()
        {
            using (var context = new DB_tryEntities())
            {
                var list = context.news
                    .Include("users")
                    .OrderByDescending(n => n.publication_date)
                    .Select(n => new NewsViewModel
                    {
                        NewsId = n.news_id,
                        UserId = n.user_id,
                        AuthorName = n.users.fio,
                        Title = n.news_title,
                        Text = n.news_text,
                        PublicationDate = n.publication_date
                    })
                    .ToList();

                return Task.FromResult(list);
            }
        }

        /// <summary>
        /// Обновляет заголовок и текст существующей новости по её ID
        /// </summary>
        /// <param name="newsId">ID новости</param>
        /// <param name="newTitle">Новый заголовок новости</param>
        /// <param name="newText">Новый текст новости</param>
        /// <exception cref="Exception">Выбрасывается, если новость с указанным newsId не найдена в БД</exception>
        public Task UpdateNews(int newsId, string newTitle, string newText)
        {
            using (var context = new DB_tryEntities())
            {
                var news = context.news.Find(newsId);
                if (news == null)
                    throw new Exception("Новость не найдена!!!");

                news.news_title = newTitle;
                news.news_text = newText;
                context.SaveChanges();
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Удаляет новость из таблицы news
        /// </summary>
        /// <param name="newsId">ID новости</param>
        /// <exception cref="Exception">Выбрасывается при ошибках подключения к БД или нарушениях внешних ключей</exception>
        public Task DeleteNews(int newsId)
        {
            using (var context = new DB_tryEntities())
            {
                var news = context.news.Find(newsId);
                if (news == null)
                    throw new Exception("Новость не найдена!!!");

                context.news.Remove(news);
                context.SaveChanges();
                return Task.CompletedTask;
            }
        }
    }

    /// <summary>
    /// Модель данных для представления новости из таблицы news
    /// </summary>
    public class NewsViewModel
    {
        public int NewsId { get; set; }
        public int UserId { get; set; }
        public string AuthorName { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime PublicationDate { get; set; }

        public override string ToString()
        {
            return $"ID: {NewsId} | {PublicationDate:dd.MM.yyyy HH:mm} | [{AuthorName}] {Title}";
        }
    }
}
