using DailyTasks.Forms.Classes;
using DailyTasks.Forms.Forms;
using DailyTasks.Notification;
using System.Text;

namespace DailyTasks.Forms
{
    public partial class MainForm : Form
    {
        #region Initialize/Data

        FileSystemWatcher watcher1 = new();
        FileSystemWatcher watcher2 = new();

        readonly string MainFolder = $"Daily Tasks/";
        readonly string MainFile = $"Daily Tasks/Daily Tasks.csv";
        readonly string ImageFolder = $"Daily Tasks/Images/";
        readonly string UsersFile = $"Daily Tasks/Users.txt";

        readonly Color Dark = Color.Black;
        readonly Color Light = Color.White;

        #endregion

        public MainForm()
        {
            InitializeComponent();
            FileToolStripMenuItem.ForeColor = Light;
            ClipboardToolStripMenuItem.ForeColor = Light;
            CurrentUserLabel.Text = Properties.Settings.Default.Username;
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
                watcher1 = new FileSystemWatcher
                {
                    Path = MainFolder,
                    NotifyFilter = NotifyFilters.LastWrite,
                    Filter = "*.csv"
                };
                watcher1.Changed += new FileSystemEventHandler(OnChanged);
                watcher1.EnableRaisingEvents = true;

                if (File.Exists(UsersFile))
                {
                    int i = 1;
                    foreach (User userItem in User.Deserialize(UsersFile))
                    {
                        ToolStripItem item = UsersToolStripMenuItem.DropDownItems.Add(userItem.UserName);
                        item.Click += UserItem_Click;
                        Label label = (Label)this.Controls.Find("UserLabel" + i, true)[0];
                        label.Text = userItem.UserName;
                        i++;
                        if (i > 6) break;
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
                MessageBox.Show("Application couldn't start", "Error" + ex, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        void RefreshListBox()
        {
            try
            {
                MainListBox.Items.Clear();
                foreach (DailyTask item in DailyTask.Deserialize(MainFile))
                {
                    MainListBox.Items.Add(item);
                    TodayAmountLabel1.Text = $"Today's total: \n {DailyTask.TotalSum(MainFile)}";
                    ScrapDoubleLabel1.Text = $"Scrap/Double: \n {DailyTask.ScrapDouble(MainFile)}";
                    AmountLeftLabel1.Text = $"Amount left: \n   {DailyTask.NotFinished(MainFile)}";
                    NGLabel1.Text = $"OK/NG:\n {DailyTask.TotalNG(MainFile)}";
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
                }
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (MainListBox.SelectedIndex != -1 && MessageBox.Show("Do you want to delete this item?", "Delete item?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                MainListBox.Items.RemoveAt(MainListBox.SelectedIndex);
                SaveChanges();
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
                }
            }
        }

        private void OpenCBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyClipboard dialog = new();
            Hide();
            dialog.ShowDialog();
            Show();
        }
        #endregion

        #region Update UI / Threading

        private delegate void UpdateLabelDelegate1(string status);

        private delegate void UpdateLabelDelegate2(string status);

        private delegate void UpdateLabelDelegate3(string status);

        private delegate void UpdateImageDelegate(string alert);

        private void UpdateLabel1(string status)
        {
            if (TodayAmountLabel1.InvokeRequired)
            {
                Invoke(new UpdateLabelDelegate1(UpdateLabel1), new object[] { status });
                return;
            }

            TodayAmountLabel1.Text = status;
        }
        private void UpdateLabel2(string status)
        {
            if (AmountLeftLabel1.InvokeRequired)
            {
                Invoke(new UpdateLabelDelegate2(UpdateLabel2), new object[] { status });
                return;
            }

            AmountLeftLabel1.Text = status;
        }
        private void UpdateLabel3(string status)
        {
            if (AmountLeftLabel1.InvokeRequired)
            {
                Invoke(new UpdateLabelDelegate3(UpdateLabel3), new object[] { status });
                return;
            }

            NGLabel1.Text = status;
        }
        private void ImageAlert(string alert)
        {
            if (ShareImageLabel.InvokeRequired)
            {
                Invoke(new UpdateImageDelegate(ImageAlert), new object[] { alert });
                return;
            }

            Alert.ShowInformation(alert, 5000);
        }
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                watcher1.EnableRaisingEvents = false;

                Thread procThread = new(Process);

                procThread.Start();
            }
            finally
            {
                watcher1.EnableRaisingEvents = true;
            }
        }
        private void OnImageCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                watcher2.EnableRaisingEvents = false;

                Thread procThread3 = new(Process3);

                procThread3.Start();
            }
            finally
            {
                watcher2.EnableRaisingEvents = true;
            }

        }

        // This is the actual method of the thread
        private void Process()
        {
            string label1 = $"Today's total: \n {DailyTask.TotalSum(MainFile)}";
            string label2 = $"Amount left: \n {DailyTask.NotFinished(MainFile)}";
            string label3 = $"OK/NG: \n {DailyTask.TotalNG(MainFile)}";
            UpdateLabel1(label1);
            UpdateLabel2(label2);
            UpdateLabel3(label3);
        }
        public static FileInfo GetNewestFile(DirectoryInfo directory)
        {
            return directory.GetFiles()!
                .Union(directory.GetDirectories().Select(d => GetNewestFile(d)))
                .OrderByDescending(f => (f == null ? DateTime.MinValue : f.LastWriteTime))
                .FirstOrDefault()!;
        }
        private void Process3()
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

    }
}
