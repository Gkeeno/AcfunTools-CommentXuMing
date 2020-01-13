using System;
using System.Collections.Generic;
using System.Text;

namespace AcfunTools.CommentXuMing.Crawler.Dtos
{
    public class CommentData
    {
        public DateTime ReleaseTime { get; set; }
        public string AcNo { get; set; }
        public int CId { get; set; }
        public string Content { get; set; }
        public int Floor { get; set; }
        public string RawData { get; set; }

        public UserInfo UserInfo;
    }

}
