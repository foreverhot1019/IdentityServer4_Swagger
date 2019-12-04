using System;
using System.Collections.Generic;
using System.Text;
using Menu_Permission;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyIdentityServer.Models;

namespace MyIdentityServer.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>//DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region 表

        //public DbSet<IdentityUser> Users { get; set; }

        public DbSet<MenuItem> MenuItem { get; set; }

        public DbSet<MenuAction> MenuAction { get; set; }

        public DbSet<RoleMenu> RoleMenu { get; set; }

        #endregion

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
