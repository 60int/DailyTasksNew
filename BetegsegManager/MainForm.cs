using System.Text;
using Vip.Notification;
using System.Runtime.InteropServices;

namespace DailyTasks.Forms
{
    public partial class MainForm : Form
    {
        #region Initialize/Data

        FileSystemWatcher watcher1 = new();
        FileSystemWatcher watcher2 = new();

        readonly string MainFolder = $"Daily Tasks/";
        readonly string DefaultFolder = $"Daily Tasks/Default/";
        readonly string MainFile = $"Daily Tasks/Daily Tasks.csv";
        readonly string ImageFolder = $"Daily Tasks/Images/";
        readonly string UsersFile = $"Daily Tasks/Users.txt";

        readonly Color Dark = Color.Black;
        readonly Color Light = Color.White;

        #endregion

        public MainForm()
        {
            InitializeComponent();
            new DropShadow().ApplyShadows(this);
            FileToolStripMenuItem.ForeColor = Light;
            ClipboardToolStripMenuItem.ForeColor = Light;
            UserLabel.Text = Properties.Settings.Default.Username;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Ugly code

            if (!Directory.Exists(MainFolder))
            {
                Directory.CreateDirectory(MainFolder);
            }
            if (!Directory.Exists(ImageFolder))
            {
                Directory.CreateDirectory(ImageFolder);
            }
            
            try
            {
                Directory.CreateDirectory(DefaultFolder);

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
                MessageBox.Show("Application couldn't start", "Error" + ex, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        void RefreshListBox()
        {
            try
            {
                MainListBox.Items.Clear();
                foreach (Task item in Task.Deserialize(MainFile))
                {
                    MainListBox.Items.Add(item);
                    TodayAmountLabel.Text = $"Today's total: \n {Task.TotalSum(MainFile)}";
                    NotFinishedLabel.Text = $"Amount left: \n {Task.NotFinished(MainFile)}";
                    NGLabel.Text = $"OK/NG:\n {Task.TotalNG(MainFile)}";
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("ListBox can't be refreshed at this time.", "Error", ex);
            }
        }

        private void UserItem_Click(object? sender, EventArgs e)
        {
            UserLabel.Text = (sender! as ToolStripItem)!.Text;
            Properties.Settings.Default.Username = UserLabel.Text;
            Properties.Settings.Default.Save();
            if (!Directory.Exists(MainFolder + (sender! as ToolStripItem)!.Text))
            {
                Directory.CreateDirectory(MainFolder + (sender! as ToolStripItem)!.Text);
            }
            
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
            Task[] tasks = new Task[MainListBox.Items.Count];
            int i = 0;
            foreach (Task item in MainListBox.Items)
            {
                tasks[i] = item;
                i++;
            }
            Task.Serialize(MainFile, tasks);
        }

        #region Main Buttons

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

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyClipboard dialog = new();
            Hide();
            dialog.ShowDialog();
            Application.Restart();
        }

        #endregion

        #region Update UI / Threading

        private delegate void UpdateLabelDelegate1(string status);

        private delegate void UpdateLabelDelegate2(string status);

        private delegate void UpdateLabelDelegate3(string status);

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
                Invoke(new UpdateLabelDelegate2(UpdateLabel2), new object[] { status });
                return;
            }

            NotFinishedLabel.Text = status;
        }
        private void UpdateLabel3(string status)
        {
            if (NotFinishedLabel.InvokeRequired)
            {
                Invoke(new UpdateLabelDelegate3(UpdateLabel3), new object[] { status });
                return;
            }

            NGLabel.Text = status;
        }
        private void ImageAlert(string alert)
        {
            if (SendImageLabel.InvokeRequired)
            {
                Invoke(new UpdateImageDelegate(ImageAlert), new object[] { alert });
                return;
            }

            Alert.ShowInformation(alert, 3000);
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
            string label1 = $"Today's total: \n {Task.TotalSum(MainFile)}";
            string label2 = $"Amount left: \n {Task.NotFinished(MainFile)}";
            string label3 = $"OK/NG: \n {Task.TotalNG(MainFile)}";
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

        private void SendImageButton_Click(object sender, EventArgs e)
        {
            if (Clipboard.GetDataObject() != null)
            {
                IDataObject data = Clipboard.GetDataObject()!;

                if (data.GetDataPresent(DataFormats.Bitmap))
                {
                    Image image = (Image)data.GetData(DataFormats.Bitmap, true)!;

                    Bitmap bm = new(image);

                    bm.Save(ImageFolder + $"{UserLabel.Text} - " + $"{DateTime.Now:MMdd-HH-mm-ss}.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

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

        #region Visuals

        private void MinimizeButton_MouseHover(object sender, EventArgs e)
        {
            MinimizeButton.ForeColor = Dark;
        }

        private void MinimizeButton_MouseLeave(object sender, EventArgs e)
        {
            MinimizeButton.ForeColor = Color.White;
        }

        private void SendImageTimer_Tick(object sender, EventArgs e)
        {
            SendImageTimer.Stop();
            SendImageLabel.Visible = false;
            GC.Collect();
        }

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditClipboardForm form = new();
            form.ShowDialog();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
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
