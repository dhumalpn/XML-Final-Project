using InventoryManagement.Models;
using InventoryManagement.Pages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagement.Data
{
    public class InventoryManagementContext : DbContext
    {
        public InventoryManagementContext (DbContextOptions<InventoryManagementContext> options)
            : base(options)
        {}
		public DbSet<Product> Product { get; set; } = default!; // Pluralize
		public DbSet<Inventory> Inventory { get; set; } = default!; // Add

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Configure relationship
			modelBuilder.Entity<Inventory>()
			    .HasOne(i => i.Product)
			    .WithMany(p => p.Inventories)
			    .HasForeignKey(i => i.ProductId)
			    .OnDelete(DeleteBehavior.Cascade);

			// Create index on ProductId + StorageLocation (for multi-location queries)
			modelBuilder.Entity<Inventory>()
			    .HasIndex(i => new { i.ProductId, i.StorageLocation });
		}
	}
}
