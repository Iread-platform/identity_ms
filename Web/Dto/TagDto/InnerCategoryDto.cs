using System;

namespace iread_identity_ms.Web.Dto.TagDto
{
    public class InnerCategoryDto
    {
        public int CategoryId { get; set; }
        public String Title { get; set; }
        public Nullable<int> Rank { get; set; }
    }
}