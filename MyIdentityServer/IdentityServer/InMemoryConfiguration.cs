using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyIdentityServer.IdentityServer
{
    public class InMemoryConfiguration
    {
        public static IConfiguration Configuration { get; set; }

        /// <summary>
        /// Define which IdentityResources will use this IdentityServer
        /// ApiResources/IdentityResources资源名称不能相同，不然会报错
        /// 客户端要显示具体错误原因，设置IdentityModelEventSource.ShowPII = true;//显示IdentityServer具体错误
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResource("IdentityClient", "CAS API Service",new string[]{ "IdentityResClaim"}),
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        /// <summary>
        /// Define which APIs will use this IdentityServer
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new[]
            {
                new ApiResource("MichaelApi", "CAS API Service"),
                //new ApiResource("IdentityClient", "CAS Client Service"),
                new ApiResource("clientservice", "CAS Client Service"),
                new ApiResource("productservice", "CAS Product Service"),
                new ApiResource("agentservice", "CAS Agent Service")
            };
        }

        /// <summary>
        /// Define which Apps will use thie IdentityServer
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                //密码模式
                new Client
                {
                    ClientId = "client.api.service",
                    ClientSecrets = new [] { new Secret("clientsecret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowedScopes = new [] {
                        IdentityServerConstants.StandardScopes.Profile,//必须有 否则获取不了 Profile-扩展的claims
                        "MichaelApi" },
                    AlwaysSendClientClaims = true,
                    AllowAccessTokensViaBrowser = true,
                    AllowOfflineAccess =true,
                    //RefreshTokenUsage = TokenUsage.ReUse,//RefreshToken 使用模式 1次或重复使用
                    AccessTokenLifetime = 30,//令牌过期时间（秒）
                    //IdentityTokenLifetime=10,//身份令牌过期时间
                },
                //客户端模式
                new Client
                {
                    ClientId = "product.api.service",
                    ClientSecrets = new [] { new Secret("productsecret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ClientCredentials,//可以多个GrantType-GrantTypes.ResourceOwnerPasswordAndClientCredentials
                    AllowedScopes = new [] {
                        IdentityServerConstants.StandardScopes.Profile,//必须有 否则获取不了 Profile-扩展的claims
                        "clientservice", "productservice" },
                    AlwaysSendClientClaims = true,
                    AccessTokenLifetime = 30,//令牌过期时间（秒）
                    Claims = new Claim[]{ //默认带的Claims
                        new Claim(type:ClaimTypes.Role, value:"ApiRole", valueType:"IdsApiRole")
                    },
                    AllowedCorsOrigins= new List<string>{ { "https://localhost:44367" } },
                },
                //自定义GrantType
                new Client
                {
                    ClientId = "agent.api.service",
                    ClientSecrets = new [] { new Secret("agentsecret".Sha256()) },
                    AllowedGrantTypes = (new string[]{ "MyGrantType"}).Concat(GrantTypes.ClientCredentials).ToList(),
                    AllowedScopes = new [] { "agentservice", "clientservice", "productservice" },
                    AccessTokenLifetime = 30,//令牌过期时间（秒）
                    AlwaysSendClientClaims = true,
                    Claims = new Claim[]{//默认带的Claims
                        new Claim(type:ClaimTypes.Role, value:"ApiRole_agent", valueType:"IdsApiRole")
                    }
                },
                /*
                隐式模式
                https://localhost:6005/connect/authorize?client_id=Implicit&redirect_uri=http://localhost:5000/Home&response_type=token&scope=WebApi
                */
                new Client
                {
                    ClientId = "cas.mvc.client.implicit",
                    ClientName = "CAS MVC Web App Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    //允许登录后重定向的地址列表，可以有多个
                    RedirectUris = new string[]{"http://localhost:53624/signin-oidc","https://localhost:44366/signin-oidc" },
                    //允许注销登录后重定向的地址列表，可以有多个
                    PostLogoutRedirectUris = { $"http://localhost:53624/signout-callback-oidc" },
                    AllowedScopes = new [] {
                        IdentityServerConstants.StandardScopes.OpenId,//必须有
                        IdentityServerConstants.StandardScopes.Profile,//必须有
                        "agentservice", "clientservice", "productservice"
                    },
                    AccessTokenLifetime = 60, ////令牌过期时间（默认3600秒）
                    AllowAccessTokensViaBrowser = true, // can return access_token to this client
                    AlwaysIncludeUserClaimsInIdToken = true,//id_token&access_token 显示claims到客户端而不是需要访问IdentityServer服务端的userinfo
                },
                /*
                 * 授权码模式:
                 * https://localhost:6005/connect/authorize?client_id=GrantCode&redirect_uri=http://localhost:5000/Home&response_type=code&scope=WebApi
                 */
                new Client()
                {
                   //客户端Id
                    ClientId="GrantCode",
                    ClientName="GrantCodeClient",
                    //客户端密码
                    ClientSecrets={new Secret("CodeSecret".Sha256()) },
                    //客户端授权类型，Code:授权码模式
                    AllowedGrantTypes=GrantTypes.Code,
                    //允许登录后重定向的地址列表，可以有多个
                    RedirectUris = new string[]{"http://localhost:53624/signin-oidc","https://localhost:44366/signin-oidc" },
                    //允许访问的资源
                    AllowedScopes={
                        IdentityServerConstants.StandardScopes.OpenId,//必须有
                        IdentityServerConstants.StandardScopes.Profile,//必须有
                        "webapi",
                        "clientservice",
                        "productservice"
                    },
                    AlwaysIncludeUserClaimsInIdToken = true,//id_token&access_token 显示claims到客户端而不是需要访问IdentityServer服务端的userinfo
                },
            };
        }

        /// <summary>
        /// Define which uses will use this IdentityServer
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TestUser> GetUsers()
        {
            return new[]
            {
                new TestUser
                {
                    SubjectId = "10001",
                    Username = "edison@hotmail.com",
                    Password = "edisonpassword"
                },
                new TestUser
                {
                    SubjectId = "10002",
                    Username = "andy@hotmail.com",
                    Password = "andypassword"
                },
                new TestUser
                {
                    SubjectId = "10003",
                    Username = "leo@hotmail.com",
                    Password = "leopassword"
                }
            };
        }
    }
}
