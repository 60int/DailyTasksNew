namespace ToolsForOffice.Shared
{
    public class TextCell
    {
        private const int MaxLength = 24;
        private string cellType;
        private string colOne;
        private string colTwo;
        private string colThree;
        private string colFour;
        private string colFive;
        private string colSix;
        private string colSeven;
        private string colEight;

        public TextCell(string? lines)
        {
            if (lines == null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            string[] cols = lines.Split(',');
            if (cols.Length != 9)
            {
                throw new ArgumentException("Invalid number of columns", nameof(lines));
            }

            Type = cellType = cols[0];
            ColOne = colOne = cols[1];
            ColTwo = colTwo = cols[2];
            ColThree = colThree = cols[3];
            ColFour = colFour = cols[4];
            ColFive = colFive = cols[5];
            ColSix = colSix = cols[6];
            ColSeven = colSeven = cols[7];
            ColEight = colEight = cols[8];
        }

        public string Type
        {
            get => cellType;
            set => cellType = ValidateColumnValue(value, nameof(Type));
        }

        public string ColOne
        {
            get => colOne;
            set => colOne = ValidateColumnValue(value, nameof(ColOne));
        }

        public string ColTwo
        {
            get => colTwo;
            set => colTwo = ValidateColumnValue(value, nameof(ColTwo));
        }

        public string ColThree
        {
            get => colThree;
            set => colThree = ValidateColumnValue(value, nameof(ColThree));
        }

        public string ColFour
        {
            get => colFour;
            set => colFour = ValidateColumnValue(value, nameof(ColFour));
        }

        public string ColFive
        {
            get => colFive;
            set => colFive = ValidateColumnValue(value, nameof(ColFive));
        }

        public string ColSix
        {
            get => colSix;
            set => colSix = ValidateColumnValue(value, nameof(ColSix));
        }

        public string ColSeven
        {
            get => colSeven;
            set => colSeven = ValidateColumnValue(value, nameof(ColSeven));
        }

        public string ColEight
        {
            get => colEight;
            set => colEight = ValidateColumnValue(value, nameof(ColEight));
        }

        public override string ToString()
        {
            return Type;
        }

        public string ToMainLB()
        {
            return $"{Type},{ColOne},{ColTwo},{ColThree},{ColFour},{ColFive},{ColSix},{ColSeven},{ColEight}";
        }

        private static string ValidateColumnValue(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value) || value.Length > MaxLength || value.Contains(','))
            {
                throw new ArgumentException($"Column item can't be longer than {MaxLength} characters, null or contain a comma (,)", paramName);
            }
            return value;
        }
    }
}
