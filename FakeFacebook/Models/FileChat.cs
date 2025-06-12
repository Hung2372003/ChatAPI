
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace FakeFacebook.Models
{
    [Table("FILE_CHAT")]
    public class FileChat
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? FileCode {  get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Path { get; set; }
        public string? NameExtension { get; set; }
        public DateTime? CreatedTime { get; set; }
        public long? Size { get; set; }
        public bool? IsDeleted { get; set; }
        public int? DeletedBy {  get; set; }
        public string? ServerCode {  get; set; }

    }
}
