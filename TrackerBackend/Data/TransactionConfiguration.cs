using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackerBackend.Models;

namespace TrackerBackend.Data
{
    /// <summary>
    /// Конфигурация сущности Transaction для Entity Framework
    /// </summary>
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        /// <summary>
        /// Настраивает схему таблицы Transaction
        /// </summary>
        /// <param name="builder">Построитель конфигурации сущности</param>
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transaction", "fin"); // finance

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("TransactionID");

            builder.Property(t => t.Date)
                .IsRequired()
                .HasColumnType("date") // без времени
                .HasColumnName("TransactionDate");

            builder.Property(t => t.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("Amount")
                .HasDefaultValue(0);

            builder.Property(t => t.Comment)
                .HasMaxLength(200)
                .HasColumnName("Comment");

            builder.HasOne(t => t.ExpenseItem)
                .WithMany(e => e.Transactions)
                .HasForeignKey(t => t.ExpenseItemId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Transactions_ExpenseItems");

            builder.HasIndex(e => e.Date)
                .HasDatabaseName("IX_Transactions_Date");

            builder.HasIndex(t => new { t.Date, t.ExpenseItemId })
                .HasDatabaseName("IX_Transactions_Date_ExpenseItem");
        }
    }
}
