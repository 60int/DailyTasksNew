using System.Text;

namespace DailyTasks.Forms.Classes
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

    class DailyTask
    {
        string? title;
        int? totalAmount;
        int? scrapNG;
        int? otherNG;
        int? amountLeft;
        int? ngOK;
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
                if (value >= 0)
                {
                    totalAmount = value;
                }
                else
                {
                    MessageBox.Show("Amount can only contain positive numbers!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public int? ScrapNG
        {
            get => scrapNG;
            set
            {
                if (value >= 0)
                {
                    scrapNG = value;
                }
                else
                {
                    MessageBox.Show("Amount can only contain positive numbers!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public int? OtherNG
        {
            get => otherNG;
            set
            {
                if (value >= 0)
                {
                    otherNG = value;
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

        public int? NgOK
        {
            get => ngOK;
            set
            {
                if (value >= 0)
                {
                    ngOK = value;
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

        public DailyTask(string? title, int? totalAmount, int? scrapNG, int? otherNG, int? amountLeft, int? ngOK, bool completed, DateTime? startTime, TaskType taskType, TaskPriority priority)
        {
            Title = title;
            TotalAmount = totalAmount;
            ScrapNG = scrapNG;
            OtherNG = otherNG;
            AmountLeft = amountLeft;
            NgOK = ngOK;
            Completed = completed;
            StartTime = startTime;
            TaskType = taskType;
            Priority = priority;
        }

        public override string ToString()
        {
            string yes = completed ? "✔" : "✖";
            return title! + " - " + StartTime!.Value.ToString("MM/dd") + " - " + totalAmount + " - " + yes;
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
            return $"{title},{totalAmount},{scrapNG},{otherNG},{amountLeft},{ngOK},{completed},{StartTime},{(int)taskType},{(int)priority}";
        }

        public static DailyTask[] Deserialize(string filename)
        {
            string[] lines = File.ReadAllLines(filename, Encoding.UTF8).Skip(1).ToArray();
            DailyTask[] tasks = new DailyTask[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] line = lines[i].Split(',');
                tasks[i] = new DailyTask(line[0], int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3]), int.Parse(line[4]), int.Parse(line[5]), bool.Parse(line[6]), DateTime.Parse(line[7]), (TaskType)int.Parse(line[8]), (TaskPriority)int.Parse(line[9]));
            }
            return tasks;
        }

        public static IEnumerable<(string Title, int TotalSum)> TotalSumByTitle(string filename)
        {
            DailyTask[] tasks = Deserialize(filename);
            return tasks
                .Where(t => t.StartTime.HasValue && t.StartTime.Value.Date == DateTime.Today)
                .GroupBy(t => t.Title)
                .Select(g => (g.Key, g.Sum(t => t.totalAmount) ?? 0))
                .OrderBy(g => g.Key)!;
        }

        public static IEnumerable<(string Title, int ScrapDouble)> ScrapDoubleByTitle(string filename)
        {
            DailyTask[] tasks = Deserialize(filename);
            return tasks
                .Where(t => t.StartTime.HasValue && t.StartTime.Value.Date == DateTime.Today)
                .GroupBy(t => t.Title)
                .Select(g => (g.Key, g.Sum(t => t.scrapNG) ?? 0))
                .OrderBy(g => g.Key)!;
        }

        public static IEnumerable<(string Title, int OtherNGS)> OtherNGSByTitle(string filename)
        {
            DailyTask[] tasks = Deserialize(filename);
            return tasks
                .Where(t => t.StartTime.HasValue && t.StartTime.Value.Date == DateTime.Today)
                .GroupBy(t => t.Title)
                .Select(g => (g.Key, g.Sum(t => t.otherNG) ?? 0))
                .OrderBy(g => g.Key)!;
        }

        public static IEnumerable<(string Title, int NotFinished)> NotFinishedByTitle(string filename)
        {
            DailyTask[] tasks = Deserialize(filename);
            return tasks
                .Where(t => t.StartTime.HasValue && t.StartTime.Value.Date == DateTime.Today)
                .GroupBy(t => t.Title)
                .Select(g => (g.Key, g.Sum(t => t.amountLeft) ?? 0))
                .OrderBy(g => g.Key)!;
        }

        public static IEnumerable<(string Title, int TotalNG)> TotalNGByTitle(string filename)
        {
            DailyTask[] tasks = Deserialize(filename);
            return tasks
                .Where(t => t.StartTime.HasValue && t.StartTime.Value.Date == DateTime.Today)
                .GroupBy(t => t.Title)
                .Select(g => (g.Key, g.Sum(t => t.ScrapNG + t.otherNG) ?? 0))
                .OrderBy(g => g.Key)!;
        }
        

        public static void Serialize(string filename, DailyTask[] tasks)
        {
            StreamWriter writer = new(filename, false, Encoding.UTF8);
            writer.WriteLine("Task Title, Total Amount, Scrap/Double, Other NG, Amount Left, OK/NG, Completed, Task Date Time, Task Type, Priority");
            foreach (DailyTask item in tasks)
            {
                writer.WriteLine(item.CSVFormat());
            }
            writer.Close();
        }
    }
}
