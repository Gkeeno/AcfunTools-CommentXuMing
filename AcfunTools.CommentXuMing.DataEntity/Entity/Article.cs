using System;
using System.Collections.Generic;
using System.Text;

namespace AcfunTools.CommentXuMing.Model.Entity
{
    public class Article
    {
        public int Id { get; set; }
        public DateTime ReleaseTime { get; set; }
        public string AcNo { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public int ChannelId { get; set; }
        public int ParentChannelId { get; set; }
        public string Raw { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string AvatarImageUrl { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}
