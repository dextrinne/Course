using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TrackerBackend.Data;
using TrackerBackend.Models;

namespace TrackerBackend.Controllers
{
    /// <summary>
    /// Контроллер для управления транзакциями (расходами)
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private const decimal DailyLimit = 1_000_000;
        private const int MaxCommentLength = 200;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает список транзакций с возможностью фильтрации
        /// </summary>
        /// <param name="date">Фильтр по конкретной дате</param>
        /// <param name="month">Фильтр по конкретному месяцу</param>
        /// <param name="year">Фильтр по текущему году</param>
        /// <returns>Список транзакций</returns>
        [HttpGet(Name = "GetTransactions")]
        public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetAll(
            [FromQuery] DateTime? date = null,
            [FromQuery] string? month = null,
            [FromQuery] int? year = null)
        {
            var query = BuildTransactionQuery();

            var filterResult = ApplyTransactionFilters(query, date, month, year);
            if (filterResult is ActionResult actionResult)
                return actionResult;

            query = filterResult.Value;

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Select(t => MapToTransactionResponse(t))
                .ToListAsync();

            return Ok(transactions);
        }

        /// <summary>
        /// Получает транзакцию по ID
        /// </summary>
        /// <param name="id">ID транзакции</param>
        /// <returns>Транзакция с указанным ID</returns>
        [HttpGet("{id}", Name = "GetTransactionById")]
        public async Task<ActionResult<TransactionResponse>> GetById(int id)
        {
            var transaction = await FindTransactionByIdAsync(id);

            if (transaction == null)
                return NotFound(new { message = $"Транзакция с id={id} не найдена" });

            return Ok(MapToTransactionResponse(transaction));
        }

        /// <summary>
        /// Получает сводку трат за конкретный день
        /// </summary>
        /// <param name="date">Дата для сводки</param>
        /// <returns>Дневная сводка (сумма, количество транзакций, список)</returns>
        [HttpGet("daily-summary", Name = "GetDailySummary")]
        public async Task<ActionResult<DailySummaryResponse>> GetDailySummary(
            [FromQuery] DateTime date)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            var dayTransactions = await _context.Transactions
                .Include(t => t.ExpenseItem)
                    .ThenInclude(e => e.Category)
                .Where(t => t.Date >= dayStart && t.Date < dayEnd)
                .Where(t => t.ExpenseItem.IsActive)
                .Where(t => t.ExpenseItem.Category.IsActive)
                .ToListAsync();

            var totalAmount = dayTransactions.Sum(t => t.Amount);

            var summary = new DailySummaryResponse
            {
                Date = date,
                TotalAmount = totalAmount,
                TransactionCount = dayTransactions.Count,
                Transactions = dayTransactions.Select(MapToTransactionResponse).ToList()
            };

