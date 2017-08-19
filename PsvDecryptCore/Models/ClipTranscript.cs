using System.ComponentModel.DataAnnotations.Schema;

namespace PsvDecryptCore.Models
{
    [Table("ClipTranscript")]
    public class ClipTranscript
    {
        public int ClipId { get; set; }
        public ulong EndTime { get; set; }
        public int Id { get; set; }
        public ulong StartTime { get; set; }
        public string Text { get; set; }
    }
}