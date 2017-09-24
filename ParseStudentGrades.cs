using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using HUGradesGetter.Dto;
using System.Threading;

namespace HUGradesGetter
{
    class ParseStudentGrades
    {
        private static HttpClient client = new HttpClient();
        public static async Task<Student> ParseAsync(string departmentLink, int id, CancellationToken ct)
        {
            var k = new HtmlDocument();
            departmentLink = departmentLink.Replace("result_search", "show_result") + "&searchString={0}&id_search=Mutli_show";
            string website = string.Format(departmentLink, id);

            try
            {
                string data;
                var stream = await client.GetAsync(website, ct);
                using (var reader = new StreamReader(await stream.Content.ReadAsStreamAsync(), Encoding.GetEncoding("windows-1256")))
                    data = reader.ReadToEnd();

                Student student = new Student();
                k.LoadHtml(data);
                var mainInfoNode = k.DocumentNode.Descendants().Where(x => x.Name == "table").ElementAt(4);
                var gradesNode = k.DocumentNode.Descendants().Where(x => x.Name == "table").ElementAt(5);

                student.ID = int.Parse(mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(0).Descendants().Where(x => x.Name == "td").ElementAt(2).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", ""));
                student.Name = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(0).Descendants().Where(x => x.Name == "td").ElementAt(4).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                student.Year = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(1).Descendants().Where(x => x.Name == "td").ElementAt(0).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                student.College = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(1).Descendants().Where(x => x.Name == "td").ElementAt(2).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                student.Divison = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(2).Descendants().Where(x => x.Name == "td").ElementAt(0).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                student.Department = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(2).Descendants().Where(x => x.Name == "td").ElementAt(2).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                double ratio = 0;
                double.TryParse(mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(3).Descendants().Where(x => x.Name == "td").ElementAt(0).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "").Replace("%", ""), out ratio);
                student.Ratio = ratio;
                student.Rating = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(3).Descendants().Where(x => x.Name == "td").ElementAt(2).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "").Replace("\t", "");
                student.MoreInfo = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(4).Descendants().Where(x => x.Name == "td").ElementAt(0).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");

                int subjectsCount = gradesNode.Descendants().Where(x => x.Name == "tr").ElementAt(0).Descendants().Where(x => x.Name == "td").Count() - 1;

                for (int j = 0; j < subjectsCount; j++)
                {
                    string test = gradesNode.Descendants().Where(x => x.Name == "tr").ElementAt(0).Descendants().Where(x => x.Name == "td").ElementAt(1 + j).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "-").Replace("	", "").Replace("_", "-").Replace(" ", "-");
                    string subjectName = gradesNode.Descendants().Where(x => x.Name == "tr").ElementAt(0).Descendants().Where(x => x.Name == "td").ElementAt(1 + j).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "").Replace(" ", "");
                    double subjectGrade = 0;
                    double.TryParse(gradesNode.Descendants().Where(x => x.Name == "tr").ElementAt(1).Descendants().Where(x => x.Name == "td").ElementAt(1 + j).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", ""), out subjectGrade);
                    string subjectRate = gradesNode.Descendants().Where(x => x.Name == "tr").ElementAt(2).Descendants().Where(x => x.Name == "td").ElementAt(1 + j).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                    student.Subjects.Add(test,new Student.Subject() { Name = test, Grade = subjectGrade, Rating = subjectRate });
                }

