using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PsvDecryptCore.Models
{
    [Table("Course")]
    public class Course
    {
        public string AuthorsFullnames { get; set; }
        public string DefaultImageUrl { get; set; }
        public string Description { get; set; }
        public ulong DurationInMilliseconds { get; set; }
        public bool? HasTranscript { get; set; }
        public string ImageUrl { get; set; }
        public bool? IsStale { get; set; }
        public string Level { get; set; }
        [Key]
        public string Name { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string ShortDescription { get; set; }
        public string Title { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}