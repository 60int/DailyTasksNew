using Vip.Notification;

namespace DailyTasks.Forms
{
    public partial class MainForm : Form
    {
        readonly FileSystemWatcher watcher1 = new();
        readonly FileSystemWatcher watcher2 = new();
        readonly FileSystemWatcher watcher3 = new();

        const string MainFile = "Daily Tasks.txt";
        const string MainFolder = "Work related/";
        const string UsersFile = "Users.txt";

        public MainForm()
        {
            InitializeComponent();

            watcher1 = new FileSystemWatcher
            {
                Path = MainFile + Directory.GetCurrentDirectory(),
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = MainFile
            };
            watcher1.Changed += new FileSystemEventHandler(OnChanged);
            watcher1.EnableRaisingEvents = true;

            watcher2 = new FileSystemWatcher
            {
                Path = MainFile + Directory.GetCurrentDirectory(),
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = MainFile
            };
            watcher2.Changed += new FileSystemEventHandler(OnChanged2);
            watcher2.EnableRaisingEvents = true;

            watcher3 = new FileSystemWatcher
            {
                Path = MainFile + Directory.GetCurrentDirectory(),
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size,
                Filter = "*.jpg"
            };
            watcher3.Changed += new FileSystemEventHandler(OnChanged3);
            watcher3.EnableRaisingEvents = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(MainFile))
            {
                foreach (User item in User.Deserialize(UsersFile))
                {

                }
                foreach (Task item in Task.Deserialize(MainFile))
                {
                    MainListBox.Items.Add(item);
                    TodayAmountLabel.Text = $"Today's total: \n {Task.TotalSum(MainFile)}";
                    NotFinishedLabel.Text = $"Amount left: \n {Task.NotFinished(MainFile)}";
                }
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            TaskForm form = new();
            if (form.ShowDialog()==DialogResult.OK)
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
                    Task = (Task)MainListBox.SelectedItem
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
                    Task = (Task)MainListBox.SelectedItem
                };
                if (form.ShowDialog() == DialogResult.OK)
                {
                    MainListBox.Items[MainListBox.SelectedIndex] = MainListBox.SelectedItem!;
                    SaveChanges();
                }
            }
        }

        void SaveChanges()
        {
            Task[] tasks = new Task[MainListBox.Items.Count];
            int i = 0;
            foreach (Task item in MainListBox.Items)
            {
                tasks[i] = item;
                i++;
            }
            Task.Serialization(MainFile, tasks);
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyClipboard dialog = new();
            WindowState = FormWindowState.Minimized;
            dialog.ShowDialog();
            WindowState = FormWindowState.Normal;
        }

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditClipboardForm form = new();
            form.ShowDialog();
        }

        #region Update UI / Threading

        private delegate void UpdateLabelDelegate1(string status);

        private delegate void UpdateLabelDelegate2(string status);

        private delegate void UpdateAlertDelegate(string alert);

        private delegate void UpdateImageDelegate(string alert);

        private void UpdateLabel1(string status)
        {
            if (TodayAmountLabel.InvokeRequired)
            {
                Invoke(new UpdateLabelDelegate1(UpdateLabel1), new object[] { status });
                return;
            }

            TodayAmountLabel.Text = status;
        }
        private void UpdateLabel2(string status)
        {
            if (NotFinishedLabel.InvokeRequired)
            {
                Invoke(new UpdateLabelDelegate1(UpdateLabel2), new object[] { status });
                return;
            }

            NotFinishedLabel.Text = status;
        }
        private void UpdateAlert(string alert)
        {
            if (MainListBox.InvokeRequired)
            {
                Invoke(new UpdateAlertDelegate(UpdateAlert), new object[] { alert });
                return;
            }

            Alert.ShowInformation(alert);
        }
        private void ImageAlert(string alert)
        {
            if (SendImageLabel.InvokeRequired)
            {
                Invoke(new UpdateImageDelegate(ImageAlert), new object[] { alert });
                return;
            }

            Alert.ShowSucess(alert);
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
        private void OnChanged2(object sender, FileSystemEventArgs e)
        {
            try
            {
                watcher2.EnableRaisingEvents = false;

                Thread procThread2 = new(Process2);

                procThread2.Start();

            }
            finally
            {
                watcher2.EnableRaisingEvents = true;
            }
        }
        private void OnChanged3(object sender, FileSystemEventArgs e)
        {
            try
            {
                watcher3.EnableRaisingEvents = false;

                Thread procThread3 = new(Process3);

                procThread3.Start();
            }
            finally
            {
                watcher3.EnableRaisingEvents = true;
            }
            
        }
        // This is the actual method of the thread
        private void Process()
        {
            string label1 = $"Today's total: \n {Task.TotalSum(MainFile)}";
            string label2 = $"Amount left: \n {Task.NotFinished(MainFile)}";
            UpdateLabel1(label1);
            UpdateLabel2(label2);
        }
        private void Process2()
        {
            string alert = $"New Task Added: {Task.AlertNotification(MainFile)}";
            UpdateAlert(alert);
        }

        private void Process3()
        {
            string alert = $"{Task.AlertNotification(MainFile)} sent an image";
            ImageAlert(alert);
        }
        #endregion

        private void SendImageButton_Click(object sender, EventArgs e)
        {
            if (Clipboard.GetDataObject() != null)
            {
                IDataObject data = Clipboard.GetDataObject()!;

                if (data.GetDataPresent(DataFormats.Bitmap))
                {
                    Image image = (Image)data.GetData(DataFormats.Bitmap, true)!;

                    Bitmap bm = new(image);

                    bm.Save(MainFolder + $"{Task.AlertNotification(MainFile)} - " + $"{DateTime.Now:MMdd-HH-mm}.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                    SendImageTimer.Start();
                    SendImageLabel.Text = "Success!";
                    SendImageLabel.Visible = true;
                    GC.Collect();
                }
                else
                {
                    SendImageTimer.Start();
                    SendImageLabel.Text = "Error";
                    SendImageLabel.Visible = true;
                    MessageBox.Show("The Data In Clipboard is not as image format");
                    GC.Collect();
                }
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void SendImageTimer_Tick(object sender, EventArgs e)
        {
            SendImageTimer.Stop();
            SendImageLabel.Visible = false;
            GC.Collect();
        }
        private void FileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void NewUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddUserForm form = new();
            if (form.ShowDialog() == DialogResult.OK)
            {
                Directory.CreateDirectory(MainFile + form!.User!.UserName!);
                Directory.SetCurrentDirectory(MainFile + form!.User!.UserName!);
                SaveChanges();
            }
        }
    }
}
