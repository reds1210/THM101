using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication4.Models;

public partial class Thm101Context : DbContext
{
    public Thm101Context()
    {
    }

    public Thm101Context(DbContextOptions<Thm101Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Income> Incomes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
    { 
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Income>(entity =>
        {
            entity.ToTable("Income");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Month).HasMaxLength(50);
            entity.Property(e => e.Year).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Incomes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Income_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();            
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
