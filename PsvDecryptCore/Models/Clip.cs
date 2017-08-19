using System.ComponentModel.DataAnnotations.Schema;

namespace PsvDecryptCore.Models
{
    [Table("Clip")]
    public class Clip
    {
        public int ClipIndex { get; set; }
        public ulong DurationInMilliseconds { get; set; }
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
        public bool SupportsStandard { get; set; }
        public bool SupportsWidescreen { get; set; }
        public string Title { get; set; }
    }
}