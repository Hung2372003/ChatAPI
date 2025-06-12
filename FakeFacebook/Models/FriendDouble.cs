using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FakeFacebook.Models
{
    [Table("FRIENDS_DOUBLE")]
    public class FriendDouble
    {
     
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserCode1 { get; set; }
        public int UserCode2 { get; set; }
        public bool IsDeleted { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public int CreatedBy { get; set; }
        
    }
}
