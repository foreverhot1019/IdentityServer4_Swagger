using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace MyAuthMVC.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext([NotNull] DbContextOptions options)
            :base(options)
        {

        }
        #region 表

        #endregion
        
        /// <summary>
        /// 保存当前上下文所有变更
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            return base.SaveChanges();
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
