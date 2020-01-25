using System;
using System.Collections.Generic;
using System.Text;

namespace AcfunTools.CommentXuMing.Crawler
{
    public static class CrawlerConstant
    {
        public static string url_articleList = "https://webapi.acfun.cn/query/article/list";
        public static string url_articleComments = "https://www.acfun.cn/rest/pc-direct/comment/listByFloor";

        public static int IntervalMillisecond_RefreshArticle = 7_300;
    }
}
