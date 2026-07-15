using Microsoft.AspNetCore.Mvc;
using TrackerBackend.Data;
using TrackerBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace TrackerBackend.Controllers
{
    /// <summary>
    /// Контроллер для управления категориями расходов
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает список всех категорий
        /// </summary>
        /// <param name="includeInactive">Если true, возвращает также неактивные категории</param>
        /// <returns>Список категорий</returns>
        [HttpGet(Name = "GetCategories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetAll(
            [FromQuery] bool includeInactive = false)
        {
            var query = _context.Categories.AsQueryable();

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            var categories = await query
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Получает категорию по ID
        /// </summary>
        /// <param name="id">ID категории</param>
        /// <returns>Категория с указанным ID</returns>
        [HttpGet("{id}", Name = "GetCategoryById")]
        public async Task<ActionResult<Category>> GetById(int id)
        {
            var category = await FindCategoryByIdWithItemsAsync(id);

            if (category == null)
                return NotFound(new { message = $"Категория с id={id} не найдена" });

            return Ok(category);
        }

        /// <summary>
        /// Создаёт новую категорию расходов
        /// </summary>
        /// <param name="dto">Данные новой категории (название, бюджет, активность)</param>
        /// <returns>Созданная категория</returns>
        [HttpPost(Name = "CreateCategory")]
        public async Task<ActionResult<Category>> Create([FromBody] CategoryDto dto)
        {
            var validationResult = await ValidateCategoryDtoAsync(dto);
            if (validationResult != null)
                return validationResult;

            var exists = await CheckCategoryNameExistsAsync(dto.Name);
            if (exists)
                return BadRequest(new { message = "Категория с таким названием уже существует" });

            var category = new Category
            {
                Name = dto.Name.Trim(),
                MonthlyBudget = dto.MonthlyBudget,
                IsActive = dto.IsActive
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        /// <summary>
        /// Обновляет существующую категорию
        /// </summary>
        /// <param name="id">ID категории</param>
        /// <param name="dto">Новые данные категории (название, бюджет, активность)</param>
        /// <returns>Обновлённая категория</returns>
        [HttpPut("{id}", Name = "UpdateCategory")]
        public async Task<ActionResult<Category>> Update(int id, [FromBody] CategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound(new { message = $"Категория с id={id} не найдена" });

            if (!category.IsActive && !dto.IsActive)
                return BadRequest(new { message = "Нельзя редактировать неактивную категорию. Сначала активируйте её." });

            var validationResult = await ValidateCategoryDtoAsync(dto);
            if (validationResult != null)
                return validationResult;

            var exists = await CheckCategoryNameExistsAsync(dto.Name, id);
            if (exists)
                return BadRequest(new { message = "Категория с таким названием уже существует" });

            category.Name = dto.Name.Trim();
            category.MonthlyBudget = dto.MonthlyBudget;
            category.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(category);
        }

        /// <summary>
        /// Удаляет категорию по ID
        /// </summary>
        /// <param name="id">ID категории</param>
        /// <returns>Сообщение об удалении</returns>
        [HttpDelete("{id}", Name = "DeleteCategory")]
        public async Task<ActionResult> Delete(int id)
        {
            var category = await FindCategoryByIdWithItemsAsync(id);

            if (category == null)
                return NotFound(new { message = $"Категория с id={id} не найдена" });

            if (!category.IsActive)
                return BadRequest(new { message = "Нельзя удалить неактивную категорию. Сначала активируйте её." });

            if (category.ExpenseItems.Any())
                return BadRequest(new
                {
                    message = "Невозможно удалить категорию, так как с ней связаны статьи расходов. " +
                             "Сначала удалите или переместите связанные статьи."
                });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Категория успешно удалена" });
        }

        /// <summary>
        /// Находит категорию по ID вместе со связанными статьями расходов
        /// </summary>
        /// <param name="id">ID категории</param>
        /// <returns>Категория с включёнными статьями расходов или null, если не найдена</returns>
        private async Task<Category?> FindCategoryByIdWithItemsAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.ExpenseItems)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Проверяет, существует ли категория с указанным названием
        /// </summary>
        /// <param name="name">Название категории</param>
        /// <param name="excludeId">ID категории, которую нужно исключить из проверки (например, при обновлении)</param>
        /// <returns>true, если категория с таким названием уже существует - иначе false</returns>
        private async Task<bool> CheckCategoryNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Categories
                .Where(c => c.Name == name);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        /// <summary>
        /// Валидирует данные категории на корректность
        /// </summary>
        /// <param name="dto">Объект с данными категории для валидации</param>
        /// <returns>BadRequest с описанием ошибки, если данные некорректны - null, если данные валидны</returns>
        private async Task<ActionResult?> ValidateCategoryDtoAsync(CategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Название категории обязательно" });

            if (dto.Name.Length > 100)
                return BadRequest(new { message = "Название категории не должно превышать 100 символов" });

            if (dto.MonthlyBudget < 0)
                return BadRequest(new { message = "Месячный бюджет не может быть отрицательным" });

            return await Task.FromResult<ActionResult?>(null);
        }
    }

    /// <summary>
    /// Модель данных для создания/редактирования категории (DTO — Data Transfer Object) 
    /// </summary>
    public class CategoryDto
    {
        /// <summary>
        /// Название категории
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Месячный бюджет
        /// </summary>
        public decimal MonthlyBudget { get; set; }
        /// <summary>
        /// Активна ли категория
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}