using System.Text;
using DailyTasks.Forms.Forms;
using DailyTasks.Notification;
using DailyTasks.Forms.Classes;

namespace DailyTasks.Forms
{
    public partial class MainForm : Form
    {
        #region Initialize/Data

        FileSystemWatcher watcher2 = new();

        readonly string MainFolder = $"Daily Tasks/";
        readonly string MainFile = $"Daily Tasks/Daily Tasks.csv";
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
            FileToolStripMenuItem.ForeColor = Light;
            ClipboardToolStripMenuItem.ForeColor = Light;
            CurrentUserLabel.Text = Properties.Settings.Default.Username;
            InitializeDataGridView();
        }

        private void MainForm_Load(object sender, EventArgs e)
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
                watcher2 = new FileSystemWatcher
                {
                    Path = ImageFolder,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size,
                    Filter = "*.jpg"
                };
                watcher2.Created += new FileSystemEventHandler(OnImageCreated);
                watcher2.EnableRaisingEvents = true;
                if (File.Exists(MainFile))
                {
                    RefreshListBox();
                }

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

        void InitializeDataGridView()
        {
            MainDataGridView.AutoGenerateColumns = false;
            MainDataGridView.Columns.Add("Title", "User");
            MainDataGridView.Columns.Add("TotalSum", "Total Sum");
            MainDataGridView.Columns.Add("ScrapDouble", "Scrap/Double");
            MainDataGridView.Columns.Add("OtherNGS", "Other NG");
            MainDataGridView.Columns.Add("NotFinished", "Not Finished");
            MainDataGridView.Columns.Add("TotalOK", "Total OK");
            MainDataGridView.Columns.Add("TotalNG", "Total NG");
        }

        void RefreshListBox()
        {
            try
            {
                MainListBox.Items.Clear();
                MainDataGridView.Rows.Clear();

                var totalSumByTitle = DailyTask.TotalSumByTitle(MainFile);
                var scrapDoubleByTitle = DailyTask.ScrapDoubleByTitle(MainFile);
                var otherNGSByTitle = DailyTask.OtherNGSByTitle(MainFile);
                var notFinishedByTitle = DailyTask.NotFinishedByTitle(MainFile);
                var totalOKByTitle = DailyTask.TotalOKByTitle(MainFile);
                var totalNGByTitle = DailyTask.TotalNGByTitle(MainFile);

                foreach (var (Title, TotalSum) in totalSumByTitle)
                {
                    string title = Title;
                    int totalSum = TotalSum;
                    int scrapDouble = scrapDoubleByTitle.FirstOrDefault(g => g.Title == title).ScrapDouble;
                    int otherNGS = otherNGSByTitle.FirstOrDefault(g => g.Title == title).OtherNGS;
                    int notFinished = notFinishedByTitle.FirstOrDefault(g => g.Title == title).NotFinished;
                    int totalOK = totalOKByTitle.FirstOrDefault(g => g.Title == title).TotalOK;
                    int totalNG = totalNGByTitle.FirstOrDefault(g => g.Title == title).TotalNG;

                    MainDataGridView.Rows.Add(title, totalSum, scrapDouble, otherNGS, notFinished, totalOK, totalNG);
                }

                foreach (DailyTask item in DailyTask.Deserialize(MainFile))
                {
                    MainListBox.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("ListBox can't be refreshed at this time.", "Error", ex);
            }
        }

        private void UserItem_Click(object? sender, EventArgs e)
        {
            CurrentUserLabel.Text = (sender! as ToolStripItem)!.Text;
            Properties.Settings.Default.Username = CurrentUserLabel.Text;
            Properties.Settings.Default.Save();
        }

        private void NewUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddUserForm form = new();
            if (form.ShowDialog() == DialogResult.OK)
            {
                UsersToolStripMenuItem.DropDownItems.Add(form.User!.UserName);
                StreamWriter writer = new(UsersFile, true, Encoding.UTF8);
                writer.WriteLine(form.User.CSVFormat());
                writer.Close();
                Application.Restart();
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
                watcher2.EnableRaisingEvents = false;

                Thread procThread3 = new(Process);

                procThread3.Start();
            }
            finally
            {
                watcher2.EnableRaisingEvents = true;
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


        #endregion

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

        #region Visuals
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
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 8;  // Turn on WS_EX_TOPMOST
                return cp;
            }
        }
    }
}
