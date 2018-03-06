using HtmlAgilityPack;
using HUGradesGetter.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HUGradesGetter.NewSite
{
    class ParseStudentGrades
    {
        private static HttpClient client = new HttpClient();
        public static async Task<Student> ParseStudentFromSetting(string depLink, int id, CancellationToken ct)
        {
            Console.WriteLine("wtf");
            var k = new HtmlDocument();
            var parseLink = depLink + "/Natglist.asp?x_st_settingno={0}";
            string website = string.Format(parseLink, id);
            try
            {
                string data;
                var stream = await client.GetAsync(website, ct);
                using (var reader = new StreamReader(await stream.Content.ReadAsStreamAsync(), Encoding.GetEncoding("windows-1256")))
                    data = reader.ReadToEnd();
                k.LoadHtml(data);

                Student student = new Student();

                var resultLink = k.DocumentNode.Descendants().First(x => x.Name == "a" && x.InnerText == "عرض النتيجة").GetAttributeValue("href", "");
                resultLink = depLink + "/" + resultLink;
                Console.WriteLine(resultLink);

                return await ParseAsync(resultLink, id, ct);
            }
            catch {
                return null;
            }
        }

        private static async Task<Student> ParseAsync(string departmentLink, int id, CancellationToken ct)
        {
            var k = new HtmlDocument();
            try
            {
                string data;
                var stream = await client.GetAsync(departmentLink, ct);
                using (var reader = new StreamReader(await stream.Content.ReadAsStreamAsync(), Encoding.GetEncoding("windows-1256")))
                    data = reader.ReadToEnd();

                Student student = new Student();
                k.LoadHtml(data);

                var mainInfoNode = k.DocumentNode.Descendants().Where(x => x.Name == "table").ElementAt(1);
                var gradesNode = k.DocumentNode.Descendants().Where(x => x.Name == "table").ElementAt(3);

                student.Name = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(2).Descendants().Where(x => x.Name == "td").ElementAt(0).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                student.Divison = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(2).Descendants().Where(x => x.Name == "td").ElementAt(1).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                student.College = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(2).Descendants().Where(x => x.Name == "td").ElementAt(2).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                student.Department = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(2).Descendants().Where(x => x.Name == "td").ElementAt(3).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                student.Year = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(3).Descendants().Where(x => x.Name == "td").ElementAt(0).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                student.ID = id;

                int subjectsCount = gradesNode.Descendants().Where(x => x.Name == "tr").ElementAt(0).Descendants().Where(x => x.Name == "td").Count() - 1;

                for (int j = 0; j < subjectsCount; j++)
                {
                    string subjectName = gradesNode.Descendants().Where(x => x.Name == "tr").ElementAt(0).Descendants().Where(x => x.Name == "td").ElementAt(1 + j).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "-").Replace("	", "").Replace("_", "-").Replace(" ", "-");
                    double subjectGrade = 0;
                    double.TryParse(gradesNode.Descendants().Where(x => x.Name == "tr").ElementAt(1).Descendants().Where(x => x.Name == "td").ElementAt(1 + j).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", ""), out subjectGrade);
                    string subjectRate = gradesNode.Descendants().Where(x => x.Name == "tr").ElementAt(2).Descendants().Where(x => x.Name == "td").ElementAt(1 + j).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                    student.Subjects.Add(subjectName, new Student.Subject() { Name = subjectName, Grade = subjectGrade, Rating = subjectRate });
                }

                return student;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                return null;
            }
        }
    }
}
