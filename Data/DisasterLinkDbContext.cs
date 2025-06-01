using Microsoft.EntityFrameworkCore;
using DisasterLink_API.Entities;

namespace DisasterLink_API.Data
{
    public class DisasterLinkDbContext : DbContext
    {
        public DisasterLinkDbContext(DbContextOptions<DisasterLinkDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<AbrigoTemporario> AbrigosTemporarios { get; set; }
        public DbSet<Alerta> Alertas { get; set; }
        public DbSet<PontoDeColetaDeDoacoes> PontosDeColetaDeDoacoes { get; set; }
        public DbSet<ParticipacaoPontoColeta> ParticipacoesPontoColeta { get; set; }
        public DbSet<VisualizacaoAlerta> VisualizacoesAlerta { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração para AbrigoTemporario.Id não ser gerado pelo banco
            modelBuilder.Entity<AbrigoTemporario>()
                .Property(a => a.Id)
                .ValueGeneratedNever(); // Garante que a API fornecerá o valor

            // Configurar Chave Primária Composta para VisualizacaoAlerta
            modelBuilder.Entity<VisualizacaoAlerta>()
                .HasKey(va => new { va.UsuarioId, va.AlertaId });

            // Configurar Relacionamento VisualizacaoAlerta -> Usuario
            modelBuilder.Entity<VisualizacaoAlerta>()
                .HasOne(va => va.Usuario)
                .WithMany(u => u.VisualizacoesAlerta)
                .HasForeignKey(va => va.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar Relacionamento VisualizacaoAlerta -> Alerta
            modelBuilder.Entity<VisualizacaoAlerta>()
                .HasOne(va => va.Alerta)
                .WithMany(a => a.VisualizacoesAlerta)
                .HasForeignKey(va => va.AlertaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}