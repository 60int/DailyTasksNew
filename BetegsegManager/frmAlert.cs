using DailyTasks.Notification.Properties;

namespace DailyTasks.Notification
{
    public partial class frmAlert : Form
    {
        #region Graphics

        Graphics? Canvas;
        Random? rnd;

        Effect[]? Effects;

        readonly int CloudCount = 50;
        readonly int SnowflakeCount = 300;
        readonly int GreenLeafCount = 100;

        readonly Bitmap cloud = new(Resources.SingleCloud64);
        readonly Bitmap snow = new(Resources.SingleSnowflake16);
        readonly Bitmap spring = new(Resources.SingleSpringLeaf16);
        readonly Bitmap summer = new(Resources.SingleGreenLeaf16);
        readonly Bitmap fall = new(Resources.SingleAutumnLeaf16);

        #endregion

        #region Properties

        private AlertAction _action;
        private int _interval;
        private int positionX;
        private int positionY;
        protected override bool ShowWithoutActivation => true;

        #endregion

        public frmAlert()
        {
            InitializeComponent();

            StartCloud();

            MainPictureBox.Invalidate();
            ptbLogo.Parent = MainPictureBox;
            lblMessage.Parent = MainPictureBox;
            ptbClose.Parent = MainPictureBox;
            lblMessage.BackColor = Color.Transparent;
        }

        public static FileInfo GetNewestFile(DirectoryInfo directory)
        {
            return directory.GetFiles()
                .Union(directory.GetDirectories().Select(d => GetNewestFile(d)))
                .OrderByDescending(f => (f == null ? DateTime.MinValue : f.LastWriteTime))
                .FirstOrDefault()!;
        }

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

        private void PtbClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FrmAlert_Click(object sender, EventArgs e)
        {
            timer.Interval = 1;
            _action = AlertAction.Close;
            FileInfo newestFile = GetNewestFile(new DirectoryInfo("Daily Tasks/Images/"));
            if (!File.Exists(newestFile.FullName))
            {
                return;
            }
            string argument = "/open, \"" + newestFile.FullName + "\"";
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            switch (_action)
            {
                case AlertAction.Start:
                    timer.Interval = 1;
                    Opacity += 0.1;

                    if (positionX < Location.X) Left--;
                    else if (Opacity == 1.0) _action = AlertAction.Wait;

                    break;
                case AlertAction.Wait:
                    timer.Interval = _interval;
                    _action = AlertAction.Close;
                    break;
                case AlertAction.Close:
                    timer.Interval = 1;
                    Opacity -= 0.1;
                    Left -= 3;
                    if (Opacity == 0.0) Close();
                    break;
            }
        }

        #region Methods

        internal void ShowAlert(string message, AlertType alertType, int interval, Image image = null!, Color color = default)
        {
            Opacity = 0.0;
            StartPosition = FormStartPosition.Manual;

            for (int i = 1; i < 10; i++)
            {
                var formName = "alert" + i;
                var frm = (frmAlert)Application.OpenForms[formName]!;

                if (frm == null)
                {
                    Name = formName;
                    positionX = Screen.PrimaryScreen!.WorkingArea.Width - Width + 15;
                    positionY = Screen.PrimaryScreen.WorkingArea.Height - Height * i - 5 * i;
                    Location = new Point(positionX, positionY);
                    break;
                }
            }

            positionX = Screen.PrimaryScreen!.WorkingArea.Width - Width - 5;

            switch (alertType)
            {
                case AlertType.Success:
                    ptbLogo.Image = Resources.success48px;
                    BackColor = Color.SeaGreen;
                    break;
                case AlertType.Information:
                    ptbLogo.Image = Resources.camera48px;
                    BackColor = Color.RoyalBlue;
                    break;
                case AlertType.Warning:
                    ptbLogo.Image = Resources.warning48px;
                    BackColor = Color.FromArgb(230, 126, 34);
                    break;
                case AlertType.Error:
                    ptbLogo.Image = Resources.error48px;
                    BackColor = Color.FromArgb(231, 76, 60);
                    break;
                case AlertType.Custom:
                    ptbLogo.Image = image ?? Resources.information48px;
                    BackColor = color;
                    break;
            }

            lblMessage.Text = message;
            _interval = interval;
            _action = AlertAction.Start;
            timer.Interval = 1;
            timer.Start();

            Show();
        }

        #endregion

