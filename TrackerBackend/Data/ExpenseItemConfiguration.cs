using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackerBackend.Models;

namespace TrackerBackend.Data
{
    /// <summary>
    /// Конфигурация сущности ExpenseItem для Entity Framework
    /// </summary>
    public class ExpenseItemConfiguration : IEntityTypeConfiguration<ExpenseItem>
    {
        /// <summary>
        /// Настраивает схему таблицы ExpenseItem
        /// </summary>
        /// <param name="builder">Построитель конфигурации сущности</param>
        public void Configure(EntityTypeBuilder<ExpenseItem> builder)
        {
            builder.ToTable("ExpenseItems", "fin"); // finance

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ExpenseItemID");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("citext")
                .HasColumnName("ItemName");

            builder.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive");

            builder.HasOne(e => e.Category)
                .WithMany(c => c.ExpenseItems)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ExpenseItems_Categories");

            builder.HasIndex(e => e.Name)
                .HasDatabaseName("IX_ExpenseItems_Name");
        }
    }
}
