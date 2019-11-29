using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using static Menu_Permission.EnumType;

namespace Menu_Permission
{
    //sealed 不能被继承
    //internal 当前程序集
    //protected 只有在继承的子类中可访问，可以跨程序集
    public class ModelTmp
    {
        [Key]
        [Display(Name = "主键", Description = "主键")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Display(Name = "状态", Description = "状态")]
        [DefaultValue(1)]
        public virtual UseStatusEnum Status { get; set; } = (UseStatusEnum)1;

        [Display(Name = "新增人ID", Description = "新增人ID")]
        [MaxLength(50)]
        public string ADDID { get; set; }

        [Display(Name = "新增人", Description = "新增人")]
        [MaxLength(20)]
        public string ADDWHO { get; set; }

        [Display(Name = "新增时间", Description = "新增时间")]
        public DateTime ADDTS { get; set; }

        [Display(Name = "修改人ID", Description = "修改人ID")]
        [MaxLength(50)]
        public string EDITID { get; set; }

        [Display(Name = "修改人", Description = "修改人")]
        [MaxLength(20)]
        public string EDITWHO { get; set; }

        [Display(Name = "修改时间", Description = "修改时间")]
        public DateTime? EDITTS { get; set; }
    }

    /// <summary>
    /// 带操作点
    /// </summary>
    public class ModelTmpOP: ModelTmp
    {
        [Display(Name = "操作点", Description = "操作点")]
        [DefaultValue(0)]
        public virtual int OperatingPoint { get; set; } = 0;
    }

    /// <summary>
    /// 带Scope范围
    /// </summary>
    public class ModelTmpScope: ModelTmp
    {
        [Display(Name = "范围", Description = "范围")]
        public virtual string Scope { get; set; } = "-";
    }
}