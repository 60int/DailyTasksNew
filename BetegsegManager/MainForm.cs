using System.Text;
using DailyTasks.Forms.Forms;
using DailyTasks.Notification;
using DailyTasks.Forms.Classes;
using System.Runtime.InteropServices;

namespace DailyTasks.Forms
{
    public partial class MainForm : Form
    {
        #region Initialize/Data

        FileSystemWatcher watcher = new();

        readonly string MainFolder = $"Daily Tasks/";
        readonly string MainFile = $"Daily Tasks/Daily Tasks - {Properties.Settings.Default.Username}.csv";
        readonly string ImageFolder = $"Daily Tasks/Images/";
        readonly string UsersFile = $"Daily Tasks/Users.txt";

        readonly Color Dark = Color.Black;
        readonly Color Light = Color.White;

        #endregion

        /*TODO: Home Page with Weather
            using System.Net;
            using Newtonsoft.Json.Linq;

            string apiKey = "YOUR_API_KEY";
            string location = "Budapest";
            string url = $"http://api.openweathermap.org/data/2.5/weather?q={location}&appid={apiKey}";

            using (WebClient client = new WebClient())
            {
                string json = client.DownloadString(url);
                JObject data = JObject.Parse(json);

                // Access data
                double temperature = (double)data["main"]["temp"];
                string description = (string)data["weather"][0]["description"];
            }
         */

