using System.Collections.Generic;

namespace BlogApp.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // relacja wiele-do-wielu z Postami
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}
