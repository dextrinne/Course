using Microsoft.AspNetCore.Mvc;
using TrackerBackend.Data;
using TrackerBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace TrackerBackend.Controllers
{
    /// <summary>
    /// Контроллер для управления статьями расходов
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ExpenseItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExpenseItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает список всех статей расходов
        /// </summary>
        /// <param name="categoryId">ID категории</param>
        /// <param name="includeInactive">Если true, возвращает также неактивные статьи</param>
        /// <returns>Список статей расходов с информацией о категории</returns>
        [HttpGet(Name = "GetExpenseItems")]
        public async Task<ActionResult<IEnumerable<ExpenseItem>>> GetAll(
            [FromQuery] int? categoryId = null,
            [FromQuery] bool includeInactive = false)
        {
            var query = _context.ExpenseItems
                .Include(e => e.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(e => e.CategoryId == categoryId.Value);

            if (!includeInactive)
                query = query.Where(e => e.IsActive);

            var items = await query
                .AsNoTracking()
                .OrderBy(e => e.Name)
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// Получает статью расхода по идентификатору
        /// </summary>
        /// <param name="id">ID статьи расхода</param>
        /// <returns>Статья расхода с информацией о категории</returns>
        [HttpGet("{id}", Name = "GetExpenseItemById")]
        public async Task<ActionResult<ExpenseItem>> GetById(int id)
        {
            var item = await _context.ExpenseItems
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (item == null)
                return NotFound(new { message = $"Статья расхода с id={id} не найдена" });

            return Ok(item);
        }

        /// <summary>
        /// Создаёт новую статью расхода
        /// </summary>
        /// <param name="dto">Данные статьи (название, ID категории, активность)</param>
        /// <returns>Созданная статья расхода</returns>
        [HttpPost(Name = "CreateExpenseItem")]
        public async Task<ActionResult<ExpenseItem>> Create([FromBody] ExpenseItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Название статьи расхода обязательно" });

            if (dto.Name.Length > 100)
                return BadRequest(new { message = "Название статьи не должно превышать 100 символов" });

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
                return BadRequest(new { message = $"Категория с id={dto.CategoryId} не найдена" });

            var exists = await _context.ExpenseItems
                .AnyAsync(e => e.Name.ToLower() == dto.Name.ToLower() && e.CategoryId == dto.CategoryId);

            if (exists)
                return BadRequest(new { message = "Статья с таким названием уже существует в данной категории" });

            var item = new ExpenseItem
            {
                Name = dto.Name.Trim(),
                CategoryId = dto.CategoryId,
                IsActive = dto.IsActive
            };

            _context.ExpenseItems.Add(item);
            await _context.SaveChangesAsync();

            await _context.Entry(item).Reference(e => e.Category).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        /// <summary>
        /// Обновляет существующую статью расхода
        /// </summary>
        /// <param name="id">ID статьи</param>
        /// <param name="dto">Новые данные статьи</param>
        /// <returns>Обновлённая статья</returns>
        [HttpPut("{id}", Name = "UpdateExpenseItem")]
        public async Task<ActionResult<ExpenseItem>> Update(int id, [FromBody] ExpenseItemDto dto)
        {
            var item = await _context.ExpenseItems
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (item == null)
                return NotFound(new { message = $"Статья расхода с id={id} не найдена" });

            if (!item.IsActive && !dto.IsActive)
                return BadRequest(new { message = "Нельзя редактировать неактивную статью расхода. Сначала активируйте её." });

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Название статьи расхода обязательно" });

            if (dto.Name.Length > 100)
                return BadRequest(new { message = "Название статьи не должно превышать 100 символов" });

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
                return BadRequest(new { message = $"Категория с id={dto.CategoryId} не найдена" });

            var exists = await _context.ExpenseItems
                .AnyAsync(e => e.Name.ToLower() == dto.Name.ToLower()
                            && e.CategoryId == dto.CategoryId
                            && e.Id != id);

            if (exists)
                return BadRequest(new { message = "Статья с таким названием уже существует в данной категории" });

            item.Name = dto.Name.Trim();
            item.CategoryId = dto.CategoryId;
            item.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(item);
        }

        /// <summary>
        /// Удаляет статью расхода по идентификатору
        /// </summary>
        /// <param name="id">ID статьи</param>
        /// <returns>Сообщение об удалении</returns>
        [HttpDelete("{id}", Name = "DeleteExpenseItem")]
        public async Task<ActionResult> Delete(int id)
        {
            var item = await _context.ExpenseItems
                .Include(e => e.Transactions)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (item == null)
                return NotFound(new { message = $"Статья расхода с id={id} не найдена" });

            if (!item.IsActive)
                return BadRequest(new { message = "Нельзя удалить неактивную статью расхода. Сначала активируйте её." });

            if (item.Transactions.Any())
                return BadRequest(new
                {
                    message = "Невозможно удалить статью расхода, так как с ней связаны транзакции. " +
                             "Сначала удалите связанные транзакции."
                });

            _context.ExpenseItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Статья расхода успешно удалена" });
        }
    }

    /// <summary>
    /// Модель данных для создания/редактирования статьи расходов (DTO — Data Transfer Object) 
    /// </summary>
    public class ExpenseItemDto
    {
        /// <summary>
        /// Название статьи расхода
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// ID категории, к которой относится статья
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// Активна ли статья
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
