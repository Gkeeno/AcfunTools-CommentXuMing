using System;
using System.Collections.Generic;
using System.Text;

namespace AcfunTools.CommentXuMing.Crawler.Dtos
{
    public class ArticleData
    {
        public DateTime ReleaseTime { get; set; }
        public string AcNo { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public int ChannelId { get; set; }
        public int ParentChannelId { get; set; }
        
        public string RawData { get; set; }

        public UserInfo UserInfo;
    }

}
