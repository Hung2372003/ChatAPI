using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace FakeFacebook.Models
{
    [Table("CHAT_GROUPS")]
    public class ChatGroups
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? GroupName { get; set; }
        public string? GroupAvartar { get; set; }
        public bool Status { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool GroupDouble { get; set; }
        public int? Quantity { get; set; }
    }
}
