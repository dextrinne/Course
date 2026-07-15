using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackerBackend.Models;

namespace TrackerBackend.Data
{
    /// <summary>
    /// Конфигурация сущности Category для Entity Framework
    /// </summary>
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        /// <summary>
        /// Настраивает схему таблицы Categories
        /// </summary>
        /// <param name="builder">Построитель конфигурации сущности</param>
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories", "fin"); // finance

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("CategoryID");

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("citext")
                .HasColumnName("CategoryName");

            builder.Property(c => c.MonthlyBudget)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .HasColumnName("MonthlyBudget");

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");

            builder.HasMany(c => c.ExpenseItems)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("IX_Categories_Name");
        }
    }
}
