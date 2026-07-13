using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class AdoNet
    {
        private readonly string _connectionString;

        /// <summary>
        /// Использует прямое подключение к SQL Server
        /// </summary>
        public AdoNet()
        {
            _connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=DB_try;Integrated Security=True;";
        }

        /// <summary>
        /// Создаёт новую запись в таблице news
        /// </summary>
        /// <param name="title">Заголовок новости</param>
        /// <param name="text">Текст новости</param>
        /// <param name="userId">ID автора</param>
        public async Task CreateNews(string title, string text, int userId)
        {
            const string checkUserSql = "SELECT COUNT(1) FROM users WHERE user_id = @UserId";
            const string insertNewsSql = @"INSERT INTO news (user_id, news_title, news_text, publication_date)
                                           VALUES (@UserId, @Title, @Text, GETDATE())";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // существует ли пользователь
                        using (var check = new SqlCommand(checkUserSql, connection, transaction))
                        {
                            check.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                            int count = (int)await check.ExecuteScalarAsync();
                            if (count == 0)
                                throw new InvalidOperationException($"Пользователь с ID {userId} не существует!!!");
                        }

                        // вставка
                        using (var insert = new SqlCommand(insertNewsSql, connection, transaction))
                        {
                            insert.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                            insert.Parameters.Add("@Title", SqlDbType.NVarChar, 255).Value = title;
                            insert.Parameters.Add("@Text", SqlDbType.NVarChar, -1).Value = text ?? (object)DBNull.Value;

                            await insert.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Получает все новости из таблицы news
        /// </summary>
        /// <returns>Список новостей</returns>
        public async Task<List<NewsItem>> ReadNews()
        {
            var newsList = new List<NewsItem>();
            const string query = "SELECT news_id, user_id, news_title, news_text, publication_date FROM news ORDER BY publication_date DESC";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                await connection.OpenAsync();
                using (var read = await command.ExecuteReaderAsync())
                {
                    while (await read.ReadAsync())
                    {
                        newsList.Add(new NewsItem
                        {
                            NewsId = read.GetInt32(0),
                            UserId = read.GetInt32(1),
                            Title = read.GetString(2),
                            Text = read.IsDBNull(3) ? null : read.GetString(3),
                            PublicationDate = read.GetDateTime(4)
                        });
                    }
                }
            }
            return newsList;
        }

        /// <summary>
        /// Обновляет заголовок и текст существующей новости по её ID
        /// </summary>
        /// <param name="newsId">ID новости</param>
        /// <param name="newTitle">Новый заголовок новости</param>
        /// <param name="newText">Новый текст новости</param>
        /// <exception cref="Exception">Выбрасывается, если новость с указанным newsId не найдена в БД</exception>
        public async Task UpdateNews(int newsId, string newTitle, string newText)
        {
            const string query = "UPDATE news SET news_title = @Title, news_text = @Text WHERE news_id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@Title", SqlDbType.NVarChar, 255).Value = newTitle;
                command.Parameters.Add("@Text", SqlDbType.NVarChar, -1).Value = newText ?? (object)DBNull.Value;
                command.Parameters.Add("@Id", SqlDbType.Int).Value = newsId;

                await connection.OpenAsync();
                int row = await command.ExecuteNonQueryAsync();
                if (row == 0)
                    throw new Exception("Новость с указанным ID не найдена!!!");
            }
        }

        /// <summary>
        /// Удаляет новость из таблицы news
        /// </summary>
        /// <param name="newsId">ID новости</param>
        /// <exception cref="Exception">Выбрасывается при ошибках подключения к БД или нарушениях внешних ключей</exception>
        public async Task DeleteNews(int newsId)
        {
            const string query = "DELETE FROM news WHERE news_id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = newsId;
                await connection.OpenAsync();
                int row = await command.ExecuteNonQueryAsync();
                if (row == 0)
                    throw new Exception("Новость с указанным ID не найдена!!!");
            }
        }
    }

    /// <summary>
    /// Модель данных для представления новости из таблицы news
    /// </summary>
    public class NewsItem
    {
        public int NewsId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime PublicationDate { get; set; }

        public override string ToString()
        {
            return $"ID: {NewsId} | {PublicationDate:dd.MM.yyyy HH:mm} | {Title}";
        }
    }
}
