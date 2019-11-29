using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Menu_Permission
{
    /// <summary>
    /// 角色菜单
    /// public DbSet<RoleMenu> RoleMenu { get; set; }
    /// </summary>
    public partial class RoleMenu : ModelTmp
    {
        [Required, StringLength(20), Display(Name = "角色", Description = "角色", Order = 1)]
        //[Index("IX_RoleMenu", 1, IsUnique = true)]
        public string RoleName { get; set; }

        [Required, StringLength(50), Display(Name = "角色Id", Description = "角色Id", Order = 2)]
        //[Index("IX_RoleMenu", 2, IsUnique = true)]
        public string RoleId { get; set; }

        [Required, Display(Name = "菜单Id", Description = "菜单Id", Order = 3)]
        //[Index("IX_RoleMenu", 3, IsUnique = true)]
        public int MenuId { get; set; }

        [ForeignKey("MenuId"), Display(Name = "菜单", Description = "菜单")]
        public MenuItem MenuItem { get; set; }
    }
}