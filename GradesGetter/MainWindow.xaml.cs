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
        //TODO: Fix getting the first rows while parsing departments.

        private CancellationTokenSource cts;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
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
                var list = await HUOperations.GetHUColleges("http://193.227.34.42/itchelwan/faculities.asp");
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
            if (cmb_college.SelectedIndex == -1 || cmb_sem.SelectedIndex == -1)
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
            //Console.WriteLine(this.IsEnabled);
            cmb_deps.Items.Clear();

            var entity = (Dto.Entity)cmb_college.SelectedItem;
            int sem = cmb_sem.SelectedIndex + 1;

            (await HUOperations.GetCollegeDepartments(entity, sem, cts.Token)).ForEach(x => cmb_deps.Items.Add(x));
            this.IsEnabled = true;
            MainGridBlur(false);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddDeps();
        }

        private void cmb_college_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddDeps();
        }


        private async void AddStudents(object parameter)
        {
            //Get data from parameter
            ThreadParameter par = (ThreadParameter)parameter;
            int start = par.Start;
            int end = par.End;
            int threadCustomId = par.Id + 1;

            Console.WriteLine($"From {start} to {end} #{threadCustomId}");

            Dto.Entity entity = null;
            Dispatcher.Invoke(() => entity = (Dto.Entity)cmb_deps.SelectedItem);

            for (int id = start; id <= end; id++)
            {
                var data = await AllTasks[id - ids.first];
                Dispatcher.Invoke(() =>
                {
                    if (gvc.Columns.Count == 4)
                    {
                        foreach (var item in data.Subjects)
                        {
                            gvc.Columns.Add(new GridViewColumn()
                            {
                                Header = item.Key,
                                DisplayMemberBinding = new Binding($"Subjects[{item.Key}].Grade"),
                            });
                        }
                    }

                    if (data != null && !listview_students.Items.Contains(data))
                    {
                        listview_students.Items.Add(data);
                        Students.Add(data);
                    }
                    lbl_count.Content = $"{listview_students.Items.Count}/{AllTasks.Count()}";
                });
            }
        }

        private static Task<Dto.Student>[] AllTasks = new Task<Dto.Student>[0];
        private static List<Task> tasks = new List<Task>();
        private static List<Dto.Student> Students = new List<Dto.Student>();
        private (int first, int last) ids;
        private async void cmb_deps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmb_deps.SelectedIndex == -1)
                return;

            while (gvc.Columns.Count > 4)
                gvc.Columns.RemoveAt(4);
            listview_students.Items.Clear();
            Students.Clear();

            var entity = (Dto.Entity)cmb_deps.SelectedItem;
            cts = new CancellationTokenSource();

            lbl_status.Content = "Status: Getting IDs...";
            ids = await HUOperations.GetFirstAndLastID(entity.Link);
            Console.WriteLine(ids.first + " " + ids.last);

            lbl_status.Content = "Status: Parsing IDs...";

            //var tasks = new List<Task>();
            for (int i = ids.first; i <= ids.last; i++)
            {
                tasks.Add(ParseStudentGrades.ParseAsync(entity.Link, i, cts.Token).ContinueWith(t =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (t != null)
                        {
                            listview_students.Items.Add(t.Result);
                            Students.Add(t.Result);
                        }
                        lbl_count.Content = $"{listview_students.Items.Count}/{(ids.last - ids.first + 1)}";
                    });
                }));
                await WaitList(tasks, 30);
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

        private class ThreadParameter
        {
            public int Start;
            public int End;
            public int Id;

            public ThreadParameter(int _start, int _end, int _Id)
            {
                Start = _start;
                End = _end;
                Id = _Id;
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
            MessageBox.Show("This tool was developed by © 3DProgrammer.\nProject Github Link: \nContact: iahmedmgd715@gmail.com", "About");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText("export.txt", JsonConvert.SerializeObject(Students, Formatting.Indented));
            await this.ShowMessageAsync("Success", "Exported to export.txt within the application folder!");
        }
    }
}
