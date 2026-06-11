using Microsoft.EntityFrameworkCore;
using PickupOrderSystem.Domain.Entities;
using PickupOrderSystem.Domain.Enums;

namespace PickupOrderSystem.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<PickupRequest> PickupRequests => Set<PickupRequest>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Occurrence> Occurrences => Set<Occurrence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Driver>(e =>
        {
            e.ToTable("drivers");
            e.HasKey(d => d.Id);
            e.Property(d => d.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(d => d.Name).HasColumnName("nome").HasMaxLength(255).IsRequired();
            e.Property(d => d.Cnh).HasColumnName("cnh").HasMaxLength(20).IsRequired();
            e.HasIndex(d => d.Cnh).IsUnique();
            e.Property(d => d.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            e.Property(d => d.Phone).HasColumnName("telefone").HasMaxLength(20).IsRequired();
            e.Property(d => d.Active).HasColumnName("ativo").IsRequired().HasDefaultValue(true);
            e.Property(d => d.AdmissionDate).HasColumnName("data_admissao").IsRequired();
            e.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(d => d.UpdatedAt).HasColumnName("updated_at").IsRequired();
        });

        modelBuilder.Entity<Client>(e =>
        {
            e.ToTable("clients");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(c => c.Name).HasColumnName("nome").HasMaxLength(255).IsRequired();
            e.Property(c => c.Cnpj).HasColumnName("cnpj").HasMaxLength(18).IsRequired();
            e.HasIndex(c => c.Cnpj).IsUnique();
            e.Property(c => c.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            e.HasIndex(c => c.Email);
            e.Property(c => c.Phone).HasColumnName("telefone").HasMaxLength(20).IsRequired();
            e.Property(c => c.Address).HasColumnName("endereco").HasMaxLength(500).IsRequired();
            e.Property(c => c.Active).HasColumnName("ativo").IsRequired().HasDefaultValue(true);
            e.HasIndex(c => c.Active);
            e.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("now()");
            e.Property(c => c.UpdatedAt).HasColumnName("updated_at").IsRequired().HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Vehicle>(e =>
        {
            e.ToTable("vehicles");
            e.HasKey(v => v.Id);
            e.Property(v => v.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(v => v.LicensePlate).HasColumnName("placa").HasMaxLength(10).IsRequired();
            e.HasIndex(v => v.LicensePlate).IsUnique();
            e.Property(v => v.Model).HasColumnName("modelo").HasMaxLength(100).IsRequired();
            e.Property(v => v.CapacityKg).HasColumnName("capacidade_kg").HasColumnType("decimal(10,2)").IsRequired();
            e.Property(v => v.CapacityM3).HasColumnName("capacidade_m3").HasColumnType("decimal(10,2)").IsRequired();
            e.Property(v => v.ManufactureYear).HasColumnName("ano_fabricacao").IsRequired();
            e.Property(v => v.Active).HasColumnName("ativo").IsRequired().HasDefaultValue(true);
            e.HasIndex(v => v.Active);
            e.Property(v => v.LastMaintenance).HasColumnName("ultima_manutencao");
            e.Property(v => v.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(v => v.UpdatedAt).HasColumnName("updated_at").IsRequired();
        });

        modelBuilder.Entity<PickupRequest>(e =>
        {
            e.ToTable("pickup_requests");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(r => r.IdentificationNumber).HasColumnName("numero_identificacao").HasMaxLength(50).IsRequired();
            e.HasIndex(r => r.IdentificationNumber).IsUnique();
            e.Property(r => r.ClientId).HasColumnName("cliente_id").IsRequired();
            e.Property(r => r.Sender).HasColumnName("remetente").HasMaxLength(255).IsRequired();
            e.Property(r => r.PickupAddress).HasColumnName("endereco_coleta").HasMaxLength(500).IsRequired();
            e.Property(r => r.Recipient).HasColumnName("destinatario").HasMaxLength(255).IsRequired();
            e.Property(r => r.DeliveryAddress).HasColumnName("endereco_entrega").HasMaxLength(500).IsRequired();
            e.Property(r => r.RequestDate).HasColumnName("data_solicitacao").IsRequired();
            e.Property(r => r.ScheduledPickupDate).HasColumnName("data_coleta_prevista").IsRequired();
            e.HasIndex(r => r.ScheduledPickupDate);
            e.Property(r => r.Priority).HasColumnName("prioridade").HasConversion<string>().IsRequired();
            e.Property(r => r.Status).HasColumnName("status").HasConversion<string>().IsRequired().HasDefaultValue(PickupRequestStatus.Aberta);
            e.HasIndex(r => r.Status);
            e.Property(r => r.Notes).HasColumnName("observacoes");
            e.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("now()");
            e.Property(r => r.UpdatedAt).HasColumnName("updated_at").IsRequired().HasDefaultValueSql("now()");

            e.HasOne(r => r.Client)
                .WithMany()
                .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Assignment>(e =>
        {
            e.ToTable("assignments");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(a => a.PickupRequestId).HasColumnName("solicitacao_id").IsRequired();
            e.HasIndex(a => a.PickupRequestId);
            e.Property(a => a.DriverId).HasColumnName("motorista_id").IsRequired();
            e.HasIndex(a => a.DriverId);
            e.Property(a => a.VehicleId).HasColumnName("veiculo_id").IsRequired();
            e.HasIndex(a => a.VehicleId);
            e.Property(a => a.AssignmentDate).HasColumnName("data_atribuicao").IsRequired().HasDefaultValueSql("now()");
            e.HasIndex(a => a.AssignmentDate);
            e.Property(a => a.ActualStartDate).HasColumnName("data_inicio_real");
            e.Property(a => a.ActualEndDate).HasColumnName("data_conclusao_real");
            e.Property(a => a.DriverNotes).HasColumnName("observacoes_motorista");
            e.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(a => a.UpdatedAt).HasColumnName("updated_at").IsRequired();

            e.HasOne(a => a.PickupRequest)
                .WithMany(r => r.Assignments)
                .HasForeignKey(a => a.PickupRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Driver)
                .WithMany()
                .HasForeignKey(a => a.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Vehicle)
                .WithMany()
                .HasForeignKey(a => a.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Occurrence>(e =>
        {
            e.ToTable("occurrences");
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(o => o.PickupRequestId).HasColumnName("solicitacao_id").IsRequired();
            e.HasIndex(o => o.PickupRequestId);
            e.Property(o => o.Type).HasColumnName("tipo").HasConversion<string>().IsRequired();
            e.HasIndex(o => o.Type);
            e.Property(o => o.Description).HasColumnName("descricao").IsRequired();
            e.Property(o => o.OccurrenceDate).HasColumnName("data_hora").IsRequired().HasDefaultValueSql("now()");
            e.HasIndex(o => o.OccurrenceDate);
            e.Property(o => o.RegisteredBy).HasColumnName("usuario_id").HasMaxLength(255).IsRequired();
            e.Property(o => o.Resolved).HasColumnName("resolvida").IsRequired().HasDefaultValue(false);
            e.HasIndex(o => o.Resolved);
            e.Property(o => o.ResolutionNotes).HasColumnName("observacoes_resolucao");
            e.Property(o => o.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(o => o.UpdatedAt).HasColumnName("updated_at").IsRequired();

            e.HasOne(o => o.PickupRequest)
                .WithMany(r => r.Occurrences)
                .HasForeignKey(o => o.PickupRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
