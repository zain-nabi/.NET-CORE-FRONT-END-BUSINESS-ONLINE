using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace Triton.BusinessOnline.Utils
{
    public class EmailHelper
    {
        public static string ResetPasswordEmail(Dictionary<string, string> template, string emailFile, IWebHostEnvironment webHostEnvironment)
        {
            string body;
            var contentRootPath = $"{webHostEnvironment.ContentRootPath}//{emailFile}";

            using (var reader = new StreamReader(contentRootPath))
            {
                body = reader.ReadToEnd();
            }

            return template.Aggregate(body, (current, item) => current.Replace($"{{{item.Key}}}", item.Value));
        }
    }
}
