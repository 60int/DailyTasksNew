using System.Text;

namespace DailyTasks.Forms
{
    enum TaskPriority
    {
        Normal,
        Important,
        Urgent
    }

    enum TaskType
    {
        StackAlignVision,
        Welding,
        ReInput,
        StackMeasure,
        CTVision,
        ScrapXRay,
        Else
    }

    class Task
    {
        string? title;
        int? totalAmount;
        int? ng;
        int? amountLeft;
        bool completed;
        DateTime? startTime;
        TaskPriority priority;
        TaskType taskType;

        public string? Title
        {
            get => title;
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Length <= 60)
                {
                    title = value;
                }
                else
                {
                    MessageBox.Show("Title can only contain alphabetic characters or numbers!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public int? TotalAmount
        {
            get => totalAmount;
            set
            {
                if (value != 0 && value > 0)
                {
                    totalAmount = value;
                }
                else
                {
                    MessageBox.Show("Amount can only contain positive numbers!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        public int? NG
        {
            get => ng;
            set
            {
                if (value >= 0)
                {
                    ng = value;
                }
                else
                {
                    MessageBox.Show("Amount can only contain positive numbers!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        public int? AmountLeft
        {
            get => amountLeft;
            set
            {
                if (value >= 0)
                {
                    amountLeft = value;
                }
                else
                {
                    MessageBox.Show("Amount can only contain positive numbers!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public bool Completed { get => completed; set => completed = value; }

        public DateTime? StartTime { get => startTime; set => startTime = value; }

        internal TaskType TaskType { get => taskType; set => taskType = value; }

        internal TaskPriority Priority { get => priority; set => priority = value; }

        public Task(string? title, int? totalAmount, int? ng, int? amountLeft, bool completed, DateTime? startTime, TaskType taskType, TaskPriority priority)
        {
            Title = title;
            TotalAmount = totalAmount;
            NG = ng; 
            AmountLeft = amountLeft;
            Completed = completed;
            StartTime = startTime;
            TaskType = taskType;
            Priority = priority;
        }

        public override string ToString()
        {
            string yes = completed? "✔" : "✖";
            return title! + " - " + StartTime!.Value.Month.ToString("00") + StartTime!.Value.Day + " - " + totalAmount + " - " + yes;
        }
        public int? ToTotal()
        {
            return totalAmount;
        }
        public string CSVFormat()
        {
            if (completed == true)
            {
                amountLeft = 0;
            }
            else if (completed == false)
            {
                amountLeft = totalAmount;
            }
            return $"{title},{totalAmount},{ng},{amountLeft},{completed},{StartTime},{(int)taskType},{(int)priority}";
        }
        public static Task[] Deserialize(string filename)
        {
            string[] lines = File.ReadAllLines(filename, Encoding.UTF8).Skip(1).ToArray();
            Task[] tasks = new Task[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] line = lines[i].Split(',');
                tasks[i] = new Task(line[0], int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3]), bool.Parse(line[4]), DateTime.Parse(line[5]), (TaskType)int.Parse(line[6]), (TaskPriority)int.Parse(line[7]));
            }
            return tasks;
        }
        public static int? TotalSum(string filename)
        {
            string[] lines = File.ReadAllLines(filename, Encoding.UTF8).Skip(1).ToArray();
            Task[] tasks = new Task[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] line = lines[i].Split(',');
                tasks[i] = new Task(line[0], int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3]), bool.Parse(line[4]), DateTime.Parse(line[5]), (TaskType)int.Parse(line[6]), (TaskPriority)int.Parse(line[7]));
            }
            return tasks.Where(a =>a.StartTime!.Value.Day == DateTime.Today.Day).Sum(a => a.totalAmount);
        }
        public static int? NotFinished(string filename)
        {
            string[] lines = File.ReadAllLines(filename, Encoding.UTF8).Skip(1).ToArray();
            Task[] tasks = new Task[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] line = lines[i].Split(',');
                tasks[i] = new Task(line[0], int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3]), bool.Parse(line[4]), DateTime.Parse(line[5]), (TaskType)int.Parse(line[6]), (TaskPriority)int.Parse(line[7]));
            }
            return tasks.Where(a => a.StartTime!.Value.Day == DateTime.Today.Day).Sum(a => a.amountLeft);
        }
        public static string? TotalNG(string filename)
        {
            string[] lines = File.ReadAllLines(filename, Encoding.UTF8).Skip(1).ToArray();
            Task[] tasks = new Task[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] line = lines[i].Split(',');
                tasks[i] = new Task(line[0], int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3]), bool.Parse(line[4]), DateTime.Parse(line[5]), (TaskType)int.Parse(line[6]), (TaskPriority)int.Parse(line[7]));
            }
            var tasksList = tasks.Where(a => a.StartTime!.Value.Day == DateTime.Today.Day).ToList();
            var totalAmount = tasksList.Sum(a => a.totalAmount);
            var ng = tasksList.Sum(a => a.ng);
            return totalAmount - ng + "/" + ng;
        }
        public static void Serialize(string filename, Task[] tasks)
        {
            StreamWriter writer = new(filename, false, Encoding.UTF8);
            writer.WriteLine("Task Title, Total Amount, NG, Amount Left, Completed, Task Date Time, Task Type, Priority");
            foreach (Task item in tasks)
            {
                writer.WriteLine(item.CSVFormat());
            }
            writer.Close();
        }
    }
}
