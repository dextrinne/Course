namespace TrackerBackend.Models
{
    /// <summary>
    /// Категория расходов
    /// </summary>
    public class Category
    {
        /// <summary>
        /// ID категории
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название категории
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Месячный бюджет категории
        /// </summary>
        public decimal MonthlyBudget { get; set; }

        /// <summary>
        /// Активна ли категория
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Связанные статьи расходов
        /// </summary>
        public ICollection<ExpenseItem> ExpenseItems { get; set; } = new List<ExpenseItem>();
    }
}
