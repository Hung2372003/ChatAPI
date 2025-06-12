using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace FakeFacebook.Models
{
    [Table("FEELING_POST")]
    public class FeelingPost
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int PostId { get; set; }
        public int CreatedBy { get; set; }
        public bool Like { get; set; }
    }
}
