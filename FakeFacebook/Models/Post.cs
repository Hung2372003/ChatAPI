using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FakeFacebook.Models
{
    [Table("POSTS")]
    public class Posts
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Content { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public int LikeNumber { get; set; }
        public int CommentNumber { get; set; }
        public string? Status { get; set; }
        public bool? IsDeleted { get; set; }

    }
}
