using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Menu_Permission
{
    //Please Register DbSet in DbContext.cs
    //public DbSet<MenuItem> MenuItems { get; set; }
    public partial class MenuItem : ModelTmpScope
    {
        public MenuItem()
        {
            SubMenus = new HashSet<MenuItem>();
        }

        [Display(Name = "菜单名称", Description = "菜单名称")]
        [StringLength(20),Required(ErrorMessage = "Please enter : 菜单名称")]
        //[Index("IX_MenuTitle", 1, IsUnique = false)]
        public string Title { get; set; }

        [Display(Name = "菜单描述", Description = "菜单描述")]
        [StringLength(100)]
        public string Description { get; set; }

        [Display(Name = "排序代码", Description = "菜单排序代码（0100开始）")]
        [StringLength(20),Required(ErrorMessage = "Please enter : 代码")]
        //[Index("IX_MenuCode", 1, IsUnique = true)]
        public string Code { get; set; }

        [Display(Name = "菜单Url", Description = "菜单Url")]
        [StringLength(100),Required(ErrorMessage = "Please enter : Url")]
        //[Index("IX_MenuUrl", 1, IsUnique = false)]
        public string Url { get; set; }

        [Display(Name = "控制器", Description = "控制器")]
        [StringLength(50)]
        public string Controller { get; set; } = "-";

        [Display(Name = "菜单图标", Description = "菜单图标")]
        [StringLength(50)]
        public string IconCls { get; set; }

        [Display(Name = "上级菜单", Description = "上级菜单")]
        public int? ParentId { get; set; }

        [Display(Name = "上级菜单", Description = "上级菜单")]
        [ForeignKey("ParentId")]
        public MenuItem Parent { get; set; }

        [Display(Name = "子菜单", Description = "子菜单")]
        public ICollection<MenuItem> SubMenus { get; set; }
    }
}