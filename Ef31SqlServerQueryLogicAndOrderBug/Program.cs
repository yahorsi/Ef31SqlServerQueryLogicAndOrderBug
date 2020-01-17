using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Ef31SqlServerQueryLogicAndOrderBug
{
    [Table("Keyboard")]
    public class Keyboard
    {
        public Keyboard()
        {
            Buttons = new HashSet<Button>();
        }

        [Key]
        public int Id { get; set; }

        public string Value { get; set; }

        [InverseProperty("Keyboard")]
        public ICollection<Button> Buttons { get; set; }
    }

    [Table("Button")]
    public class Button
    {
        [Key]
        public int Id { get; set; }

        public int KeyboardId { get; set; }

        public string Value { get; set; }

        [ForeignKey("KeyboardId")]
        [InverseProperty("Buttons")]
        public Keyboard Keyboard { get; set; }
    }

    public class DatabaseContext : DbContext
    {
        public DbSet<Keyboard> Keyboards { get; set; }
        public DbSet<Button> Buttons { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "Server=.;Database=Ef31SqlServerQueryLogicAndOrderBug;Trusted_Connection=True;";
            var connection = new SqlConnection(connectionString);
            optionsBuilder.UseSqlServer(connection);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Keyboard>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });
            modelBuilder.Entity<Button>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.HasOne(d => d.Keyboard)
                    .WithMany(p => p.Buttons)
                    .HasForeignKey(d => d.KeyboardId)
                    .HasConstraintName("[FK_Keyboard_Id]");
            });
        }
    }

    class Program
    {
        private static Keyboard BuildKeyboard()
        {
            var keyboardEntry = new Keyboard()
            {
                Id = 1,
                Value = "Entry1"
            };

            var button1 = new Button()
            {
                Id = 1,
                KeyboardId = 1,
                Value = "ButtonEntry1"
            };

            var button2 = new Button()
            {
                Id = 2,
                KeyboardId = 1,
                Value = "ButtonEntry2"
            };

            keyboardEntry.Buttons.Add(button1);
            keyboardEntry.Buttons.Add(button2);

            return keyboardEntry;
        }

        private static void CleanupDatabase()
        {
            var context = new DatabaseContext();
            //context.Database.ExecuteSqlRaw("DELETE FROM Keyboard");
            context.Database.ExecuteSqlCommand("DELETE FROM Keyboard");
        }

        private static void AddInitialKeyboard()
        {
            var context = new DatabaseContext();

            context.Keyboards.Add(BuildKeyboard());
            context.SaveChanges();
        }

        private static void UpdateUsingSimpleRemoveAndAdd(int id)
        {
            var context = new DatabaseContext();
            var keyboard = BuildKeyboard();
            context.Keyboards.Remove(keyboard);
            context.Keyboards.Add(keyboard);
            context.SaveChanges();
        }

        private static void UpdateUsingSimpleRemoveAndAdd2(int id)
        {
            var context = new DatabaseContext();
            context.Keyboards.Remove(BuildKeyboard());
            context.Keyboards.Add(BuildKeyboard());
            context.SaveChanges();
        }

        private static void UpdateUsingSimpleRemoveAndAdd3(int id)
        {
            var context = new DatabaseContext();
            context.Keyboards.Update(BuildKeyboard());
            context.SaveChanges();
        }

        private static void UpdateUsingExistenceCheck(int id)
        {
            var context = new DatabaseContext();

            var existingKeyboard = context.Keyboards.Find(id);

            if (existingKeyboard != null)
            {
                context.Keyboards.Remove(existingKeyboard);
            }

            context.Keyboards.Add(BuildKeyboard());
            context.SaveChanges();
        }

        private static void UpdateUsingExistenceCheck2(int id)
        {
            var context = new DatabaseContext();

            var existingKeyboard = context.Keyboards.Find(id);

            if (existingKeyboard != null)
            {
                context.Keyboards.Remove(existingKeyboard);
                context.SaveChanges();
            }

            context.Keyboards.Add(BuildKeyboard());
            context.SaveChanges();
        }

        static void CatchAndPrintException(Action a)
        {
            try
            {
                a();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void Case1()
        {
            CleanupDatabase();
            CatchAndPrintException(() => UpdateUsingSimpleRemoveAndAdd(1));
            CatchAndPrintException(() => UpdateUsingSimpleRemoveAndAdd(1));
        }

        static void Case2()
        {
            CleanupDatabase();
            CatchAndPrintException(() => UpdateUsingSimpleRemoveAndAdd2(1));
            CatchAndPrintException(() => UpdateUsingSimpleRemoveAndAdd2(1));
        }

        static void Case3()
        {
            CleanupDatabase();
            CatchAndPrintException(() => UpdateUsingSimpleRemoveAndAdd3(1));
            CatchAndPrintException(() => UpdateUsingSimpleRemoveAndAdd3(1));
        }

        static void Case4()
        {
            CleanupDatabase();
            CatchAndPrintException(() => UpdateUsingExistenceCheck(1));
            CatchAndPrintException(() => UpdateUsingExistenceCheck(1));
        }

        static void Case5()
        {
            CleanupDatabase();
            CatchAndPrintException(() => UpdateUsingExistenceCheck2(1));
            CatchAndPrintException(() => UpdateUsingExistenceCheck2(1));
        }

        static void Main(string[] args)
        {
            try
            {
                Case1();
                Case2();
                Case3();
                Case4();
                Case5();

                Console.WriteLine("Hello World!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
}
    }
}
