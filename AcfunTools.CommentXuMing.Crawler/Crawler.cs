using AcfunTools.CommentXuMing.Crawler.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace AcfunTools.CommentXuMing.Crawler
{
    using ConsumeDataHandle = Action<ArticleData, List<CommentData>>;

    public class Crawler
    {
        /// <summary>
        /// 需要释放
        /// 存在问题:
        ///     复用的HttpClient，一些公共的设置就没办法灵活的调整了，如请求头的自定义
        ///     HttpClient请求每个url时，会缓存该url对应的主机ip，从而会导致DNS更新失效(TTL失效)
        /// </summary>
        private static readonly HttpClient HttpClient = new HttpClient();

        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

        private ConsumeDataHandle _consumeDataHandle;

        private ConcurrentDictionary<string, CommentFetchContext> _commentFetchContexts = new ConcurrentDictionary<string, CommentFetchContext>();

        static Crawler()
        {
            HttpClient.Timeout = TimeSpan.FromSeconds(5); // 默认是100秒, 为了减少下一波压力设置周期放弃
        }

        public Crawler()
        {
        }

        public void SetDataConsumeHandle(ConsumeDataHandle handle) =>
            _consumeDataHandle = handle ?? throw new ArgumentException("数据消费函数为空");

        public async Task RunAsync()
        {
            Console.WriteLine("[Crawler] Running..............");

            var cancellationToken = _cancellationSource.Token;
            cancellationToken.Register(() => { Console.WriteLine("[Crawler] ProcessCancel.............."); });

            await Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        Console.WriteLine($"[Crawler]{DateTime.Now} ProcessStart..............");
                        await RunProcess("综合区");
                        await Task.Delay(CrawlerConstant.IntervalMillisecond_RefreshArticle);
                        await RunProcess("情感区");
                        await Task.Delay(CrawlerConstant.IntervalMillisecond_RefreshArticle - 800);
                        Console.WriteLine($"[Crawler]{DateTime.Now} ProcessOver..............处理文章数:{_commentFetchContexts.Count}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Crawler][err] unhandled exception: {0}", Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                    await SendServerErrorNotice(ex.Message);
                    throw ex;
                }
            }, cancellationToken);
        }

        public void Exit()
        {
            _cancellationSource.Cancel();
            _consumeDataHandle = null;
        }

        public dynamic DumpData()
        {
            return null;
        }

        private async Task RunProcess(string type = "综合区")
        {
            if (_consumeDataHandle == null) throw new Exception("[Crawler][err] 数据消费函数不能为空");

            // 循环每个文章（目前就综合区）的第一页, 并不断刷新是否有新的文章，加入到文章队列；
            var result = (
                type == "综合区" ? await FetchArticlesJson_综合区() :
                type == "情感区" ? await FetchArticlesJson_情感区() : await FetchArticlesJson_综合区()
                ) ?? throw new Exception("[Crawler][err] 没捉取到任何文章");
            var articles = result["data"]?["articleList"]?.AsJEnumerable() ?? throw new Exception("[Crawler][err] 没捉取到任何文章");

            foreach (var article in articles)
            {
                if ((int)article["comment_count"] < 3 || (int)article["comment_count"] > 2000) continue; // 不超过3个，超过2000个评论的不爬

                CommentFetchContext handle;
                var id = article["id"] + "";
                if (!_commentFetchContexts.TryGetValue(id, out handle)) // 没有 CommentFetchContext, 新建一个开爬!
                {
                    handle = CommentFetchContext.Initial(ResolveToArticleData(article));
                    handle.OnFindTargetComments += _consumeDataHandle;
                    handle.OnFectchOver += (sender) =>
                    {
                        Console.WriteLine($"[OnFetchOver]{DateTime.Now} FROM Id: {sender.ArticleInfo.AcNo} ；文章名称：{sender.ArticleInfo.Title}；");
                        _commentFetchContexts.TryRemove(sender.ArticleInfo.AcNo, out var _);
                    };

                    // 存储对应文章爬取任务
                    if (!_commentFetchContexts.TryAdd(id, handle)) continue;
                }

                _ = handle.RunProcessAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 获取文章区数据;
        /// 默认按"最新动态" 2
        /// </summary>
        /// <param name="acNo"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        private async Task<JObject> FetchArticlesJson_综合区(int retryCount = 0)
        {
            //var queryUrl = $"{CrawlerConstant.url_articleList}?pageNo=1&size=300&originalOnly=false&orderType=2&periodType=-1&filterTitleImage=true";
            var queryUrl = $"{CrawlerConstant.url_articleList}?pageNo=1&size=130&realmIds=5%2C22%2C3%2C4&originalOnly=false&orderType=1&periodType=-1&filterTitleImage=true";
            try
            {
                var response = await HttpClient.GetAsync(queryUrl, _cancellationSource.Token);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<JObject>(responseBody);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Crawler][err] FetchArticlesJson http Fail: {0}", e.Message);
                if (retryCount > 2)  return null;

                Console.WriteLine("[Crawler] FetchArticlesJson_综合区 http Fail，重试{0}", retryCount);
                await Task.Delay(100);
                return await FetchArticlesJson_综合区(retryCount++);
            }
        }
        /// <summary>
        /// 获取情感区数据
        /// </summary>
        /// <param name="acNo"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        private async Task<JObject> FetchArticlesJson_情感区(int retryCount = 0)
        {
            var queryUrl = $"{CrawlerConstant.url_articleList}?pageNo=1&size=80&realmIds=25%2C34%2C7%2C6%2C17%2C1%2C2&originalOnly=false&orderType=1&periodType=-1&filterTitleImage=true";
            try
            {
                var response = await HttpClient.GetAsync(queryUrl, _cancellationSource.Token);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<JObject>(responseBody);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Crawler][err] FetchArticlesJson_情感区 http Fail: {0}", e.Message);
                if (retryCount > 2) return null;

                Console.WriteLine("[Crawler] FetchArticlesJson_情感区 http Fail，重试{0}", retryCount);
                await Task.Delay(100);
                return await FetchArticlesJson_情感区(retryCount++);
            }
        }

        private ArticleData ResolveToArticleData(JToken articleRaw)
        {
            return new ArticleData
            {
                AcNo = articleRaw["id"] + "",
                ChannelId = (int)articleRaw["channel_id"],
                ParentChannelId = (int)articleRaw["parent_channel_id"],
                ReleaseTime = ((long)articleRaw["contribute_time"]).ToDateTime(),
                RawData = articleRaw.ToString(),
                Title = articleRaw["title"] + "",
                SubTitle = articleRaw["description"] + "",
                UserInfo = new UserInfo
                {
                    AvatarImageUrl = articleRaw["user_avatar"] + "",
                    Name = articleRaw["username"] + "",
                    Id = (long)articleRaw["user_id"]
                }
            };
        }

        private async Task SendServerErrorNotice(string msg = "")
        {
            var logurl = $"https://sc.ftqq.com/SCU70012T2d9cae3a9ef2addbb1086174331369f35dfc665e633f6.send?text="+ msg;
            
            var response = await HttpClient.GetAsync(logurl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
        }
    }
}
