using System;
using System.Collections.Generic;
using HospitalManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Data;

public partial class HospitalDbContext : DbContext
{
    public HospitalDbContext()
    {
    }

    public HospitalDbContext(DbContextOptions<HospitalDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Article> Articles { get; set; }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<Careorder> Careorders { get; set; }

    public virtual DbSet<Drug> Drugs { get; set; }

    public virtual DbSet<Exammetric> Exammetrics { get; set; }

    public virtual DbSet<Medicalrecord> Medicalrecords { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Prescriptiondetail> Prescriptiondetails { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Servicedetail> Servicedetails { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<Syslog> Syslogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=G5_QLBV.mssql.somee.com;Database=G5_QLBV;User Id=ainha_SQLLogin_1;Password=wkvj3vbtgx;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.ApptId);

            entity.ToTable("APPOINTMENTS");

            entity.Property(e => e.ApptId)
                .ValueGeneratedNever()
                .HasColumnName("ApptID");
            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TimeSlot)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK_APPOINTMENTS_USERS");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK_APPOINTMENTS_PATIENTS");
        });

        modelBuilder.Entity<Article>(entity =>
        {
            entity.ToTable("ARTICLES");

            entity.Property(e => e.ArticleId)
                .ValueGeneratedNever()
                .HasColumnName("ArticleID");
            entity.Property(e => e.AuthorId).HasColumnName("AuthorID");
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("smalldatetime");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Author).WithMany(p => p.Articles)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK_ARTICLES_USERS");
        });

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.ToTable("BILLS");

            entity.Property(e => e.BillId)
                .ValueGeneratedNever()
                .HasColumnName("BillID");
            entity.Property(e => e.BillType).HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasColumnType("smalldatetime");
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.Record).WithMany(p => p.Bills)
                .HasForeignKey(d => d.RecordId)
                .HasConstraintName("FK_BILLS_MEDICALRECORDS");
        });

        modelBuilder.Entity<Careorder>(entity =>
        {
            entity.ToTable("CAREORDERS");

            entity.Property(e => e.CareOrderId)
                .ValueGeneratedNever()
                .HasColumnName("CareOrderID");
            entity.Property(e => e.ActionName).HasMaxLength(255);
            entity.Property(e => e.ExecuteAt).HasColumnType("smalldatetime");
            entity.Property(e => e.NurseId).HasColumnName("NurseID");
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Nurse).WithMany(p => p.Careorders)
                .HasForeignKey(d => d.NurseId)
                .HasConstraintName("FK_CAREORDERS_USERS");

            entity.HasOne(d => d.Record).WithMany(p => p.Careorders)
                .HasForeignKey(d => d.RecordId)
                .HasConstraintName("FK_CAREORDERS_MEDICALRECORDS");
        });

        modelBuilder.Entity<Drug>(entity =>
        {
            entity.ToTable("DRUGS");

            entity.Property(e => e.DrugId)
                .ValueGeneratedNever()
                .HasColumnName("DrugID");
            entity.Property(e => e.DrugName).HasMaxLength(100);
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<Exammetric>(entity =>
        {
            entity.HasKey(e => e.RecordId);

            entity.ToTable("EXAMMETRICS");

            entity.Property(e => e.RecordId)
                .ValueGeneratedNever()
                .HasColumnName("RecordID");
            entity.Property(e => e.AxisLeft).HasColumnName("AXIS_Left");
            entity.Property(e => e.AxisRight).HasColumnName("AXIS_Right");
            entity.Property(e => e.CylLeft).HasColumnName("CYL_Left");
            entity.Property(e => e.CylRight).HasColumnName("CYL_Right");
            entity.Property(e => e.IopLeft).HasColumnName("IOP_Left");
            entity.Property(e => e.IopRight).HasColumnName("IOP_Right");
            entity.Property(e => e.SphLeft).HasColumnName("SPH_Left");
            entity.Property(e => e.SphRight).HasColumnName("SPH_Right");

            entity.HasOne(d => d.Record).WithOne(p => p.Exammetric)
                .HasForeignKey<Exammetric>(d => d.RecordId)
                .HasConstraintName("FK_EXAMMETRICS_MEDICALRECORDS");
        });

        modelBuilder.Entity<Medicalrecord>(entity =>
        {
            entity.HasKey(e => e.RecordId);

            entity.ToTable("MEDICALRECORDS");

            entity.Property(e => e.RecordId)
                .ValueGeneratedNever()
                .HasColumnName("RecordID");
            entity.Property(e => e.Diagnosis).HasMaxLength(1000);
            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.IsInpatient).HasDefaultValue(false);
            entity.Property(e => e.NurseId).HasColumnName("NurseID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Symptoms).HasMaxLength(1000);
            entity.Property(e => e.VisitDate).HasColumnType("smalldatetime");

            entity.HasOne(d => d.Doctor).WithMany(p => p.MedicalrecordDoctors)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK_MEDICALRECORDS_USERS_1");

            entity.HasOne(d => d.Nurse).WithMany(p => p.MedicalrecordNurses)
                .HasForeignKey(d => d.NurseId)
                .HasConstraintName("FK_MEDICALRECORDS_USERS_2");

            entity.HasOne(d => d.Patient).WithMany(p => p.Medicalrecords)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK_MEDICALRECORDS_PATIENTS");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("PATIENTS");

            entity.Property(e => e.PatientId).ValueGeneratedNever();
            entity.Property(e => e.DateofBirth).HasColumnType("smalldatetime");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.HealthInsuranceCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Prescriptiondetail>(entity =>
        {
            entity.HasKey(e => new { e.RecordId, e.DrugId });

            entity.ToTable("PRESCRIPTIONDETAILS");

            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.DrugId).HasColumnName("DrugID");
            entity.Property(e => e.Dosage).HasMaxLength(255);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Drug).WithMany(p => p.Prescriptiondetails)
                .HasForeignKey(d => d.DrugId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PRESCRIPTIONDETAILS_DRUGS");

            entity.HasOne(d => d.Record).WithMany(p => p.Prescriptiondetails)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PRESCRIPTIONDETAILS_MEDICALRECORDS");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("QUESTIONS");

            entity.Property(e => e.QuestionId)
                .ValueGeneratedNever()
                .HasColumnName("QuestionID");
            entity.Property(e => e.CreatedAt).HasColumnType("smalldatetime");
            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Doctor).WithMany(p => p.Questions)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK_QUESTIONS_USERS");

            entity.HasOne(d => d.Patient).WithMany(p => p.Questions)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK_QUESTIONS_PATIENTS");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("ROOMS");

            entity.Property(e => e.RoomId)
                .ValueGeneratedNever()
                .HasColumnName("RoomID");
            entity.Property(e => e.RoomName).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("SERVICES");

            entity.Property(e => e.ServiceId)
                .ValueGeneratedNever()
                .HasColumnName("ServiceID");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnName("category");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ServiceName).HasMaxLength(100);
        });

        modelBuilder.Entity<Servicedetail>(entity =>
        {
            entity.HasKey(e => new { e.RecordId, e.ServiceId });

            entity.ToTable("SERVICEDETAILS");

            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.PaymentStatus).HasMaxLength(20);
            entity.Property(e => e.ResultNote).HasMaxLength(1000);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Record).WithMany(p => p.Servicedetails)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SERVICEDETAILS_MEDICALRECORDS");

            entity.HasOne(d => d.Service).WithMany(p => p.Servicedetails)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SERVICEDETAILS_SERVICES");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.ToTable("SHIFTS");

            entity.Property(e => e.ShiftId)
                .ValueGeneratedNever()
                .HasColumnName("ShiftID");
            entity.Property(e => e.RoomId).HasColumnName("RoomID");
            entity.Property(e => e.ShiftDate).HasColumnType("smalldatetime");
            entity.Property(e => e.TimeFrame)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Room).WithMany(p => p.Shifts)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_SHIFTS_ROOMS");

            entity.HasOne(d => d.User).WithMany(p => p.Shifts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_SHIFTS_USERS");
        });

        modelBuilder.Entity<Syslog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.ToTable("SYSLOGS");

            entity.Property(e => e.LogId)
                .ValueGeneratedNever()
                .HasColumnName("LogID");
            entity.Property(e => e.Action).HasMaxLength(500);
            entity.Property(e => e.LoggedAt).HasColumnType("smalldatetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Syslogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_SYSLOGS_USERS");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USERS");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Fullname).HasMaxLength(100);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
