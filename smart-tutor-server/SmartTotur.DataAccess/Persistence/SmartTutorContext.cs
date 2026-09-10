using Microsoft.EntityFrameworkCore;
using SmartTutor.Domain.Models;

namespace SmartTutor.DataAccess.Persistence
{
    public class SmartTutorContext : DbContext
    {
        public SmartTutorContext(DbContextOptions<SmartTutorContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AttendanceLog> AttendanceLogs { get; set; } = null!;
        public virtual DbSet<ClassEnrollment> ClassEnrollments { get; set; } = null!;
        public virtual DbSet<Class> Classes { get; set; } = null!;
        public virtual DbSet<MonthlyReport> MonthlyReports { get; set; } = null!;
        public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Session> Sessions { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");

                entity.Property(e => e.RoleName)
                    .IsUnicode(false)
                    .HasMaxLength(50);

                entity.HasIndex(e => e.RoleName)
                    .IsUnique();

                entity.HasData(
                    new Role { Id = 1, RoleName = "Admin" },
                    new Role { Id = 2, RoleName = "User" }
                );
            });

            // RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");

                entity.Property(e => e.Token)
                    .IsUnicode(false)
                    .HasMaxLength(255);

                entity.HasIndex(e => e.Token);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AttendanceLog
            modelBuilder.Entity<AttendanceLog>(entity =>
            {
                entity.ToTable("AttendanceLogs");

                entity.Property(e => e.AttendanceStatus)
                    .IsUnicode(false)
                    .HasMaxLength(20);

                entity.Property(e => e.Attitude)
                    .HasMaxLength(100);

                entity.HasOne(e => e.Session)
                    .WithMany(e => e.AttendanceLogs)
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Student)
                    .WithMany(e => e.AttendanceLogs)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ClassEnrollment
            modelBuilder.Entity<ClassEnrollment>(entity =>
            {
                entity.ToTable("ClassEnrollments");

                entity.Property(e => e.CustomFee)
                    .HasPrecision(18, 2);

                entity.Property(e => e.JoinedDate)
                    .HasColumnType("date");

                entity.Property(e => e.Status)
                    .IsUnicode(false)
                    .HasMaxLength(20);

                entity.HasOne(e => e.Class)
                    .WithMany(e => e.ClassEnrollments)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Student)
                    .WithMany(e => e.ClassEnrollments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Class
            modelBuilder.Entity<Class>(entity =>
            {
                entity.ToTable("Classes");

                entity.Property(e => e.ClassName)
                    .HasMaxLength(150);

                entity.Property(e => e.ClassType)
                    .IsUnicode(false)
                    .HasMaxLength(20);

                entity.Property(e => e.DefaultFeePerSession)
                    .HasPrecision(18, 2);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Classes)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // MonthlyReport
            modelBuilder.Entity<MonthlyReport>(entity =>
            {
                entity.ToTable("MonthlyReports");

                entity.Property(e => e.ReportMonth)
                    .IsUnicode(false)
                    .HasMaxLength(7);

                entity.Property(e => e.TotalHours)
                    .HasPrecision(5, 2);

                entity.Property(e => e.GrossAmount)
                    .HasPrecision(18, 2);

                entity.Property(e => e.CreditDeducted)
                    .HasPrecision(18, 2);

                entity.Property(e => e.FinalAmount)
                    .HasPrecision(18, 2);

                entity.Property(e => e.AmountPaid)
                    .HasPrecision(18, 2);

                entity.Property(e => e.OverpaidAmount)
                    .HasPrecision(18, 2);

                entity.Property(e => e.TransferCode)
                    .IsUnicode(false)
                    .HasMaxLength(50);

                entity.Property(e => e.MagicToken)
                    .IsUnicode(false)
                    .HasMaxLength(64);

                entity.Property(e => e.PaymentStatus)
                    .IsUnicode(false)
                    .HasMaxLength(20);

                entity.HasOne(e => e.Class)
                    .WithMany(e => e.MonthlyReports)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Student)
                    .WithMany(e => e.MonthlyReports)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PaymentTransaction
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.ToTable("PaymentTransactions");

                entity.Property(e => e.Gateway)
                    .IsUnicode(false)
                    .HasMaxLength(50);

                entity.Property(e => e.TransactionId)
                    .IsUnicode(false)
                    .HasMaxLength(100);

                entity.Property(e => e.AmountIn)
                    .HasPrecision(18, 2);

                entity.HasOne(e => e.MonthlyReport)
                    .WithMany(e => e.PaymentTransactions)
                    .HasForeignKey(e => e.ReportId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Student)
                    .WithMany(e => e.PaymentTransactions)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Session
            modelBuilder.Entity<Session>(entity =>
            {
                entity.ToTable("Sessions");

                entity.Property(e => e.SessionDate)
                    .HasColumnType("date");

                entity.Property(e => e.DurationHours)
                    .HasPrecision(4, 2);

                entity.Property(e => e.Status)
                    .IsUnicode(false)
                    .HasMaxLength(20);

                entity.HasOne(e => e.Class)
                    .WithMany(e => e.Sessions)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Student
            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Students");

                entity.Property(e => e.FullName)
                    .HasMaxLength(100);

                entity.Property(e => e.GradeLevel)
                    .HasMaxLength(30);

                entity.Property(e => e.ParentName)
                    .HasMaxLength(100);

                entity.Property(e => e.ParentPhone)
                    .IsUnicode(false)
                    .HasMaxLength(20);

                entity.Property(e => e.CreditBalance)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Status)
                    .IsUnicode(false)
                    .HasMaxLength(20);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Students)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.Property(e => e.FullName)
                    .HasMaxLength(100);

                entity.Property(e => e.Phone)
                    .IsUnicode(false)
                    .HasMaxLength(20);

                entity.Property(e => e.Email)
                    .IsUnicode(false)
                    .HasMaxLength(100);

                entity.Property(e => e.PasswordHash)
                    .IsUnicode(false)
                    .HasMaxLength(255);

                entity.Property(e => e.BankCode)
                    .IsUnicode(false)
                    .HasMaxLength(20);

                entity.Property(e => e.BankAccountNumber)
                    .IsUnicode(false)
                    .HasMaxLength(50);

                entity.Property(e => e.BankAccountName)
                    .HasMaxLength(100);

                entity.Property(e => e.WebhookToken)
                    .IsUnicode(false)
                    .HasMaxLength(100);

                entity.HasOne(e => e.Role)
                    .WithMany(e => e.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
