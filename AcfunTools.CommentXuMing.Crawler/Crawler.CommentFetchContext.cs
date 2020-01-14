using AcfunTools.CommentXuMing.Crawler.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AcfunTools.CommentXuMing.Crawler
{
    using ConsumeDataHandle = Action<ArticleData, List<CommentData>>;

    public class CommentFetchContext
    {
        /// <summary>
        /// 问题同上类
        /// </summary>
        private static readonly HttpClient HttpClient = new HttpClient();

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private Timer _timerForExpireCountdown;

        /// <summary>
        /// key: "c112072423"; value: jobject
        /// </summary>
        private ConcurrentDictionary<string, JObject> _comments = new ConcurrentDictionary<string, JObject>();
        private ConcurrentDictionary<string, bool> _excludeCommentIds = new ConcurrentDictionary<string, bool>();

        public ArticleData ArticleInfo;

        public event ConsumeDataHandle OnFindTargetComments;
        public event Action<CommentFetchContext> OnFectchOver;

        public static CommentFetchContext Initial(ArticleData articleinfo)
        {
            return new CommentFetchContext(articleinfo);
        }

        private CommentFetchContext(ArticleData articleinfo)
        {
            this.ArticleInfo = articleinfo;
            this._timerForExpireCountdown = new Timer((sender) => this.Exit(), null, (int)TimeSpan.FromHours(1).TotalMilliseconds, -1);
        }

        public void Exit()
        {
            Console.WriteLine("[CommentFetchContext] exit....{0}", ArticleInfo.Title);
            OnFindTargetComments = null;
            _cancellationTokenSource.Cancel();
            _timerForExpireCountdown.Dispose();
            OnFectchOver?.Invoke(this);
        }
        public async Task RunProcessAsync()
        {
            try
            {
                await Process();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[CommentFetchContext] 发生未处理的异常，将停止当前作业: " + ex.Message + Environment.NewLine + ex.StackTrace);
                //Exit();
                await Task.FromException(ex);
            }
        }
        public async Task Process()
        {
            var orgnum = _comments.Count;
            //Console.WriteLine("[CommentFetchContext] running....{0}", ArticleInfo.Title);

            var comments = await FetchCommentsJson(ArticleInfo.AcNo, "1");
            if (comments == null) return;

            var totalPageCount = (int)comments["totalPage"];
            var commentInfosRaw = comments["commentsMap"].ToString();
            UpdateComments(commentInfosRaw);

            if (totalPageCount > 1)
            {
                var tasks_leftPage = from ipage in Enumerable.Range(2, totalPageCount - 1)
                                     select Task.Run(async () =>
                                     {
                                         var comments_curPage = await FetchCommentsJson(ArticleInfo.AcNo, ipage + "");
                                         var commentInfosRaw_curPage = comments_curPage["commentsMap"].ToString();
                                         UpdateComments(commentInfosRaw_curPage);
                                     });
                await Task.WhenAll(tasks_leftPage);
            }

            var curnum = _comments.Count;
            if (orgnum != curnum)
            {
                Console.WriteLine("[CommentFetchContext][评论记录变动] {1}-->{2}, ###文章###{0}", ArticleInfo.Title, orgnum, curnum);
            }
        }

        private async Task<JObject> FetchCommentsJson(string acNo, string pageIndex)
        {
            var queryUrl = $"{CrawlerConstant.url_articleComments}?sourceId={acNo}&sourceType=3&page={pageIndex}&pivotCommentId=0&newPivotCommentId=0";
            try
            {
                var response = await HttpClient.GetAsync(queryUrl, _cancellationTokenSource.Token);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<JObject>(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("[Crawler][err] FetchArticlesJson http Fail: {0}", e.Message); // 这里一般是网络环境变化会引发异常,比如切换代理
                return null;
            }
        }

        private void UpdateComments(string commentsMap)
        {
            var commentsKeyValuePair = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(commentsMap);
            var comments_target = new List<CommentData>();
            foreach (var item in commentsKeyValuePair)
            {
                var commentInfoKey = item.Key;
                var commentInfo = item.Value;
                if (_comments.TryAdd(commentInfoKey, commentInfo))// 未有记录的评论
                {
                    if (IsUnAvaliableCommment(commentInfo))
                    {
                        _excludeCommentIds.TryAdd(commentInfoKey, true);
                    }
                }
                else // 已有记录的评论
                {
                    // 查看最新获取的内容是否已失效
                    // 评论是否原来已失效
                    if (IsUnAvaliableCommment(commentInfo) && !_excludeCommentIds.ContainsKey(commentInfoKey))
                    {
                        // 找到目标评论内容已删除的，以触发目标事件
                        _comments.TryGetValue(commentInfoKey, out commentInfo);
                        var targetComment = ResovleToComment(commentInfo);
                        _excludeCommentIds.TryAdd(commentInfoKey, true);

                        Console.WriteLine("[CommentFetchContext][找到目标评论] name##{0} ==== floor##{1} ==== content##{2}", targetComment.UserInfo.Name, targetComment.Floor, targetComment.Content);

                        comments_target.Add(targetComment);
                    }
                }
            }

            if (comments_target.Any())
            {
                Console.WriteLine("[CommentFetchContext][触发找到目标事件]");
                OnFindTargetComments?.Invoke(ArticleInfo, comments_target);
            }
        }

        /// <summary>
        /// 内容为"评论已被删除"，"用户已被封禁"，即为失效评论
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        private bool IsUnAvaliableCommment(JObject comment)
        {
            var content = comment["content"] + "";
            var flag = content.Contains("该评论已被删除") || content.Contains("用户已被封禁") || content.Contains("评论正在审核中");
            return flag;
        }

        private CommentData ResovleToComment(JObject commentRaw)
        {
            return new CommentData
            {
                AcNo = ArticleInfo.AcNo,
                Content = (string)commentRaw["content"],
                Floor = (int)commentRaw["floor"],
                ReleaseTime = ((long)commentRaw["timestamp"]).ToDateTime(),
                UserInfo = new UserInfo
                {
                    AvatarImageUrl = (string)commentRaw["headUrl"]?.AsEnumerable().FirstOrDefault()?["url"],
                    Id = (long)commentRaw["userId"],
                    Name = (string)commentRaw["userName"],
                },
                CId = (int)commentRaw["cid"],
                RawData = commentRaw.ToString()
            };
        }
    }
}
