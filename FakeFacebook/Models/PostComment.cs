using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace FakeFacebook.Models
{
    [Table("POST_COMMENT")]
    public class PostComment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int PostCode {  get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int CreatedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
