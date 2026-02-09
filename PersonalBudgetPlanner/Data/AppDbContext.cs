using Microsoft.EntityFrameworkCore;
using PersonalBudgetPlanner.Models;

namespace PersonalBudgetPlanner.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Absence> Absences { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public AppDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var folder = Environment.SpecialFolder.MyDocuments;
                var path = Environment.GetFolderPath(folder);
                var dbPath = System.IO.Path.Join(path, "BudgetPlanner.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId);


            modelBuilder.Entity<Category>().HasData(
                    // --- EXPENSES (UTGIFTER) ---
                    new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Mat", Type = TransactionType.Expense },
                    new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111112"), Name = "Hus & drift", Type = TransactionType.Expense },
                    new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111113"), Name = "Transport", Type = TransactionType.Expense },
                    new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111114"), Name = "Fritid", Type = TransactionType.Expense },
                    new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111115"), Name = "Barn", Type = TransactionType.Expense },
                    new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111116"), Name = "Streaming-tjänster", Type = TransactionType.Expense },
                    new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111117"), Name = "SaaS-produkter", Type = TransactionType.Expense },

                    // Bra tillägg för utgifter
                    new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111118"), Name = "Hälsa & Sjukvård", Type = TransactionType.Expense },
                    new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111119"), Name = "Skulder & Räntor", Type = TransactionType.Expense },
                    new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111120"), Name = "Övrigt (Utgift)", Type = TransactionType.Expense },

                    // --- INCOMES (INKOMSTER) ---
                    new Category { Id = Guid.Parse("22222222-2222-2222-2222-222222222221"), Name = "Lön", Type = TransactionType.Income },
                    new Category { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Bidrag", Type = TransactionType.Income },
                    new Category { Id = Guid.Parse("22222222-2222-2222-2222-222222222223"), Name = "Hobbyverksamhet", Type = TransactionType.Income },

                    // Bra tillägg för inkomster
                    new Category { Id = Guid.Parse("22222222-2222-2222-2222-222222222224"), Name = "Återbäring/Bonus", Type = TransactionType.Income },
                    new Category { Id = Guid.Parse("22222222-2222-2222-2222-222222222225"), Name = "Gåvor", Type = TransactionType.Income },
                    new Category { Id = Guid.Parse("22222222-2222-2222-2222-222222222226"), Name = "Övrigt (Inkomst)", Type = TransactionType.Income }
                );


            modelBuilder.Entity<UserProfile>().HasData(
                new UserProfile
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Standard User",
                    AnnualIncome = 0m
                }
                );
        }
    }
}
