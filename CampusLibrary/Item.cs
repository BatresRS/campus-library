namespace CampusLibrary
{
    public class Item
    {
        private static int _nextId = 1;
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public double Fee { get; set; }
        public int CheckedOutBy { get; set; }
        public DateOnly CheckedOutOn { get; set; }

        public Item(string name, string type, int id = 0, double fee = 0.5, int user = 0, DateOnly checkedOutOn = default)
        {
            // ID will only be manually set when loading from csv
            if (id == 0)
            {
                Id = _nextId++;
            }
            else
            {
                Id = id;
                _nextId = id + 1;
            }

            Name = name;
            Type = type;
            Fee = fee;
            // 0 is available, -1 marks it as deleted
            // Deleted items will not be loaded back from csv
            CheckedOutBy = user;
            CheckedOutOn = checkedOutOn;
        }

        public void IncrementId()
        {
            _nextId++;
        }

        public string ExportData()
        {
            // next id gets overwritten to one higher than the last loaded id
            return $"ITEM,{Id},\"{Name}\",\"{Type}\",{CheckedOutBy},{Fee},{CheckedOutOn}";
        }

        public bool IsAvailable()
        {
            return CheckedOutBy == 0;
        }

        public override string ToString()
        {
            if (IsAvailable()) return $"{Id} - {Name} ({Type}) - Available";
            if (CheckedOutBy == -1) return "Item Unavailable";
            return $"{Id} - {Name} ({Type}) - Checked Out";
        }
        
        public string ToStringFull()
        {
            if (IsAvailable()) return $"{Id} - {Name} ({Type}) - Available - ${Fee}";
            if (CheckedOutBy==-1) return $"{Id} - {Name} ({Type}) - Marked for Deletion - ${Fee}";
            return $"{Id} - {Name} ({Type}) - {CheckedOutBy} ({CheckedOutOn}) - ${Fee}";
        }
        
        public void CheckOut(int userId, DateOnly date = default)
        {
            // Can be checked in by setting userId to 0
            // Also can be marked as deleted by setting userId to -1
            CheckedOutBy = userId;
            if (date == default) date = DateOnly.FromDateTime(DateTime.Now);
            CheckedOutOn = date;
        }
        
        public int CalculateLateDays(int checkOutLength = 14)
        {
            if (CheckedOutBy == 0 || CheckedOutBy == -1) return 0;
            var dueDate = CheckedOutOn.AddDays(checkOutLength);
            var today = DateOnly.FromDateTime(DateTime.Now);
            if (today > dueDate)
            {
                return (today.DayNumber - dueDate.DayNumber);
            }
            else
            {
                return 0;
            }
        }
        
        public double CalculateLateFee(int checkOutLength = 14)
        {
            var lateDays = CalculateLateDays(checkOutLength);
            return lateDays * Fee;
        }
    }
}