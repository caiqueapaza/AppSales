using APISales.Domain.Customers;
using APISales.Domain.Employees;
using APISales.Domain.Products;
using APISales.Domain.Sales;
using APISales.Domain.ServiceItens;
using APISales.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace APISales.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ServiceItem> ServiceItens { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItens { get; set; }
        public DbSet<SaleEntryItem> SaleEntryItems { get; set; }
        public DbSet<SaleEntryItemService> SaleEntryItemServices { get; set; }
        public DbSet<SalePayment> SalePayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee)
                .WithMany(e => e.Users)
                .HasForeignKey(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmployeeId)
                .IsUnique();

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.SellerEmployee)
                .WithMany()
                .HasForeignKey(s => s.SellerEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.Items)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany()
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.ServiceItem)
                .WithMany()
                .HasForeignKey(si => si.ServiceItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.ExecutorEmployee)
                .WithMany()
                .HasForeignKey(si => si.ExecutorEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleEntryItem>()
                .HasOne(ei => ei.Sale)
                .WithMany(s => s.EntryItems)
                .HasForeignKey(ei => ei.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleEntryItem>()
                .HasOne(ei => ei.Category)
                .WithMany()
                .HasForeignKey(ei => ei.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleEntryItemService>()
                .HasOne(es => es.SaleEntryItem)
                .WithMany(ei => ei.Services)
                .HasForeignKey(es => es.SaleEntryItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleEntryItemService>()
                .HasOne(es => es.ServiceItem)
                .WithMany()
                .HasForeignKey(es => es.ServiceItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleEntryItemService>()
                .HasOne(es => es.ExecutorEmployee)
                .WithMany()
                .HasForeignKey(es => es.ExecutorEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleEntryItemService>()
                .HasOne(es => es.DeliveredByEmployee)
                .WithMany()
                .HasForeignKey(es => es.DeliveredByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalePayment>()
                .HasOne(sp => sp.Sale)
                .WithMany(s => s.Payments)
                .HasForeignKey(sp => sp.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalePayment>()
                .HasOne(sp => sp.ReceivedByEmployee)
                .WithMany()
                .HasForeignKey(sp => sp.ReceivedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is not null &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }

                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
