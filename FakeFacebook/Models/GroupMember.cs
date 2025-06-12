using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace FakeFacebook.Models
{
    [Table("GROUP_MEMBER")]
    public class GroupMember
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int GroupChatId { get; set; }
        public int? MemberCode { get; set; }
        public bool Status { get; set; }
        public DateTime InvitedTime { get; set; }
        public int InvitedBy { get; set; }
        public DateTime DeletedTime { get; set; }
        public int DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
