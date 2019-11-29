using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Menu_Permission
{
    /// <summary>
    /// 菜单动作
    /// public DbSet<MenuAction> MenuAction { get; set; }
    /// </summary>
    public partial class MenuAction : ModelTmp
    {
        public MenuAction()
        {
        }

        [Display(Name = "动作名称", Description = "菜单动作名称")]
        [StringLength(20)]
        [Required(ErrorMessage = "Please enter : 菜单动作")]
        //[Index("IX_ActionName", 1, IsUnique = true)]
        public string Name { get; set; }

        [Display(Name = "代码", Description = "菜单动作代码")]
        [Required(ErrorMessage = "Please enter : 菜单代码")]
        //[Index("IX_ActionCode", 1, IsUnique = true)]
        [StringLength(20)]
        public string Code { get; set; }

        [Display(Name = "排序代码", Description = "菜单排序代码（0100开始）")]
        [StringLength(20)]
        [Required(ErrorMessage = "Please enter : 排序代码")]
        public string Sort { get; set; }

        [Display(Name = "菜单描述", Description = "菜单描述")]
        [StringLength(50)]
        public string Description { get; set; }
    }
}