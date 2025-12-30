namespace CampusLibrary
{

    public static class Menu
    {
        public static void Startup()
        {
            List<string> files = Csv.GetFileNames();
            if (files.Count == 0)
            {
                Console.WriteLine("No CSV files found in the current directory.");
                return;
            }
            Console.WriteLine("Available CSV files:");
            for (var i = 0; i < files.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {files[i]}");
            }
        }

        public static void PreMenu()
        {
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("1. Edit Items");
            Console.WriteLine("2. View Users");
            Console.WriteLine("3. Continue to User Menu");
            Console.WriteLine("4. Exit and Save");
        }
        
        public static void UserPreMenu()
        {
            Console.WriteLine("User Menu:");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. Exit");
        }

        public static void UserMenu()
        {
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("1. View Available Items");
            Console.WriteLine("2. View Checked Out Items");
            Console.WriteLine("3. View Balance");
            Console.WriteLine("4. Sign Out");
        }

        public static void ListUsers(List<User> users)
        {
            if (users.Count == 0)
            {
                Console.WriteLine("No users found.");
                return;
            }
            Console.WriteLine("Registered Users:");
            foreach (var user in users)
            {
                Console.WriteLine(user.ToString());
            }
        }
        
        public static void ListItems(List<Item> items, bool availableOnly = false, bool fullDetails = false)
        {
            if (items.Count == 0)
            {
                Console.WriteLine("No items found.");
                return;
            }
            Console.WriteLine("Items:");
            foreach (var item in items)
            {
                if (availableOnly && !item.IsAvailable()) continue;
                if (!fullDetails) Console.WriteLine(item.ToString());
                else Console.WriteLine(item.ToStringFull());
            }
        }
        
        public static void EditItemMenu(List<Item> items)
        {
            ListItems(items, false, true);
            Console.WriteLine("Edit Item Menu:");
            Console.WriteLine("1. Add Item");
            Console.WriteLine("2. Remove Item");
            Console.WriteLine("3. Back to Main Menu");
        }
    }
}