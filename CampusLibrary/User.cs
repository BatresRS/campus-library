namespace CampusLibrary
{

    public class User
    {
        private static int _nextId = 1;
        public int Id { get; set; }
        public string Name { get; set; }
        private string Username { get; set; }
        private string Password { get; set; }
        private double Balance { get; set; }
        public List<Item> CheckedOutItems { get; set; }

        public User(string name, string username, string password, int id = 0, double balance = 0)
        {
            // ID and Balance will only be manually set when loading from csv
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
            Username = username;
            Password = password;
            Balance = balance;
            CheckedOutItems = new List<Item>();
            
        }

        public void IncrementId()
        {
            _nextId++;
        }

        public string ExportData()
        {
            // next id gets overwritten to one higher than the last loaded id
            return $"USER,{Id},\"{Name}\",\"{Username}\",\"{Password}\",{Balance}";
        }
        
        public override string ToString()
        {
            return $"{Id} - {Name} - ${Balance} - {CheckedOutItems.Count} items";
        }

        public double CalculateLateFee(int checkOutLength = 14)
        {
            double fees = 0;
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            foreach (var item in CheckedOutItems)
            {
                fees += item.CalculateLateFee(checkOutLength);
            }
            return fees;
        }
        
        public int Authenticate(string username, string password)
        {
            if (Username == username && Password == password) return Id;
            return 0;
        }
        
        public static bool UniqueUsername(string username, List<User> users)
        {
            // This is how I would do a lookup without the example I referenced from stackoverflow
            foreach (var user in users)
            {
                if (user.Username == username)
                {
                    return false;
                }
            }
            return true;
        }
        
        public double AddBalance(double amount = 0)
        {
            Balance += amount;
            return Balance;
        }
    }
}