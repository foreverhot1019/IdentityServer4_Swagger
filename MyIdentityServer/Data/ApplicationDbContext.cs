using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyIdentityServer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
         {
             ////判断当前数据库是Oracle 需要手动添加Schema(DBA提供的数据库账号名称)
             //if(this.Database.IsOracle())
             //{
             //    modelBuilder.HasDefaultSchema("NETCORE");
             //}
             base.OnModelCreating(modelBuilder);
         }
}
}
