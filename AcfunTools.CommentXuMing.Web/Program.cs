using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcfunTools.CommentXuMing.Crawler.Dtos;
using AcfunTools.CommentXuMing.Model;
using AcfunTools.CommentXuMing.Model.Entity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AcfunTools.CommentXuMing.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.SetMaxThreads(1 /*工作线程*/, 200 /*IO线程*/); // 俩处线程数值必须 >= 主机逻辑线程数

            var host = CreateHostBuilder(args).Build();
            // 创建爬虫
            var seviScope = host.Services.CreateScope();
            var seviProvider = seviScope.ServiceProvider;
            var logger = seviProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                var dbcontext = seviProvider.GetRequiredService<CommentXuMingDbContext>();
                var config = seviProvider.GetRequiredService<IConfiguration>();
                var dbSaveLocker = new object();
                var crawler = new Crawler.Crawler(config["NotifySendUrl:CrawlerStop"]);
                crawler.SetDataConsumeHandle((article, comments) =>
                {
                    Console.WriteLine("摸到了! ##Title## {0}##comments## >>>>> {1}",
                        article.Title + Environment.NewLine,
                        Environment.NewLine + string.Join(Environment.NewLine, comments.Select(c => $"#{c.Floor} 用户名:{c.UserInfo.Name}===内容：{c.Content}"))
                        );

                    try
                    {
                        lock (dbSaveLocker)
                        {
                            var articleEntity = dbcontext.Articles.FirstOrDefault(a => a.AcNo == article.AcNo);
                            if (articleEntity == null)
                            {
                                dbcontext.Articles.Add(new Article
                                {
                                    AcNo = article.AcNo,
                                    SubTitle = article.SubTitle,
                                    Title = article.Title,
                                    ChannelId = article.ChannelId,
                                    ParentChannelId = article.ParentChannelId,
                                    Raw = article.RawData,
                                    ReleaseTime = article.ReleaseTime,
                                    UserId = article.UserInfo.Id,
                                    UserName = article.UserInfo.Name,
                                    AvatarImageUrl = article.UserInfo.AvatarImageUrl,
                                });
                            }
                            dbcontext.Comments.AddRange(comments.Select(c => new Comment
                            {
                                AcNo = c.AcNo,
                                CId = c.CId,
                                Content = c.Content,
                                Floor = c.Floor,
                                Raw = c.RawData,
                                ReleaseTime = c.ReleaseTime,
                                UserId = c.UserInfo.Id,
                                UserName = c.UserInfo.Name,
                                AvatarImageUrl = c.UserInfo.AvatarImageUrl,
                            }));
                            dbcontext.SaveChanges(); // 不能同时在多个线程中执行此操作（上个线程没完成下个线程启动，会抛出异常)
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("数据库更新错误:" + Environment.NewLine + ex.Message);
                        throw ex;
                    }
                });
                crawler.RunAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "创建爬虫错误.");
                throw ex;
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseUrls("http://*:8000;")
                    .UseStartup<Startup>();
                })
            ;
    }
}
