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
            var query = _context.Transactions
                .Include(t => t.ExpenseItem)
                    .ThenInclude(e => e.Category)
                .AsQueryable();

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

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Select(t => new TransactionResponse
                {
                    Id = t.Id,
                    Date = t.Date,
                    Amount = t.Amount,
                    Comment = t.Comment,
                    ExpenseItemId = t.ExpenseItemId,
                    ExpenseItemName = t.ExpenseItem.Name,
                    CategoryName = t.ExpenseItem.Category.Name,
                    IsExpenseItemActive = t.ExpenseItem.IsActive
                })
                .ToListAsync();

            return Ok(transactions);
        }

        /// <summary>
        /// Получает транзакцию по идентификатору
        /// </summary>
        /// <param name="id">ID транзакции</param>
        /// <returns>Транзакция с указанным ID</returns>
        [HttpGet("{id}", Name = "GetTransactionById")]
        public async Task<ActionResult<TransactionResponse>> GetById(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.ExpenseItem)
                    .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
                return NotFound(new { message = $"Транзакция с id={id} не найдена" });

            var response = new TransactionResponse
            {
                Id = transaction.Id,
                Date = transaction.Date,
                Amount = transaction.Amount,
                Comment = transaction.Comment,
                ExpenseItemId = transaction.ExpenseItemId,
                ExpenseItemName = transaction.ExpenseItem.Name,
                CategoryName = transaction.ExpenseItem.Category.Name,
                IsExpenseItemActive = transaction.ExpenseItem.IsActive
            };

            return Ok(response);
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
                Transactions = dayTransactions.Select(t => new TransactionResponse
                {
                    Id = t.Id,
                    Date = t.Date,
                    Amount = t.Amount,
                    Comment = t.Comment,
                    ExpenseItemId = t.ExpenseItemId,
                    ExpenseItemName = t.ExpenseItem?.Name,
                    CategoryName = t.ExpenseItem?.Category?.Name,
                    IsExpenseItemActive = t.ExpenseItem?.IsActive ?? false
                }).ToList()
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
            if (request.Amount <= 0)
                return BadRequest(new { message = "Сумма транзакции должна быть положительной" });

            if (request.Amount > 1_000_000)
                return BadRequest(new { message = "Сумма одной транзакции не может превышать 1 000 000 рублей" });

            if (request.Comment != null && request.Comment.Length > 200)
                return BadRequest(new { message = "Комментарий не должен превышать 200 символов" });

            var expenseItem = await _context.ExpenseItems
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == request.ExpenseItemId);

            if (expenseItem == null)
                return BadRequest(new { message = $"Статья расхода с id={request.ExpenseItemId} не найдена" });

            if (!expenseItem.IsActive)
                return BadRequest(new { message = "Нельзя создать транзакцию для неактивной статьи расхода" });

            var dayStart = request.Date.Date;
            var dayEnd = dayStart.AddDays(1);

            var dailyTotal = await _context.Transactions
                .Where(t => t.Date >= dayStart && t.Date < dayEnd)
                .Where(t => t.ExpenseItem.IsActive)
                .SumAsync(t => t.Amount);

            if (dailyTotal + request.Amount > 1_000_000)
            {
                var remaining = 1_000_000 - dailyTotal;
                return BadRequest(new
                {
                    message = $"Превышен суточный лимит трат. Доступно для ввода: {remaining:F2} руб.",
                    dailyTotal = dailyTotal,
                    dailyLimit = 1_000_000,
                    remaining = remaining
                });
            }

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

            var response = new TransactionResponse
            {
                Id = transaction.Id,
                Date = transaction.Date,
                Amount = transaction.Amount,
                Comment = transaction.Comment,
                ExpenseItemId = transaction.ExpenseItemId,
                ExpenseItemName = transaction.ExpenseItem.Name,
                CategoryName = transaction.ExpenseItem.Category.Name,
                IsExpenseItemActive = transaction.ExpenseItem.IsActive
            };

            return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, response);
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
            var transaction = await _context.Transactions
                .Include(t => t.ExpenseItem)
                    .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
                return NotFound(new { message = $"Транзакция с id={id} не найдена" });

            if (request.Amount <= 0)
                return BadRequest(new { message = "Сумма транзакции должна быть положительной" });

            if (request.Amount > 1_000_000)
                return BadRequest(new { message = "Сумма одной транзакции не может превышать 1 000 000 рублей" });

            if (request.Comment != null && request.Comment.Length > 200)
                return BadRequest(new { message = "Комментарий не должен превышать 200 символов" });

            if (request.ExpenseItemId != transaction.ExpenseItemId)
            {
                if (!transaction.ExpenseItem.IsActive)
                {
                    return BadRequest(new
                    {
                        message = "Нельзя изменить статью расхода, так как она стала неактивной " +
                                 "после создания транзакции. Создайте новую транзакцию."
                    });
                }

                var newExpenseItem = await _context.ExpenseItems.FindAsync(request.ExpenseItemId);
                if (newExpenseItem == null)
                    return BadRequest(new { message = $"Статья расхода с id={request.ExpenseItemId} не найдена" });

                if (!newExpenseItem.IsActive)
                    return BadRequest(new { message = "Нельзя выбрать неактивную статью расхода" });

                transaction.ExpenseItemId = request.ExpenseItemId;
            }

            var dayStart = request.Date.Date;
            var dayEnd = dayStart.AddDays(1);

            // проверка дневного лимита при редактировании
            var dailyTotalWithoutCurrent = await _context.Transactions
                .Where(t => t.Date >= dayStart && t.Date < dayEnd && t.Id != id)
                .Where(t => t.ExpenseItem.IsActive)
                .SumAsync(t => t.Amount);

            if (dailyTotalWithoutCurrent + request.Amount > 1_000_000)
            {
                var remaining = 1_000_000 - dailyTotalWithoutCurrent;
                return BadRequest(new
                {
                    message = $"Превышен суточный лимит трат. Доступно для ввода: {remaining:F2} руб.",
                    dailyTotal = dailyTotalWithoutCurrent,
                    dailyLimit = 1_000_000,
                    remaining = remaining
                });
            }

            transaction.Date = request.Date.Date;
            transaction.Amount = request.Amount;
            transaction.Comment = request.Comment?.Trim();

            await _context.SaveChangesAsync();

            var response = new TransactionResponse
            {
                Id = transaction.Id,
                Date = transaction.Date,
                Amount = transaction.Amount,
                Comment = transaction.Comment,
                ExpenseItemId = transaction.ExpenseItemId,
                ExpenseItemName = transaction.ExpenseItem.Name,
                CategoryName = transaction.ExpenseItem.Category.Name,
                IsExpenseItemActive = transaction.ExpenseItem.IsActive
            };

            return Ok(response);
        }

        /// <summary>
        /// Удаляет транзакцию по идентификатору
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
        /// Идентификатор транзакции
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
