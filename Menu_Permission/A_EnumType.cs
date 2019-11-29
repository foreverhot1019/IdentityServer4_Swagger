using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Menu_Permission
{
    public static class EnumType
    {
        /// <summary>
        /// 使用状态
        /// </summary>
        public enum UseStatusEnum
        {
            [Display(Name = "草稿", Description = "草稿")]
            Draft = 0,
            [Display(Name = "启用", Description = "启用")]
            Enable = 1,
            [Display(Name = "停用", Description = "停用")]
            Disable = -1
        }

        /// <summary>
        /// 审批状态
        /// </summary>
        public enum AuditStatusEnum
        {
            [Display(Name = "草稿", Description = "草稿")]
            Draft = 0,
            [Display(Name = "审批中", Description = "审批中")]
            Auditing = 1,
            [Display(Name = "审批通过", Description = "审批通过")]
            AuditSuccess = 2,
            [Display(Name = "审批拒绝", Description = "审批拒绝")]
            AuditFail = -1
        }

        /// <summary>
        /// 状态
        /// </summary>
        public enum UseStatusIsOrNoEnum
        {
            [Display(Name = "否", Description = "否")]
            Draft = 0,
            [Display(Name = "是", Description = "是")]
            Enable = 1
        }

        /// <summary>
        /// 数据新增类型
        /// </summary>
        public enum AddType
        {
            [Display(Name = "自动", Description = "自动产生")]
            AutoAdd =1,
            [Display(Name = "手动", Description = "手动产生")]
            HandAdd
        }

        /// <summary>
        /// 发送状态
        /// </summary>
        public enum SendStatus
        {
            [Display(Name = "正常", Description = "正常")]
            Normal = 1,
            [Display(Name = "已发送", Description = "已发送")]
            Send,
            [Display(Name = "发送成功", Description = "发送成功")]
            SuccessFeedBack,
            [Display(Name = "发送异常", Description = "发送异常")]
            ErrorFeedBack,
        }
    }
}