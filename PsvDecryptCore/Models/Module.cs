using System.ComponentModel.DataAnnotations.Schema;

namespace PsvDecryptCore.Models
{
    [Table("Module")]
    public class Module :IPsvObject
    {
        public string AuthorHandle { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public ulong DurationInMilliseconds { get; set; }
        public int Id { get; set; }
        public int ModuleIndex { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
    }
}