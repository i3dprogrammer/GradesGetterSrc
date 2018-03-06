using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HUGradesGetter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {   //This is so badly implemented, uses LOTS of rams.
        //But who gives a fuck? As long as it works, that's fine with me.
        private CancellationTokenSource cts;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //http://app.helwan.edu.eg/Nat_W/Natglist.asp?x_st_settingno=14 Get link from here.
            try
            {
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    await this.ShowMessageAsync("Error", "This program requires internet connection to function properly.. Closing");
                    return;
                }
                AddColleges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }

        private async void AddColleges()
        {
            try
            {
                MainGridBlur(true);
                //var list = await HUOperations.GetHUColleges("http://193.227.34.42/itchelwan/faculities.asp");
                var list = new List<Dto.Entity>()
                {
                    new Dto.Entity(){Name = "كلية العلوم", Link="http://app.helwan.edu.eg/Nat_W"} //Actually get the colleges instead of this xD?
                };

                foreach (var item in list)
                {
                    cmb_college.Items.Add(item);
                }
                MainGridBlur(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }

        private async void AddDeps()
        {
            int dep_id_start = -1, dep_id_end = -1;
            Int32.TryParse(id_start.Text, out dep_id_start);
            Int32.TryParse(id_end.Text, out dep_id_end);
            if (cmb_college.SelectedIndex == -1 || dep_id_end == -1 || dep_id_start == -1)
            {
                return;
            }

            if (cts != null)
            {
                cts.Cancel();
            }

            cts = new CancellationTokenSource();

            MainGridBlur(true);
            this.IsEnabled = false;

            var entity = (Dto.Entity)cmb_college.SelectedItem;
            this.IsEnabled = true;
            MainGridBlur(false);
        }

        private void cmb_college_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddDeps();
        }

        private static Task<Dto.Student>[] AllTasks = new Task<Dto.Student>[0];
        private static List<Task> tasks = new List<Task>();
        private static List<Dto.Student> Students = new List<Dto.Student>();
        private (int first, int last) ids;
        private async void StartProcess()
        {
            ids.first = -1;
            ids.last = -1;
            Int32.TryParse(id_start.Text, out ids.first);
            Int32.TryParse(id_end.Text, out ids.last);

            if (cmb_college.SelectedIndex == -1 || ids.first == -1 || ids.last == -1)
            {
                lbl_status.Content = "Status: error occurred!";
                return;
            }

            if (cts != null)
            {
                cts.Cancel();
            }

            //// Remove subject columns
            //while (gvc.Columns.Count > 4)
            //    gvc.Columns.RemoveAt(4);

            listview_students.Items.Clear();
            Students.Clear();

            var entity = (Dto.Entity)cmb_college.SelectedItem;
            cts = new CancellationTokenSource();

            lbl_status.Content = "Status: Parsing IDs...";

            //var tasks = new List<Task>();
            for (int i = ids.first; i <= ids.last; i++)
            {
                lbl_status.Content = $"Status: Parsing id {i}";
                tasks.Add(NewSite.ParseStudentGrades.ParseStudentFromSetting(entity.Link, i, cts.Token).ContinueWith(t =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (t != null && t.Result != null)
                        {
                            listview_students.Items.Add(t.Result);
                            Students.Add(t.Result);
                        }
                        lbl_count.Content = $"{listview_students.Items.Count}/{(ids.last - ids.first + 1)}";
                    });
                }));
                await WaitList(tasks, 10);
            }

            await Task.WhenAll(tasks);
            lbl_status.Content = "Status: Completed.";
        }

        private async Task WaitList(IList<Task> _tasks, int maxSize)
        {
            while (_tasks.Count > maxSize)
            {
                var completed = await Task.WhenAny(_tasks).ConfigureAwait(false);
                _tasks.Remove(completed);
            }
        }

        private void MainGridBlur(bool blur)
        {
            if (blur == true)
            {
                BlurBitmapEffect effect = new BlurBitmapEffect();
                effect.Radius = 10;
                effect.KernelType = KernelType.Gaussian;
                mainGrid.BitmapEffect = effect;
                progressGrid.Visibility = Visibility.Visible;
            }
            else
            {
                mainGrid.BitmapEffect = null;
                progressGrid.Visibility = Visibility.Hidden;
            }

        }
        private void cmb_sort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview_students.Items.Count == 0)
                return;

            listview_students.Items.SortDescriptions.Clear();

            string varName = "Name";
            ListSortDirection dir = ListSortDirection.Ascending;

            if (cmb_sort.SelectedIndex == 0) //Add
            {
                varName = "FailedSubjectsCount";
                dir = ListSortDirection.Ascending;
                listview_students.Items.SortDescriptions.Add(new SortDescription(varName, dir));
                varName = "TotalGrades";
                dir = ListSortDirection.Descending;
                listview_students.Items.SortDescriptions.Add(new SortDescription(varName, dir));
                varName = "Id";
                dir = ListSortDirection.Ascending;
            }
            else if (cmb_sort.SelectedIndex == 1)
            {
                varName = "TotalGrades";
                dir = ListSortDirection.Descending;
            }
            else if (cmb_sort.SelectedIndex == 3)
            {
                dir = ListSortDirection.Descending;
            }

            listview_students.Items.SortDescriptions.Add(new SortDescription(varName, dir));
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("This tool was developed by © 3DProgrammer.\nProject Github Link: https://github.com/i3dprogrammer/GradesGetterSrc \nContact: iahmedmgd715@gmail.com", "About");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<string> students = Students.OrderByDescending(x => x.TotalGrades).Select(x => x?.Name + "\t" + x?.TotalGrades);
            //File.WriteAllText("export.txt", JsonConvert.SerializeObject(Students, Formatting.Indented));
            File.WriteAllLines("export.txt", students);
            await this.ShowMessageAsync("Success", "Exported to export.txt within the application folder!");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            StartProcess();
        }
    }
}
