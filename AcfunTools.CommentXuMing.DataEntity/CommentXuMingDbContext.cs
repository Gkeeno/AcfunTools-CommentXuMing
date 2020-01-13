using AcfunTools.CommentXuMing.Model.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcfunTools.CommentXuMing.Model
{
    public class CommentXuMingDbContext : DbContext
    {
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Article> Articles { get; set; }
        public CommentXuMingDbContext(DbContextOptions<CommentXuMingDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}


