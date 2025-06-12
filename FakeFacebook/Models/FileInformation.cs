using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace FakeFacebook.Models
{
    [Table("FILE_INFORMATION")]
    public class FileInformation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? Type { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public int? Code {  get; set; }

    }
}
