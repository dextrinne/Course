using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TrackerBackend.Models;

namespace TrackerBackend.Data
{
    /// <summary>
    /// Контекст базы данных приложения
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Инициализирует новый экземпляр контекста базы данных с заданными параметрами
        /// </summary>
        /// <param name="options">Параметры контекста базы данных</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Категории расходов (название, месячный бюджет, статус активности)
        /// </summary>
        public DbSet<Category> Categories => Set<Category>();

        /// <summary>
        /// Статьи расходов (название, категория, статус активности)
        /// </summary>
        public DbSet<ExpenseItem> ExpenseItems => Set<ExpenseItem>();

        /// <summary>
        /// Транзакции (дата, сумма, комментарий, статья расхода)
        /// </summary>
        public DbSet<Transaction> Transactions => Set<Transaction>();

        /// <summary>
        /// Настройка модели базы данных. 
        /// Автоматически применяет все конфигурации сущностей (IEntityTypeConfiguration) из текущей сборки
        /// </summary>
        /// <param name="modelBuilder">Построитель модели для настройки сущностей и связей</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.HasPostgresExtension("citext");
            base.OnModelCreating(modelBuilder);
        }
    }
}
