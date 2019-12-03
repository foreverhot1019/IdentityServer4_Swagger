using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyAuthApi.Controllers
{
    /// <summary>
    /// SwaggerTest测试
    /// <remark>
    /// Swagger测试
    /// 展示接口名称参数
    /// 以及展示测试数据
    /// </remark>
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SwaggerTestController : ControllerBase
    {
        /// <summary>
        ///  获取测试数据
        /// </summary>
        /// <returns>Json返回值</returns>
        [HttpGet()]
        public async Task<IActionResult> SwaggerTest(Guid Id, string StrVal)
        {
            MySwaggerModel mySwaggerModel = new MySwaggerModel { Id = Id, StrVal = StrVal };
            mySwaggerModel.IntVal = 1;
            mySwaggerModel.TEnumVal = TEnum.TEnum3;
            return await Task.FromResult<JsonResult>(new JsonResult(mySwaggerModel));
        }
        
        /// <summary>
         /// Swagger测试带ModeldeAction 
         /// </summary>
         /// <remarks>
         /// Note that the key is a GUID and not an integer.
         ///  
         ///     POST /Todo
         ///     {
         ///        "key": "0e7ad584-7788-4ab1-95a6-ca0a5b444cbb",
         ///        "name": "Item1",
         ///        "isComplete": true
         ///     }
         /// 
         /// </remarks>
         /// <param name="mySwaggerModel">Model类参数</param>
         /// <returns>Json返回值{"Success":true,"ErrMsg":""}</returns>
         /// <response code="200">{"Success":true,"ErrMsg":""}</response>
         /// <response code="400">{"errors":{}}</response>
        [HttpPost]
        [Authorize]
        [Route("Test")]
        public async Task<IActionResult> SwaggerTest([FromBody, Required]MySwaggerModel mySwaggerModel)
        {
            return await Task.FromResult<JsonResult>(new JsonResult(new { Success = true, ErrMsg = "SwaggerTest->Test" }));
        }
    }

    /// <summary>
    /// TEnum枚举
    /// </summary>
    public enum TEnum
    {
        /// <summary>
        /// TEnum枚举1Des
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Name = "TEnum枚举1")]
        TEnum1 = 1,
        /// <summary>
        /// TEnum枚举2Des
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Name = "TEnum枚举2")]
        TEnum2,
        /// <summary>
        /// TEnum枚举3Des
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Name = "TEnum枚举3")]
        TEnum3
    }
    /// <summary>
    /// Swagger-Model 测试
    /// </summary>
    public class MySwaggerModel
    {
        /// <summary>
        /// Guid主键Des
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Name = "主键")]
        public Guid Id { get; set; }

        /// <summary>
        /// String值Des
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Name = "String值")]
        [System.ComponentModel.DataAnnotations.MaxLength(10)]
        public string StrVal { get; set; }

        /// <summary>
        /// Int32值Des
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Name = "Int32值")]
        [System.ComponentModel.DataAnnotations.Range(1, 100)]
        public int IntVal { get; set; }

        /// <summary>
        /// 枚举值Des
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Name = "枚举值")]
        public TEnum TEnumVal { get; set; }
    }
}