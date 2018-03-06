using HtmlAgilityPack;
using HUGradesGetter.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HUGradesGetter.NewSite
{
    class SiteOperations
    {
        public static async Task<List<Entity>> GetHUColleges(string link)
        {
            link = FixLink(link);
            var retList = new List<Entity>();
            try
            {
                string data;
                using (var client = new HttpClient())
                {
                    var stream = await client.GetStreamAsync(link);
                    using (var reader = new StreamReader(stream, Encoding.GetEncoding("windows-1256")))
                        data = reader.ReadToEnd();
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(data);
            }
            catch { }
            return retList;
        }


        public static string FixLink(string link)
        {
            if (link.StartsWith("http://"))
                return link;
            else
                return "http://" + link;
        }
    }
}