        public MainForm()
        {
            InitializeComponent();
            InitializeDataGridView();
            FileToolStripMenuItem.ForeColor = Light;
            ClipboardToolStripMenuItem.ForeColor = Light;
            CurrentUserLabel.Text = Properties.Settings.Default.Username;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    string[] directories = { MainFolder, ImageFolder };
                    foreach (string directory in directories)
                    {
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while creating directories: " + ex.Message);
                }

                CreateFileSystemWatcher();

                RefreshListBox();

                if (File.Exists(UsersFile))
                {
                    foreach (User userItem in User.Deserialize(UsersFile))
                    {
                        ToolStripItem item = UsersToolStripMenuItem.DropDownItems.Add(userItem.UserName);
                        item.Click += UserItem_Click;
                    }
                }
                else
                {
                    switch (MessageBox.Show("Users file is corrupted or doesn't exist! Do you want to create a new one?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Information))
                    {
                        case DialogResult.Yes:
                            using (FileStream fs = File.Create(UsersFile))
                            {
                                char[] value = "Default\n".ToCharArray();
                                fs.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);
                            }
                            MessageBox.Show("File created!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Application.Restart();
                            break;
                        case DialogResult.Cancel:
                            Environment.Exit(0);
                            break;
                        case DialogResult.No:
                            Environment.Exit(0);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Application couldn't start" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        List<DailyTask> LoadDataFromCsvFiles()
        {
            List<DailyTask> allTasks = new();

            if (CurrentUserLabel.Text == "Default")
            {
                // Load data from multiple CSV files
                string[] filePaths = Directory.GetFiles(MainFolder, "*.csv");

                foreach (string filePath in filePaths)
                {
                    allTasks.AddRange(DailyTask.Deserialize(filePath));
                }
            }
            else
            {
                // Check if the file exists
                if (!File.Exists(MainFile))
                {
                    // Create the file if it doesn't exist
                    using FileStream fs = File.Create(MainFile);
                }

                // Load data from a single CSV file
                allTasks.AddRange(DailyTask.Deserialize(MainFile));
            }

            return allTasks;
        }

        private void CreateFileSystemWatcher()
        {
            watcher = new FileSystemWatcher
            {
                Path = ImageFolder,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size,
                Filter = "*.jpg"
            };
            watcher.Created += new FileSystemEventHandler(OnImageCreated);
            watcher.EnableRaisingEvents = true;
        }

        void InitializeDataGridView()
        {
            MainDataGridView.AutoGenerateColumns = false;
            MainDataGridView.Columns.Add("Day of the Week", "Day Of Week");
            MainDataGridView.Columns.Add("Title", "User");
            MainDataGridView.Columns.Add("TotalSum", "Total Sum");
            MainDataGridView.Columns.Add("NotFinished", "Not Finished");
            MainDataGridView.Columns.Add("TotalOK", "Total OK");
            MainDataGridView.Columns.Add("TotalNG", "Total NG");
            MainDataGridView.Columns.Add("Task Type", "TaskType");
        }

        void RefreshListBox()
        {
            // Clear MainDataGridView on first working day of the week
            if (DateTime.Today.DayOfWeek == DayOfWeek.Monday)
            {
                MainDataGridView.Rows.Clear();
            }

            MainListBox.Items.Clear();
            MainDataGridView.Rows.Clear();

            // Load data from CSV files
            List<DailyTask> allTasks = LoadDataFromCsvFiles();

            // Calculate total sum, not finished, total OK and total NG by title
            var totalSumByTitle = DailyTask.TotalSumByTitle(allTasks.ToArray());
            var notFinishedByTitle = DailyTask.NotFinishedByTitle(allTasks.ToArray());
            var totalOKByTitle = DailyTask.TotalOKByTitle(allTasks.ToArray());
            var totalNGByTitle = DailyTask.TotalNGByTitle(allTasks.ToArray());

            // Group tasks by date and title
            var tasksGroupedByDateAndTitle = allTasks.GroupBy(t => new { t.StartTime!.Value.Date, t.Title });

            // Add a row for each group
            foreach (var group in tasksGroupedByDateAndTitle)
            {
                string date = group.Key.Date.ToString("d");
                string title = group.Key.Title!;
                int totalSum = group.Sum(t => t.TotalAmount) ?? 0;
                int notFinished = group.Sum(t => t.AmountLeft) ?? 0;
                int totalOK = group.Sum(t => t.TotalAmount - (t.ScrapNG + t.OtherNG)) ?? 0;
                int totalNG = group.Sum(t => t.ScrapNG + t.OtherNG) ?? 0;
                string taskType = group.First().TaskType.ToString();

                MainDataGridView.Rows.Add(date, title, totalSum, notFinished, totalOK, totalNG, taskType);
            }

            foreach (DailyTask item in allTasks)
            {
                MainListBox.Items.Add(item);
            }
        }

        private void UserItem_Click(object? sender, EventArgs e)
        {
            CurrentUserLabel.Text = (sender! as ToolStripItem)!.Text;
            Properties.Settings.Default.Username = CurrentUserLabel.Text;
            Properties.Settings.Default.Save();

            RefreshListBox();
            Application.Restart();
        }

        private void NewUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddUserForm form = new();
            if (form.ShowDialog() == DialogResult.OK)
            {
                // Create a new ToolStripItem for the new user
                ToolStripItem newUserItem = new ToolStripMenuItem(form.User!.UserName);

                // Attach the UserItem_Click event handler to the new ToolStripItem
                newUserItem.Click += UserItem_Click;

                // Add the new ToolStripItem to the UsersToolStripMenuItem
                UsersToolStripMenuItem.DropDownItems.Add(newUserItem);

                using (StreamWriter writer = new(UsersFile, true, Encoding.UTF8))
                {
                    writer.WriteLine(form.User.CSVFormat());
                }

                // No need to restart the application
            }
        }

        private void MainDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            // Get the selected date
            DateTime selectedDate = MainDateTimePicker.Value;

            MainListBox.Items.Clear();
            MainDataGridView.Rows.Clear();

            List<DailyTask> allTasks = new();
            if (CurrentUserLabel.Text == "Default")
            {
                // Load data from multiple CSV files
                string dailyTasksDirectory = Path.Combine(MainFolder, "Daily Tasks");
                string[] filePaths = Directory.GetFiles(MainFolder, "*.csv");
                foreach (string filePath in filePaths)
                {
                    allTasks.AddRange(DailyTask.Deserialize(filePath));
                }
            }
            else
            {
                // Check if the file exists
                if (!File.Exists(MainFile))
                {
                    // Create the file if it doesn't exist
                    using FileStream fs = File.Create(MainFile);
                }

                // Load data from a single CSV file
                allTasks.AddRange(DailyTask.Deserialize(MainFile));
            }

            var totalSumByTitle = DailyTask.TotalSumByTitle(allTasks.ToArray());
            var notFinishedByTitle = DailyTask.NotFinishedByTitle(allTasks.ToArray());
            var totalOKByTitle = DailyTask.TotalOKByTitle(allTasks.ToArray());
            var totalNGByTitle = DailyTask.TotalNGByTitle(allTasks.ToArray());

            // Filter tasks by selected date
            var tasksBySelectedDate = allTasks.Where(t => t.StartTime.HasValue && t.StartTime.Value.Date == selectedDate.Date);

            // Group tasks by date and title
            var tasksGroupedByDateAndTitle = tasksBySelectedDate.GroupBy(t => new { t.StartTime!.Value.Date, t.Title });

            // Add a row for each group
            foreach (var group in tasksGroupedByDateAndTitle)
            {
                string date = group.Key.Date.ToString("d");
                string title = group.Key.Title!;
                int totalSum = group.Sum(t => t.TotalAmount) ?? 0;
                int notFinished = group.Sum(t => t.AmountLeft) ?? 0;
                int totalOK = group.Sum(t => t.TotalAmount - (t.ScrapNG + t.OtherNG)) ?? 0;
                int totalNG = group.Sum(t => t.ScrapNG + t.OtherNG) ?? 0;
                string taskType = group.First().TaskType.ToString();

                MainDataGridView.Rows.Add(date, title, totalSum, notFinished, totalOK, totalNG, taskType);
            }

            foreach (DailyTask item in allTasks)
            {
                MainListBox.Items.Add(item);
            }
        }

        void SaveChanges()
        {
            DailyTask[] tasks = new DailyTask[MainListBox.Items.Count];
            int i = 0;
            foreach (DailyTask item in MainListBox.Items)
            {
                tasks[i] = item;
                i++;
            }
            DailyTask.Serialize(MainFile, tasks);
        }

        #region Main Buttons
        private void AddButton_Click(object sender, EventArgs e)
        {
            TaskForm form = new();
            if (form.ShowDialog() == DialogResult.OK)
            {
                MainListBox.Items.Add(form.Task!);
                SaveChanges();
                RefreshListBox();
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (MainListBox.SelectedItem != null)
            {
                TaskForm form = new()
                {
                    Task = (DailyTask)MainListBox.SelectedItem
                };
                if (form.ShowDialog() == DialogResult.OK)
                {
                    MainListBox.Items[MainListBox.SelectedIndex] = MainListBox.SelectedItem!;
                    SaveChanges();
                    RefreshListBox();
                }
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (MainListBox.SelectedIndex != -1 && MessageBox.Show("Do you want to delete this item?", "Delete item?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                MainListBox.Items.RemoveAt(MainListBox.SelectedIndex);
                SaveChanges();
                RefreshListBox();
            }
        }

        private void MainListBox_DoubleClick(object sender, EventArgs e)
        {
            if (MainListBox.SelectedItem != null)
            {
                TaskForm form = new()
                {
                    Task = (DailyTask)MainListBox.SelectedItem
                };
                if (form.ShowDialog() == DialogResult.OK)
                {
                    MainListBox.Items[MainListBox.SelectedIndex] = MainListBox.SelectedItem!;
                    SaveChanges();
                    RefreshListBox();
                }
            }
        }

        private void OpenCBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyClipboard dialog = new();
            WindowState = FormWindowState.Minimized;
            dialog.Owner = this;
            dialog.ShowDialog();
            WindowState = FormWindowState.Normal;
        }
        #endregion

        #region Share Image Function / Threading

        private delegate void UpdateImageDelegate(string alert);

        private void ImageAlert(string alert)
        {
            if (ShareImageLabel.InvokeRequired)
            {
                Invoke(new UpdateImageDelegate(ImageAlert), new object[] { alert });
                return;
            }

            Alert.ShowInformation(alert, 5000);
        }

        private void OnImageCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                watcher.EnableRaisingEvents = false;

                Thread procThread3 = new(Process);

                procThread3.Start();
            }
            finally
            {
                watcher.EnableRaisingEvents = true;
            }

        }
        public static FileInfo GetNewestFile(DirectoryInfo directory)
        {
            return directory.GetFiles()!
                .Union(directory.GetDirectories().Select(d => GetNewestFile(d)))
                .OrderByDescending(f => (f == null ? DateTime.MinValue : f.LastWriteTime))
                .FirstOrDefault()!;
        }

        private void Process()
        {
            FileInfo newestFile = GetNewestFile(new DirectoryInfo("Daily Tasks/Images/"));
            if (!File.Exists(newestFile.FullName))
            {
                return;
            }
            string alert = $"{newestFile.Name.Split(" - ").FirstOrDefault()!} sent an image";
            ImageAlert(alert);
        }

        private void ShareImageButton_Click(object sender, EventArgs e)
        {
            if (Clipboard.GetDataObject() != null)
            {
                IDataObject data = Clipboard.GetDataObject()!;

                if (data.GetDataPresent(DataFormats.Bitmap))
                {
                    Image image = (Image)data.GetData(DataFormats.Bitmap, true)!;

                    Bitmap bm = new(image);

                    bm.Save(ImageFolder + $"{CurrentUserLabel.Text} - " + $"{DateTime.Now:MMdd-HH-mm-ss}.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                    ShareImageTimer.Start();
                    ShareImageLabel.Text = "Success!";
                    ShareImageLabel.Visible = true;
                    GC.Collect();
                }
                else
                {
                    ShareImageTimer.Start();
                    ShareImageLabel.Text = "Error";
                    ShareImageLabel.Visible = true;
                    MessageBox.Show("The Data In Clipboard is not as image format");
                    GC.Collect();
                }
            }
        }

        #endregion

        #region Visuals

        private void ShowAllButton_Click(object sender, EventArgs e)
        {
            RefreshListBox();
        }

        private void ShareImageTimer_Tick(object sender, EventArgs e)
        {
            ShareImageTimer.Stop();
            ShareImageLabel.Visible = false;
            GC.Collect();
        }

        private void EditCBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditClipboard form = new();
            form.ShowDialog();
        }

        private void FileToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            FileToolStripMenuItem.ForeColor = Dark;
            ClipboardToolStripMenuItem.ForeColor = Dark;
        }

        private void FileToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            FileToolStripMenuItem.ForeColor = Light;
            ClipboardToolStripMenuItem.ForeColor = Light;
        }

        private void ClipboardToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            FileToolStripMenuItem.ForeColor = Dark;
            ClipboardToolStripMenuItem.ForeColor = Dark;
        }

        private void ClipboardToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            FileToolStripMenuItem.ForeColor = Light;
            ClipboardToolStripMenuItem.ForeColor = Light;
        }

        #endregion

        //Keep window on top always

        #region Window always on top function

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOSIZE = 0x0001;

        private void AlwaysOnTopCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (AlwaysOnTopCheckBox.Checked)
            {
                // Turn on WS_EX_TOPMOST
                SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }
            else
            {
                // Turn off WS_EX_TOPMOST
                SetWindowPos(Handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }
        }
        #endregion

    }
}