            return Ok(summary);
        }

        /// <summary>
        /// Создаёт новую транзакцию
        /// </summary>
        /// <param name="request">Данные транзакции (дата, сумма, статья, комментарий)</param>
        /// <returns>Созданная транзакция</returns>
        [HttpPost(Name = "CreateTransaction")]
        public async Task<ActionResult<TransactionResponse>> Create([FromBody] TransactionRequest request)
        {
            var validationResult = await ValidateTransactionRequest(request);
            if (validationResult != null)
                return validationResult;

            var expenseItem = await _context.ExpenseItems
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == request.ExpenseItemId);

            if (expenseItem == null)
                return BadRequest(new { message = $"Статья расхода с id={request.ExpenseItemId} не найдена" });

            if (!expenseItem.IsActive)
                return BadRequest(new { message = "Нельзя создать транзакцию для неактивной статьи расхода" });

            var dailyLimitResult = await CheckDailyLimitAsync(request.Date, request.Amount);
            if (dailyLimitResult != null)
                return dailyLimitResult;

            var transaction = new Transaction
            {
                Date = request.Date.Date,
                Amount = request.Amount,
                Comment = request.Comment?.Trim(),
                ExpenseItemId = request.ExpenseItemId
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _context.Entry(transaction).Reference(t => t.ExpenseItem).LoadAsync();
            await _context.Entry(transaction.ExpenseItem).Reference(e => e.Category).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, MapToTransactionResponse(transaction));
        }

        /// <summary>
        /// Обновляет существующую транзакцию
        /// </summary>
        /// <param name="id">ID транзакции</param>
        /// <param name="request">Новые данные транзакции</param>
        /// <returns>Обновлённая транзакция</returns>
        [HttpPut("{id}", Name = "UpdateTransaction")]
        public async Task<ActionResult<TransactionResponse>> Update(int id, [FromBody] TransactionRequest request)
        {
            var transaction = await FindTransactionByIdAsync(id);

            if (transaction == null)
                return NotFound(new { message = $"Транзакция с id={id} не найдена" });

            var validationResult = await ValidateTransactionRequest(request);
            if (validationResult != null)
                return validationResult;

            if (request.ExpenseItemId != transaction.ExpenseItemId)
            {
                var changeItemResult = await ChangeExpenseItemAsync(transaction, request.ExpenseItemId);
                if (changeItemResult != null)
                    return changeItemResult;
            }

            var dailyLimitResult = await CheckDailyLimitAsync(request.Date, request.Amount, id);
            if (dailyLimitResult != null)
                return dailyLimitResult;

            transaction.Date = request.Date.Date;
            transaction.Amount = request.Amount;
            transaction.Comment = request.Comment?.Trim();

            await _context.SaveChangesAsync();

            return Ok(MapToTransactionResponse(transaction));
        }

        /// <summary>
        /// Удаляет транзакцию по ID
        /// </summary>
        /// <param name="id">ID транзакции</param>
        /// <returns>Сообщение об удалении</returns>
        [HttpDelete("{id}", Name = "DeleteTransaction")]
        public async Task<ActionResult> Delete(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
                return NotFound(new { message = $"Транзакция с id={id} не найдена" });

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Транзакция успешно удалена" });
        }

        /// <summary>
        /// Строит базовый запрос для получения транзакций с включёнными связанными данными
        /// </summary>
        private IQueryable<Transaction> BuildTransactionQuery()
        {
            return _context.Transactions
                .Include(t => t.ExpenseItem)
                    .ThenInclude(e => e.Category)
                .AsQueryable();
        }

        /// <summary>
        /// Применяет фильтры к запросу транзакций
        /// </summary>
        /// <param name="query">Исходный запрос транзакций</param>
        /// <param name="date">Фильтр по дате</param>
        /// <param name="month">Фильтр по месяцу в формате YYYY-MM</param>
        /// <param name="year">Фильтр по году</param>
        /// <returns>Отфильтрованный запрос или ошибка</returns>
        private ActionResult<IQueryable<Transaction>> ApplyTransactionFilters(
            IQueryable<Transaction> query,
            DateTime? date,
            string? month,
            int? year)
        {
            if (date.HasValue)
            {
                query = query.Where(t => t.Date.Date == date.Value.Date);
            }
            else if (!string.IsNullOrEmpty(month))
            {
                if (DateTime.TryParse(month + "-01", out var monthDate))
                {
                    var startOfMonth = new DateTime(monthDate.Year, monthDate.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                    query = query.Where(t => t.Date >= startOfMonth && t.Date <= endOfMonth);
                }
                else
                {
                    return BadRequest(new { message = "Неверный формат месяца. Используйте YYYY-MM" });
                }
            }
            else if (year.HasValue)
            {
                query = query.Where(t => t.Date.Year == year.Value);
            }

            return query;
        }

        /// <summary>
        /// Находит транзакцию по ID с включёнными связанными данными
        /// </summary>
        /// <param name="id">ID транзакции</param>
        /// <returns>Транзакция с ExpenseItem и Category</returns>
        private async Task<Transaction?> FindTransactionByIdAsync(int id)
        {
            return await _context.Transactions
                .Include(t => t.ExpenseItem)
                    .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Валидирует запрос транзакции
        /// </summary>
        /// <param name="request">Данные транзакции для проверки</param>
        /// <returns>BadRequest с описанием ошибки</returns>
        private async Task<ActionResult?> ValidateTransactionRequest(TransactionRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest(new { message = "Сумма транзакции должна быть положительной" });

            if (request.Amount > DailyLimit)
                return BadRequest(new { message = $"Сумма одной транзакции не может превышать {DailyLimit} рублей" });

            if (request.Comment != null && request.Comment.Length > MaxCommentLength)
                return BadRequest(new { message = $"Комментарий не должен превышать {MaxCommentLength} символов" });

            return null;
        }

        /// <summary>
        /// Проверяет дневной лимит трат
        /// </summary>
        /// <param name="date">Дата транзакции</param>
        /// <param name="amount">Сумма новой транзакции</param>
        /// <param name="excludeTransactionId">ID транзакции для исключения из расчёта (при обновлении)</param>
        /// <returns>BadRequest при превышении лимита</returns>
        private async Task<ActionResult?> CheckDailyLimitAsync(DateTime date, decimal amount, int? excludeTransactionId = null)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            var dailyQuery = _context.Transactions
                .Where(t => t.Date >= dayStart && t.Date < dayEnd)
                .Where(t => t.ExpenseItem.IsActive);

            if (excludeTransactionId.HasValue)
                dailyQuery = dailyQuery.Where(t => t.Id != excludeTransactionId.Value);

            var dailyTotal = await dailyQuery.SumAsync(t => t.Amount);

            if (dailyTotal + amount > DailyLimit)
            {
                var remaining = DailyLimit - dailyTotal;
                return BadRequest(new
                {
                    message = $"Превышен суточный лимит трат. Доступно для ввода: {remaining:F2} руб.",
                    dailyTotal = dailyTotal,
                    dailyLimit = DailyLimit,
                    remaining = remaining
                });
            }

            return null;
        }

        /// <summary>
        /// Изменяет статью расхода для существующей транзакции
        /// </summary>
        /// <param name="transaction">Текущая транзакция</param>
        /// <param name="newExpenseItemId">ID новой статьи расхода</param>
        /// <returns>BadRequest при ошибке или null</returns>
        private async Task<ActionResult?> ChangeExpenseItemAsync(Transaction transaction, int newExpenseItemId)
        {
            if (!transaction.ExpenseItem.IsActive)
            {
                return BadRequest(new
                {
                    message = "Нельзя изменить статью расхода, так как она стала неактивной " +
                             "после создания транзакции. Создайте новую транзакцию."
                });
            }

            var newExpenseItem = await _context.ExpenseItems.FindAsync(newExpenseItemId);
            if (newExpenseItem == null)
                return BadRequest(new { message = $"Статья расхода с id={newExpenseItemId} не найдена" });

            if (!newExpenseItem.IsActive)
                return BadRequest(new { message = "Нельзя выбрать неактивную статью расхода" });

            transaction.ExpenseItemId = newExpenseItemId;
            return null;
        }

        /// <summary>
        /// Преобразует сущность Transaction в DTO
        /// </summary>
        /// <param name="transaction">Сущность транзакции</param>
        /// <returns>DTO с данными транзакции</returns>
        private static TransactionResponse MapToTransactionResponse(Transaction transaction)
        {
            return new TransactionResponse
            {
                Id = transaction.Id,
                Date = transaction.Date,
                Amount = transaction.Amount,
                Comment = transaction.Comment,
                ExpenseItemId = transaction.ExpenseItemId,
                ExpenseItemName = transaction.ExpenseItem?.Name,
                CategoryName = transaction.ExpenseItem?.Category?.Name,
                IsExpenseItemActive = transaction.ExpenseItem?.IsActive ?? false
            };
        }
    }

    /// <summary>
    /// Модель запроса для создания/редактирования транзакции
    /// </summary>
    public class TransactionRequest
    {
        /// <summary>
        /// Дата транзакции
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Сумма расхода
        /// </summary>
        [Range(0.01, 1_000_000, ErrorMessage = "Сумма должна быть от 0.01 до 1 000 000")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Комментарий к транзакции
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// ID статьи расхода, к которой относится транзакция
        /// </summary>
        public int ExpenseItemId { get; set; }
    }

    /// <summary>
    /// Модель ответа с данными транзакции
    /// </summary>
    public class TransactionResponse
    {
        /// <summary>
        /// ID транзакции
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата транзакции
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Сумма расхода
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Комментарий
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// ID связанной статьи расхода
        /// </summary>
        public int ExpenseItemId { get; set; }

        /// <summary>
        /// Название статьи расхода
        /// </summary>
        public string? ExpenseItemName { get; set; }

        /// <summary>
        /// Название категории, к которой относится статья
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// Активна ли статья расхода на момент запроса
        /// </summary>
        public bool IsExpenseItemActive { get; set; }
    }

    /// <summary>
    /// Модель дневной сводки трат
    /// </summary>
    public class DailySummaryResponse
    {
        /// <summary>
        /// Дата сводки
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Общая сумма трат за день
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Количество транзакций за день
        /// </summary>
        public int TransactionCount { get; set; }

        /// <summary>
        /// Список транзакций за этот день
        /// </summary>
        public List<TransactionResponse> Transactions { get; set; } = new();
    }
}