                return student;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                return null;
            }
        }
        public static Student Parse(string departmentLink, int id, int optional = 0)
        {
            try
            {
                departmentLink = departmentLink.Replace("result_search", "show_result") + "&searchString={0}&id_search=Mutli_show";
                string website = string.Format(departmentLink, id);

                string data;
                using (var client = new WebClient())
                {
                    var stream = client.DownloadString(website);
                    using (var reader = new StreamReader(stream, Encoding.GetEncoding("windows-1256")))
                        data = reader.ReadToEnd();
                }

                Student student = new Student();
                var k = new HtmlDocument();
                k.LoadHtml(data);

                var mainInfoNode = k.DocumentNode.Descendants().Where(x => x.Name == "table").ElementAt(4);
                var gradesNode = k.DocumentNode.Descendants().Where(x => x.Name == "table").ElementAt(5);

                student.ID = short.Parse(mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(0).Descendants().Where(x => x.Name == "td").ElementAt(2).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", ""));
                student.Name = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(0).Descendants().Where(x => x.Name == "td").ElementAt(4).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                student.Year = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(1).Descendants().Where(x => x.Name == "td").ElementAt(0).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                student.College = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(1).Descendants().Where(x => x.Name == "td").ElementAt(2).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                student.Divison = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(2).Descendants().Where(x => x.Name == "td").ElementAt(0).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                student.Department = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(2).Descendants().Where(x => x.Name == "td").ElementAt(2).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                double ratio = 0;
                double.TryParse(mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(3).Descendants().Where(x => x.Name == "td").ElementAt(0).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "").Replace("%", ""), out ratio);
                student.Rating = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(3).Descendants().Where(x => x.Name == "td").ElementAt(2).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "").Replace("\t", "");
                //short totalGrades = 0;
                //short.TryParse(mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(3).Descendants().Where(x => x.Name == "td").ElementAt(4).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace(" ", "").Replace("\t", ""), out totalGrades);
                //student.TotalGrades = totalGrades;
                student.Ratio = ratio;
                student.MoreInfo = mainInfoNode.Descendants().Where(x => x.Name == "tr").ElementAt(4).Descendants().Where(x => x.Name == "td").ElementAt(0).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");

                int subjectsCount = gradesNode.Descendants().Where(x => x.Name == "tr").ElementAt(0).Descendants().Where(x => x.Name == "td").Count() - 1;

                for (int j = 0; j < subjectsCount; j++)
                {
                    string subjectName = gradesNode.Descendants().Where(x => x.Name == "tr").ElementAt(0).Descendants().Where(x => x.Name == "td").ElementAt(1 + j).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "").Replace(" ", "");
                    short subjectGrade = 0;
                    short.TryParse(gradesNode.Descendants().Where(x => x.Name == "tr").ElementAt(1).Descendants().Where(x => x.Name == "td").ElementAt(1 + j).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", ""), out subjectGrade);
                    string subjectRate = gradesNode.Descendants().Where(x => x.Name == "tr").ElementAt(2).Descendants().Where(x => x.Name == "td").ElementAt(1 + j).InnerText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("	", "");
                    student.Subjects.Add(subjectName, new Student.Subject() { Name = subjectName, Grade = subjectGrade, Rating = subjectRate });
                }

                return student;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        static void DepartmentParse(string fileName, string mainLink, int start, int end)
        {
            //Console.WriteLine("ID\tTotal\tPercentage");
            //string link = mainLink.Replace("result_search", "show_result") + "&searchString={0}&id_search=Mutli_show"
            //using (var writer = new StreamWriter(Environment.CurrentDirectory + "\\" + fileName, false))
            //{
            //    writer.Write("ID\tName\t");
            //    foreach (var test in ret[0].Subjects)
            //        writer.Write(test.Name + "\t");
            //    writer.WriteLine("Total");

            //    foreach (var student in ret.OrderByDescending(x => x.Ratio))
            //    {
            //        writer.Write(student.ID + "\t" + student.Name + "\t");
            //        foreach (var grade in student.Subjects)
            //            writer.Write(grade.Grade + "\t");
            //        writer.WriteLine(student.TotalGrades);
            //    }

            //    foreach (var student in failed.OrderBy(x => x.Subjects.Where(y => y.Rating == "غ" || y.Rating == "ض" || y.Rating == "ضج").Count()))
            //    {
            //        writer.Write(student.ID + "\t" + student.Name + "\t");
            //        foreach (var grade in student.Subjects)
            //            writer.Write(grade.Grade + "\t");
            //        writer.WriteLine(student.TotalGrades);
            //    }
            //}

            //Console.WriteLine("\nRanking saved to " + fileName + " in the same directory\n");
        }
    }
}
