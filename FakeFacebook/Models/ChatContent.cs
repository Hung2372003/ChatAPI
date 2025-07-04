using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FakeFacebook.Models
{
    [Table("CHAT_CONTENT")]
    public class ChatContent
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? GroupChatId { get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public int? UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public int? FileCode {  get; set; }   
        public DateTime? ContentTime { get; set; }
    }
}
