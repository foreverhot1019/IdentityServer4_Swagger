using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyIdentityServer.DataProtection
{
    /// <summary>
    ///  数据保护XML文件
    /// </summary>
    public class XmlRepository : IXmlRepository
    {
        private readonly string _KeyContentPath = "";
        private readonly IConfiguration _Configuration;

        public XmlRepository(IConfiguration configuration)
        {
            _Configuration = configuration;
            var DataProtectionDir = _Configuration["DataProtection:DirPath"] ?? "";
            var DataProtectionFileName = _Configuration["DataProtection:FileName"] ?? "";
            var LOCALAPPDATA = Environment.GetEnvironmentVariable("LocalAppData");
            DataProtectionDir = DataProtectionDir.Replace("%LocalAppData%", LOCALAPPDATA);
            //_KeyContentPath = Path.Combine(Directory.GetCurrentDirectory(), "ShareKeys", "key.xml");
            _KeyContentPath = Path.Combine(DataProtectionDir, DataProtectionFileName);
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            Console.WriteLine(_KeyContentPath);
            Uri.TryCreate(_KeyContentPath, UriKind.Absolute, out Uri NewUri);
            var elements = new List<XElement>() { XElement.Load(NewUri.AbsolutePath) };
            return elements;
        }

        public void StoreElement(XElement element, string friendlyName)
        {

        }
    }
}
