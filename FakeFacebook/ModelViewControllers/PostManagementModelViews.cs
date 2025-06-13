

namespace FakeFacebook.ModelViewControllers
{
    public class PostManagementModelViews
    {
        public string? Status { get; set; }
        public string? Content { get; set; }
        public List<IFormFile>? Files { get; set; }

    }
    public class CommentModelViews
    {
        public int PostCode { get; set; }
        public string? Content { get; set; }
    }
}
