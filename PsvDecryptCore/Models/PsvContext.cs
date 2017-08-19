using Microsoft.EntityFrameworkCore;
using PsvDecryptCore.Common;

namespace PsvDecryptCore.Models
{
    public class PsvContext : DbContext
    {
        private readonly PsvInformation _psvInfo;

        public PsvContext(PsvInformation psvInfo)
        {
            _psvInfo =
                psvInfo;
        }

        public DbSet<Clip> Clips { get; set; }
        public DbSet<ClipTranscript> ClipTranscripts { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Module> Modules { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite($"Filename=\"{_psvInfo.FilePath}\"");
        }
    }
}