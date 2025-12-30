namespace CampusLibrary
{

    public class Library
    {
        public string Name { get; set; }
        public int StudentCheckoutLimit { get; set; }
        public int CheckoutDuration { get; set; }
        public List<Item>? Items { get; set; }
        public List<User>? Users { get; set; }

        public Library(string name, int limit = 3, int duration = 14)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Name = "Library";
            }
            else
            {
                Name = name;
            }

            StudentCheckoutLimit = limit;
            CheckoutDuration = duration;
            Items = new List<Item>();
            Users = new List<User>();
        }

        public override string ToString()
        {
            return $"{Name} - Limit: {StudentCheckoutLimit} items - Duration: {CheckoutDuration} days";
        }
        
        public string ExportData()
        {
            return $"LIBRARY,\"{Name}\",{StudentCheckoutLimit},{CheckoutDuration}";
        }
    }
}