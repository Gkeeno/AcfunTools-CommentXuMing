using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var host = CreateHostBuilder(args).Build();
            // ��������
            var seviScope = host.Services.CreateScope();
            var seviProvider = seviScope.ServiceProvider;
            var logger = seviProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                var dbcontext = seviProvider.GetRequiredService<CommentXuMingDbContext>();
                var dbSaveLocker = new object();
                var crawler = new Crawler.Crawler();
                crawler.SetDataConsumeHandle((article, comments) =>
                {
                    Console.WriteLine("������! ##Title## {0} , ##comments## {1}",
                        Environment.NewLine + article.Title,
                        Environment.NewLine + JsonConvert.SerializeObject(comments));

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
                            dbcontext.SaveChanges(); // ����ͬʱ�ڶ���߳���ִ�д˲���
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("���ݿ���´���:" + Environment.NewLine + ex.Message);
                        throw ex;
                    }
                });
                crawler.RunAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "�����������.");
                throw ex;
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