        #region All Animations
        private void MakeCloud()
        {
            for (int i = 0; i < CloudCount; i++)
            {
                float addSpeed = 2 + (float)rnd!.NextDouble();
                Effects![i] = new Effect(rnd!.Next(-100, 550), rnd!.Next(0, 580), addSpeed, rnd!.Next(16, 64), 1, cloud);
            }
        }
        private void MakeSnow()
        {
            for (int i = 0; i < SnowflakeCount; i++)
            {
                float addSpeed = 2 + (float)rnd!.NextDouble();
                Effects![i] = new Effect(rnd!.Next(-100, 550), rnd!.Next(0, 580), addSpeed / 1.2F, rnd!.Next(8, 16), 1, snow);
            }
        }
        private void MakeSpring()
        {
            for (int i = 0; i < SnowflakeCount; i++)
            {
                float addSpeed = 2 + (float)rnd!.NextDouble() + (float)rnd!.NextDouble() + (float)rnd!.NextDouble();
                Effects![i] = new Effect(rnd!.Next(-100, 550), rnd!.Next(0, 580), addSpeed / 1.2F, rnd!.Next(8, 32), 1, spring);
            }
        }
        private void MakeSummer()
        {
            for (int i = 0; i < GreenLeafCount; i++)
            {
                float addSpeed = 2 + (float)rnd!.NextDouble() + (float)rnd!.NextDouble() + (float)rnd!.NextDouble();
                Effects![i] = new Effect(rnd!.Next(-100, 550), rnd!.Next(0, 580), addSpeed / 1.2F, rnd!.Next(8, 16), 1, summer);
            }
        }
        private void MakeAutumn()
        {
            for (int i = 0; i < SnowflakeCount; i++)
            {
                float addSpeed = 2 + (float)rnd!.NextDouble() + (float)rnd!.NextDouble() + (float)rnd!.NextDouble();
                Effects![i] = new Effect(rnd!.Next(-100, 550), rnd!.Next(0, 580), addSpeed / 1.5F, rnd!.Next(8, 16), 1, fall);
            }
        }
        private void StartSnow()
        {
            rnd = new Random();

            Bitmap myBitmap = new(Width, Height);

            MainPictureBox.Image = myBitmap;

            Canvas = Graphics.FromImage(MainPictureBox.Image);
            Effects = new Effect[SnowflakeCount];
            MakeSnow();
        }
        private void StartSpring()
        {
            rnd = new Random();

            Bitmap myBitmap = new(Width, Height);

            MainPictureBox.Image = myBitmap;

            Canvas = Graphics.FromImage(MainPictureBox.Image);
            Effects = new Effect[SnowflakeCount];
            MakeSpring();
        }
        private void StartSummer()
        {
            rnd = new Random();

            Bitmap myBitmap = new(Width, Height);

            MainPictureBox.Image = myBitmap;

            Canvas = Graphics.FromImage(MainPictureBox.Image);
            Effects = new Effect[GreenLeafCount];
            MakeSummer();
        }
        private void StartFall()
        {
            rnd = new Random();

            Bitmap myBitmap = new(Width, Height);

            MainPictureBox.Image = myBitmap;

            Canvas = Graphics.FromImage(MainPictureBox.Image);
            Effects = new Effect[SnowflakeCount];
            MakeAutumn();
        }
        private void StartCloud()
        {
            rnd = new Random();

            Bitmap myBitmap = new(Width, Height);

            MainPictureBox.Image = myBitmap;

            Canvas = Graphics.FromImage(MainPictureBox.Image);
            Effects = new Effect[CloudCount];
            MakeCloud();
        }

        #endregion

        private void TimerUpdate_Tick(object sender, EventArgs e)
        {
            MainPictureBox.Invalidate();
        }

        private void MainPictureBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Canvas!.Clear(Color.RoyalBlue);

            for (int i = 0; i < CloudCount; i++)
            {
                Canvas.DrawImage(Effects![i].Img, Effects[i].X, Effects[i].Y, Effects[i].Size, Effects[i].Size);

                Effects[i].Time += 0.1f;

                if (Effects[i].X > 700)
                {
                    Effects[i].X = -25;
                    Effects[i].Time = 0;
                }
                if (Effects[i].Y > 580 & Effects[i].Y < -5)
                {
                    Effects[i].Y = rnd!.Next(0, 580);
                }
                else
                {
                    Effects[i].X += Effects[i].Speed + rnd!.Next(-1, 0);
                }
            }
        }
    }
}

