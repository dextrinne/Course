namespace TrackerBackend.Models
{
    /// <summary>
    /// Статья расхода
    /// </summary>
    public class ExpenseItem
    {
        /// <summary>
        /// ID статьи
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название статьи расхода
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Активна ли статья
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ID связанной категории
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Связанная категория
        /// </summary>
        public Category Category { get; set; } = null!;

        /// <summary>
        /// Связанные транзакции
        /// </summary>
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
