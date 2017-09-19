using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HUGradesGetter.Dto
{
    class Student
    {
        /// <summary>
        /// Student name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sitting number.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// College name.
        /// </summary>
        public string College { get; set; }

        /// <summary>
        /// College year, first/second..
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// Department.
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Divison of the department.
        /// </summary>
        public string Divison { get; set; }

        /// <summary>
        /// More info the student should notice.
        /// </summary>
        public string MoreInfo { get; set; }

        /// <summary>
        /// The sum of all subject grades.
        /// </summary>
        public double TotalGrades
        {
            get
            {
                return Subjects.Sum(x => x.Value.Grade);
            }
        }

        /// <summary>
        /// The ratio of total grades to maximum grades.
        /// </summary>
        public double Ratio { get; set; }

        /// <summary>
        /// Total rating of the subjects e.g. A, B, C, etc...
        /// </summary>
        public string Rating { get; set; }

        /// <summary>
        /// The amount of failed subjects by the student.
        /// </summary>
        public int FailedSubjectsCount
        {
            get
            {
                return Subjects.Where(x => (x.Value.Rating == "غ" || x.Value.Rating == "ض" || x.Value.Rating == "ضج")).Count();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Student)obj;

            return this.Name == other.Name && this.ID == other.ID && this.College == other.College && this.Year == other.Year &&
                this.Department == other.Department && this.Divison == other.Divison && this.MoreInfo == other.MoreInfo &&
                this.TotalGrades == other.TotalGrades && this.Ratio == other.Ratio && this.Rating == other.Rating &&
                this.Subjects.SequenceEqual(other.Subjects);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// All semester subjects.
        /// </summary>
        public Dictionary<string, Subject> Subjects { get; set; } = new Dictionary<string, Subject>();

        public class Subject
        {
            /// <summary>
            /// Subject name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Subject grade.
            /// </summary>
            public double Grade { get; set; }

            /// <summary>
            /// Subject grade rating.
            /// </summary>
            public string Rating { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                var other = (Subject)obj;

                return this.Name == other.Name && this.Grade == other.Grade && this.Rating == other.Rating;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
