using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FakeFacebook.Models
{
    [Table("USER_ACCOUNT")]
    public class UserAccount
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        
        public int Id { get; set; }
        public int UserCode { get; set; }
        public string? UserName { get; set; }
        public string? UserPassword { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public bool? IsEncryption { get; set; }
        public string? Role { get; set; }
        public string? Permission { get; set; }
    }
}
