using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FakeFacebook.Models
{
    [Table("USER_INFORMATION")]
    public class UserInformation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public bool? IsDeleted { get; set; }
        public string? PhoneNumber { get; set; }
        public int? FileCode { get; set; }
        public bool? IsEncryption { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Avatar { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public int? UpdatedBy { get; set; }
        public int? CreatedBy { get; set; }

    }
}
