using System;
using System.Collections.Generic;
using System.Text;

namespace AcfunTools.CommentXuMing.Model.Entity
{
    public class Comment
    {
        public int Id { get; set; }
        public int CId { get; set; }
        public DateTime ReleaseTime { get; set; }
        public string AcNo { get; set; }
        public string Content { get; set; }
        public int Floor { get; set; }
        public string Raw { get; set; }

        public long UserId { get; set; }
        public string UserName { get; set; }
        public string AvatarImageUrl { get; set; }
    }
}
