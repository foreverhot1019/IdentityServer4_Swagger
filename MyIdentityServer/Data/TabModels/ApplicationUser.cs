using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyIdentityServer.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "所属资源", Description = "")]
        [Required, StringLength(50), DefaultValue("-")]
        public string Resource { get; set; } = "-";
    }
}
