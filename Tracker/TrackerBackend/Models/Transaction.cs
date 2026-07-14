namespace TrackerBackend.Models
{
    /// <summary>
    /// Транзакция расхода
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// ID транзакции
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата транзакции
        /// </summary>
        public DateTime Date { get; set; } // без времени

        /// <summary>
        /// Сумма расхода
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Комментарий к транзакции
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// ID связанной статьи расхода
        /// </summary>
        public int ExpenseItemId { get; set; }

        /// <summary>
        /// Связанная статья расхода
        /// </summary>
        public ExpenseItem ExpenseItem { get; set; } = null!;
    }
}
