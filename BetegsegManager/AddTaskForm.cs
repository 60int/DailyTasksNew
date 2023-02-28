namespace DailyTasks.Forms
{
    public partial class TaskForm : Form
    {
        Task? task;
        internal Task? Task
        {
            get => task;
            set
            {
                task = value;
                textBox1.Text = task!.Title;
                MainNumericUpDown.Value = (int)task.TotalAmount!;
                AmountLeftNumberUD.Value = (int)task.AmountLeft!;
                CompletedCheckBox.Checked = task.Completed;
                MainDateTimePicker.Value = (DateTime)task.StartTime!;
                comboBox1.SelectedIndex = (int)task.TaskType;
                comboBox2.SelectedIndex = (int)task.Priority;
            }
        }
        public TaskForm()
        {
            InitializeComponent();
            comboBox1.DataSource = Enum.GetValues(typeof(TaskType));
            comboBox2.DataSource = Enum.GetValues(typeof(TaskPriority));
            MaximizeBox = false;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (task == null)
            {
                if (textBox1.Text.Length >= 3)
                {
                    task = new Task(textBox1.Text, (int)MainNumericUpDown.Value, (int)AmountLeftNumberUD.Value, CompletedCheckBox.Checked, MainDateTimePicker.Value, (TaskType)comboBox1.SelectedIndex, (TaskPriority)comboBox2.SelectedIndex);
                }
                else
                {
                    MessageBox.Show("The title must be 3 or less characters long!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox1.Focus();
                    DialogResult = DialogResult.None;
                }
            }
            else
            {
                task.Title = textBox1.Text;
                task.TotalAmount = (int)MainNumericUpDown.Value;
                task.AmountLeft = (int)AmountLeftNumberUD.Value;
                task.Completed = CompletedCheckBox.Checked;
                task.StartTime = MainDateTimePicker.Value;
                task.TaskType = (TaskType)comboBox1.SelectedIndex;
                task.Priority = (TaskPriority)comboBox2.SelectedIndex;
            }
        }
    }
}
