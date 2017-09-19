using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HUGradesGetter
{
    class HUOperations
    {
        public static async Task<(int first, int last)> GetFirstAndLastID(string departmentLink)
        {
            int start = 0, end = 0;
            try
            {
                string website = departmentLink.Replace("result_search", "found_multi");
                if (!website.StartsWith("http://"))
                    website = "http://" + website;
                string data;
                using (var client = new HttpClient())
                {
                    var stream = await client.GetStreamAsync(website);
                    using (var reader = new StreamReader(stream, Encoding.GetEncoding("windows-1256")))
                        data = reader.ReadToEnd();
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(data);

                var studentsCountTable = doc.DocumentNode.Descendants().Where(x => x.Name == "table").ElementAt(2);
                var studentsTable = doc.DocumentNode.Descendants().Where(x => x.Name == "table").ElementAt(3);

                start = int.Parse(studentsTable.Descendants().Where(x => x.Name == "tr").ElementAt(1).Descendants().Where(x => x.Name == "td").ElementAt(1).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", ""));

                string textWithCount = studentsCountTable.Descendants().Where(x => x.Name == "tr").ElementAt(0).Descendants().Where(x => x.Name == "td").ElementAt(1).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                string studentsCount = "";
                foreach (var t in textWithCount)
                {
                    if (char.IsDigit(t))
                    {
                        studentsCount += t;
                    }
                }

                end = start + int.Parse(studentsCount) - 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.Source);
            }
            return (start, end);
        }
        public static async Task<List<Dto.Entity>> GetHUColleges(string mainWebsiteLink)
        {
            if (!mainWebsiteLink.StartsWith("http://"))
                mainWebsiteLink = "http://" + mainWebsiteLink;
            var retList = new List<Dto.Entity>();
            try
            {
                string data;
                using (var client = new HttpClient())
                {
                    var stream = await client.GetStreamAsync(mainWebsiteLink);
                    using (var reader = new StreamReader(stream, Encoding.GetEncoding("windows-1256")))
                        data = reader.ReadToEnd();
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(data);

                var colleges = doc.DocumentNode.Descendants("td").Where(x => x.GetAttributeValue("Width", "") == "619").ElementAt(1);
                foreach (var node in colleges.Descendants("td"))
                {
                    if (node.Descendants().Where(x => x.Name == "a").Count() == 1)
                    {
                        string collegeLink = node.Descendants("a").ElementAt(0).GetAttributeValue("href", "");
                        string collegeName = ParseString(node.InnerText);
                        if (!collegeLink.StartsWith("http"))
                            collegeLink = "http://193.227.34.42/itchelwan/" + collegeLink;
                        retList.Add(new Dto.Entity() { Link = collegeLink, Name = collegeName });
                    }
                    else
                    {
                        foreach (var node2 in node.Descendants().Where(x => x.Name == "a"))
                        {
                            string collegeLink = node2.GetAttributeValue("href", "");
                            string collegeName = ParseString(node2.InnerText);
                            if (string.IsNullOrEmpty(collegeName))
                                continue;
                            if (!collegeLink.StartsWith("http"))
                                collegeLink = "http://193.227.34.42/itchelwan/" + collegeLink;
                            retList.Add(new Dto.Entity() { Link = collegeLink, Name = collegeName });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            return retList;
        }
        public static async Task<List<Dto.Entity>> GetCollegeDepartments(Dto.Entity college, int semester, CancellationToken ct)
        {
            var retList = new List<Dto.Entity>();
            try
            {

                string website = college.Link + "&sem=" + semester;
                if (!website.StartsWith("http://"))
                    website = "http://" + website;
                string data;
                using (var client = new HttpClient())
                {
                    var stream = await client.GetAsync(website, ct);
                    using (var reader = new StreamReader(await stream.Content.ReadAsStreamAsync(), Encoding.GetEncoding("windows-1256")))
                        data = reader.ReadToEnd();
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(data);

                var node = doc.DocumentNode.Descendants().Where(x => x.GetAttributeValue("id", "") == "AutoNumber2").ElementAt(1);
                int trCount = node.Descendants("tr").Count();

                string currentType = college.Name;

                for (int i = 1; i < trCount; i++)
                {
                    int tdCount = node.Descendants("tr").ElementAt(i).Descendants("td").Count();
                    var el = node.Descendants("tr").ElementAt(i);
                    if (tdCount == 1)
                        currentType = ParseString(el.InnerText);
                    else if (tdCount == 2) //TODO fix first years without type..
                    {
                        int hrefCount = el.Descendants().ToList().Where(x => x.Name == "a").Count();
                        if (hrefCount == 0) continue;
                        int imgCount = el.Descendants().ToList().Where(x => x.Name == "img").Count();
                        string depLink = el.Descendants("a").ElementAt(0).GetAttributeValue("href", "");
                        string yearName = ParseString(el.Descendants("td").ElementAt(1).InnerText);
                        string depName = currentType + " - " + yearName;
                        if (!depLink.StartsWith("http"))
                            depLink = "http://193.227.34.42/itchelwan/" + depLink;

                        if (hrefCount == 1)
                        {
                            if (imgCount == 2) //Incase of 1 link but (Entzam & Entsab)
                            {
                                if (depLink.Contains("state=1"))
                                    depName = currentType + " - انتظـام - " + yearName;
                                else
                                    depName = currentType + " - انتســاب - " + yearName;
                            }
                        } else if(hrefCount == 2)
                        {
                            depName = currentType + " - انتظـام - " + yearName;
                            retList.Add(new Dto.Entity() { Name = depName, Link = depLink });
                            depName = currentType + " - انتســاب - " + yearName;
                            depLink = depLink.Replace("state=1", "state=2").Replace("x", "s");
                        }
                        retList.Add(new Dto.Entity() { Name = depName, Link = depLink });
                    }
                    else
                    {
                        //Clipboard.SetText(el.OuterHtml);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.GetBaseException());
            }
            return retList;
        }
        private static string ParseString(string data)
        {
            string k = "";
            while (data.Contains("&nbsp;&nbsp;"))
                data = data.Replace("&nbsp;&nbsp;", "&nbsp;");
            data.Replace("&nbsp;", " ").ToList().ForEach(y =>
            {
                if (char.IsLetter(y) || y == ' ' || y == ' ')
                    k += y.ToString();
            });
            return k;
        }
    }
}
