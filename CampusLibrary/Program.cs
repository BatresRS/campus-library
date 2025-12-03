/*
 I tried to expand off the previous menu by adding a basic 'User' system with login functionality.
 Each student has their own inventory and account. There is a very basic login system too.
 I originally planned to use Sqlite for data storage but ran out of time, more info at the bottom.
 */

using System.Globalization;
using System.Linq.Expressions;
//using Microsoft.Data.Sqlite;

Library library;
var fileList = LoadFileNames();
var dbFileName = "";
if (fileList.Count == 0)
{
    Console.WriteLine("No CSV files found. Please create a library.");
    library = LibraryBuilder();
}
else
{
    Console.WriteLine("Available CSV files:");
    for (int i = 0; i < fileList.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {fileList[i]}");
    }

    Console.Write("What number file would you like to load? (0 to create new): ");
    int fileChoice;
    while (true)
    {
        try
        {
            fileChoice = Convert.ToInt32(Console.ReadLine());
            if (fileChoice == 0)
            {
                Console.WriteLine("Creating new library.");
                library = LibraryBuilder();
                dbFileName = "";
                break;
            }
            if (fileChoice < 1 || fileChoice > fileList.Count)
            {
                Console.Write("Invalid choice. Please enter a valid number: ");
                continue;
            }
        }
        catch
        {
            Console.Write("Invalid input. Please enter a number: ");
            continue;
        }
        break;
    }
    dbFileName = fileList[fileChoice - 1] + ".csv";
    library = LoadLibraryFromCsv(dbFileName);
    Console.WriteLine($"Loaded library: {library.Name}");
}

int choice = PreMenu();
while (choice != 4)
{
    switch (choice)
    {
        case 1:
            // Create Library Items
            library = CreateLibraryItem(library);
            break;
        case 2:
            // Save Library
            Console.WriteLine("Enter a name for the CSV file (without extension):");
            string? fileNameInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(fileNameInput))
            {
                Console.WriteLine("Cancelling save.");
                break;
            }
            SaveLibraryToCsv(library, dbFileName);
            break;
        case 3:
            // View Library
            ShowLibraryMenu(library);
            break;
        case 5:
            // Exit
            Environment.Exit(0);
            break;
    }
    choice = PreMenu();
}

Console.Clear();
int loggedInStudentId = -1;






//test for csv in/out
static bool SaveLibraryToCsv(Library library, string fileName)
{
    try
    {
        using (var writer = new StreamWriter(fileName))
        {
            // Write library info
            writer.WriteLine(library.CsvFormat());
            // Write items
            foreach (var item in library.Items)
            {
                writer.WriteLine(item.CsvFormat());
            }
            // Write students
            foreach (var student in library.Students)
            {
                writer.WriteLine(student.CsvFormat());
            }
            // Write checked out items
            foreach (var checkOut in library.CheckOutItems)
            {
                writer.WriteLine(checkOut.CsvFormat());
            }
        }
        return true;
    }
    catch
    {
        return false;
    }
}


static Library LoadLibraryFromCsv(string fileName)
{
    var library = new Library();
    var itemDict = new Dictionary<int, LibraryItem>();
    var studentDict = new Dictionary<int, Student>();

    var lines = File.ReadAllLines(fileName);
    foreach (var line in lines)
    {
        var parts = line.Split(',');
        switch (parts[0])
        {
            case "Library":
                library.Name = parts[1].Trim('"');
                library.MaxStudentItems = int.Parse(parts[2]);
                break;
            case "Item":
                var item = new LibraryItem(
                    parts[2].Trim('"'),
                    parts[3].Trim('"'),
                    string.IsNullOrEmpty(parts[4]) ? null : DateOnly.Parse(parts[4]),
                    double.Parse(parts[5]),
                    int.Parse(parts[1])
                );
                itemDict[item.Id] = item;
                library.Items.Add(item);
                break;
            case "Student":
                var student = new Student(
                    parts[2].Trim('"'),
                    parts[3].Trim('"'),
                    parts[4].Trim('"'),
                    int.Parse(parts[1])
                )
                {
                    Balance = double.Parse(parts[5])
                };
                studentDict[student.Id] = student;
                library.Students.Add(student);
                break;
            case "CheckOut":
                var checkOutItem = new CheckOutItem(
                    int.Parse(parts[1]),
                    itemDict[int.Parse(parts[2])],
                    DateOnly.Parse(parts[3]),
                    DateOnly.Parse(parts[4])
                );
                if (!string.IsNullOrEmpty(parts[5]))
                {
                    checkOutItem.CheckIn(DateOnly.Parse(parts[5]));
                }
                library.CheckOutItems.Add(checkOutItem);
                studentDict[int.Parse(parts[1])].AddCheckedOutItem(checkOutItem);
                break;
        }
    }

    // finish up
    Console.WriteLine("Load complete. Finishing setup...");
    LibraryItem.SetIdAutoIncrementPointer(itemDict.Keys.Max() + 1);
    Student.SetIdAutoIncrementPointer(studentDict.Keys.Max() + 1);
    // Students rely on their checked out items being in a list within the student object
    // Go through each checked out item and add it to the student's list
    foreach (var checkOut in library.CheckOutItems)
    {
        if (studentDict.TryGetValue(checkOut.StudentId, out var student))
        {
            student.AddCheckedOutItem(checkOut);
        }
    }
    return library;
}











// Student side
StudentLoginProcess();
// Logged in
while (true)
{
    choice = StudentMenu();
    // Loads the student
    var student = library.Students.First(s => s.Id == loggedInStudentId);
    switch (choice)
    {
        case 1:
            // View Checked Out Items
            Console.WriteLine("Your Checked Out Items:");
            Console.WriteLine("ID - Title (Type) - Year - Late Fee - Due Date - Late Days");
            foreach (var checkOut in student.GetCheckedOutItems())
            {
                checkOut.PrintReciept();
            }

            Console.WriteLine($"Estimated total late fees: {student.GetCheckedOutItems().Sum(c => c.CalculateLateFee(DateOnly.FromDateTime(DateTime.Now))):C}");

            break;
        case 2:
            // Check Out Item
            var availableSlots = student.CheckOutSlotsLeft(library.MaxStudentItems);
            if (availableSlots == 0)
            {
                Console.WriteLine("You cannot check out any more items.");
                break;
            }

            Console.WriteLine($"You can check out {availableSlots} more item(s).");
            Console.WriteLine("Available Items:");
            foreach (var item in library.Items)
            {
                if (item.GetAvailability())
                {
                    Console.WriteLine(item.ToString());
                }
            }

            Console.WriteLine("Enter the ID of the item you want to check out:");
            string? checkOutItemInput = Console.ReadLine();
            if (!int.TryParse(checkOutItemInput, out int checkOutItemId))
            {
                Console.WriteLine("Invalid input, cancelling.");
                break;
            }

            var itemToCheckOut = library.Items.FirstOrDefault(i => i.Id == checkOutItemId);
            if (itemToCheckOut == null || !itemToCheckOut.GetAvailability())
            {
                Console.WriteLine("Item not found or not available.");
                break;
            }
            var checkOutDate = DateOnly.FromDateTime(DateTime.Now);
            var dueDate = checkOutDate.AddDays(14); // 2 weeks due date
            var checkOutItem = new CheckOutItem(student.Id, itemToCheckOut, checkOutDate, dueDate);
            if (checkOutItem.CheckOut(itemToCheckOut))
            {
                library.CheckOutItems.Add(checkOutItem);
                student.AddCheckedOutItem(checkOutItem);
                Console.WriteLine("Item checked out successfully.");
                break;
            }

            Console.WriteLine("Failed to check out item.");
            break;
        case 3:
            // Return Item, late fee added when returned
            Console.WriteLine("Checked Out Items:");
            foreach (var item in student.GetCheckedOutItems())
            {
                Console.WriteLine(item.GetItem().ToString());
            }
            Console.WriteLine("Enter the ID of the item you want to return:");
            int returnItemId;
            try
            {
                returnItemId = Convert.ToInt32(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("Invalid input, cancelling.");
                break; 
            }
            if (returnItemId == 0)
            {
                Console.WriteLine("Cancelling return.");
                break; 
            }
            var checkOutToReturn = student.GetCheckedOutItems().FirstOrDefault(c => c.GetItem().Id == returnItemId);
            if (checkOutToReturn == null)
            {
                Console.WriteLine("Item not found in your checked out items. Cancelling.");
                break;
            }

            if (checkOutToReturn.CheckIn(DateOnly.FromDateTime(DateTime.Now)))
            {
                student.RemoveCheckedOutItem(checkOutToReturn);
                library.CheckOutItems.Remove(checkOutToReturn);
                double lateFee = checkOutToReturn.CalculateLateFee(DateOnly.FromDateTime(DateTime.Now));
                if (lateFee > 0)
                {
                    student.AdjustBalance(-lateFee);
                    Console.WriteLine($"Item returned successfully. Late fee of {lateFee:C} applied to your account.");
                }
                else
                {
                    Console.WriteLine("Item returned successfully. No late fee applied.");
                }
            }
            break;
        case 4:
            // View or Pay Balance
            Console.WriteLine($"Your current balance is: {student.Balance:C}");
            Console.WriteLine("Would you like to make a payment? (Y/n)");
            string paymentChoice = Console.ReadLine() ?? "n";
            if (paymentChoice.ToLower() == "y")
            {
                Console.WriteLine("Enter the amount you would like to pay:");
                try
                {
                    double paymentAmount = Convert.ToDouble(Console.ReadLine());
                    if (paymentAmount <= 0)
                    {
                        Console.WriteLine("Payment amount must be positive.");
                        break;
                    }

                    student.AdjustBalance(paymentAmount);
                    Console.WriteLine($"Payment of {paymentAmount:C} applied. New balance is {student.Balance:C}");
                }
                catch
                {
                    Console.WriteLine("Invalid input, cancelling payment.");
                }
            }
            break;
        case 5:
            // Logout
            Console.WriteLine("Are you sure you want to logout? (Y/n)");
            string logoutChoice = Console.ReadLine() ?? "n";
            if (logoutChoice.ToLower() == "y")
            {
                loggedInStudentId = -1;
                Console.WriteLine("Logged out successfully.");
            }

            StudentLoginProcess();
            break;
    }
        
}


// Functions

void StudentLoginProcess()
{
    while (loggedInStudentId == -1)
    {
        int loginChoice = LoginMenu();
        switch (loginChoice)
        {
            case 1:
                // Login
                Console.Write("Enter your email: ");
                string email = Console.ReadLine() ?? "";
                Console.Write("Enter your password: ");
                string password = Console.ReadLine() ?? "";
                int studentId = StudentLogin(library, email, password);
                if (studentId != -1)
                {
                    loggedInStudentId = studentId;
                    Console.WriteLine("Login successful.");
                }
                else
                {
                    Console.WriteLine("Login failed. Please try again.");
                }
                break;
            case 2:
                // Sign Up
                Console.Write("Enter your name: ");
                string name = Console.ReadLine() ?? "";
                Console.Write("Enter your email: ");
                string signUpEmail = Console.ReadLine() ?? "";
                Console.Write("Enter your password: ");
                string signUpPassword = Console.ReadLine() ?? "";
                Console.Write("Confirm your password: ");
                string confirmPassword = Console.ReadLine() ?? "";
                // make sure email isn't already used
                if (library.Students.Any(s => s.GetEmail() == signUpEmail))
                {
                    Console.WriteLine("Error: Email is already registered.");
                    break;
                }
                if (StudentSignUp(library, name, signUpEmail, signUpPassword, confirmPassword))
                {
                    Console.WriteLine("Sign up successful. You can now log in.");
                }
                else
                {
                    Console.WriteLine("Sign up failed. Please try again.");
                }
                break;
            case 3:
                // Exit
                Environment.Exit(0);
                break;
        }
    }
}

static Library LibraryBuilder()
{
    Console.WriteLine("Enter the name of your library:");
    string libraryName = Console.ReadLine() ?? "Campus Library";
    Console.WriteLine("How many items can a student check out at once? (default 3)");
    string maxItemsInput = Console.ReadLine() ?? "3";
    if (!int.TryParse(maxItemsInput, out int maxItems))
    {
        Console.WriteLine("Invalid input. Defaulting to 3 items.");
        maxItems = 3;
    }
    var library = new Library(maxItems)
    {
        Name = libraryName
    };
    return library;
}
static bool StudentSignUp(Library library, string name, string email, string password, string confirmPassword)
{
    // Check if email already exists
    foreach (var student in library.Students)
    {
        if (student.ValidateLogin(email, password))
        {
            Console.WriteLine("Error: Email is already registered.");
            return false;
        }
    }
    if (password != confirmPassword)
    {
        Console.WriteLine("Error: Passwords do not match.");
        return false;
    }
    var newStudent = new Student(name, email, password);
    library.Students.Add(newStudent);
    return true;
}
static int StudentLogin(Library library, string email, string password)
{
    foreach (var student in library.Students)
    {
        if (student.ValidateLogin(email, password))
        {
            return student.Id;
        }
    }
    return -1;
}

static Library CreateLibraryItem(Library library)
{
    Console.WriteLine("Enter the title of the item:");
    string title = Console.ReadLine() ?? "Untitled";
    Console.WriteLine("Enter the type of the item (e.g., Book, DVD):");
    string type = Console.ReadLine() ?? "Unknown";
    Console.WriteLine("Enter the release date of the item (YYYY-MM-DD or YYYY) or leave blank:");
    string? releaseDateInput = Console.ReadLine();
    DateOnly? releaseDate = null;
    if (releaseDateInput.Length == 4)
    {
        releaseDateInput = $"{releaseDateInput}-01-01";
    }
    if (DateOnly.TryParseExact(releaseDateInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedDate))
    {
        releaseDate = parsedDate;
    }
    Console.WriteLine("Enter the daily late fee for the item (e.g., 0.50):");
    string dailyLateFeeInput = Console.ReadLine() ?? "0.50";
    if (!double.TryParse(dailyLateFeeInput, out double dailyLateFee))
    {
        Console.WriteLine("Invalid input. Defaulting to $0.50.");
        dailyLateFee = 0.50;
    }
    var newItem = new LibraryItem(title, type, releaseDate, dailyLateFee);
    library.Items.Add(newItem);
    Console.WriteLine("Library item created successfully.");
    Console.WriteLine(newItem.PrintInfo());
    return library;
}


static void ShowLibraryMenu(Library library)
{
    Console.WriteLine($"Library: {library.Name}");
    Console.WriteLine("What would you like to do?");
    Console.WriteLine("1. View all items");
    Console.WriteLine("2. View checked out items");
    Console.WriteLine("3. View available items");
    Console.WriteLine("4. View students");
    Console.WriteLine("5. Back to main menu");
    int choice;
    while (true)
    {
        Console.Write("Enter your choice (1-5): ");
        try
        {
            choice = Convert.ToInt32(Console.ReadLine());
            if (choice < 1 || choice > 5)
            {
                Console.WriteLine("Invalid choice. Please enter a number between 1 and 5.");
                continue;
            }
            switch (choice)
            {
                case 1:
                    Console.WriteLine("All Library Items:");
                    foreach (var item in library.Items)
                    {
                        Console.WriteLine(item.ToString());
                    }
                    break;
                case 2:
                    Console.WriteLine("Checked Out Items:");
                    foreach (var checkOut in library.CheckOutItems)
                    {
                        Console.WriteLine(checkOut.GetItem().ToString());
                    }
                    break;
                case 3:
                    Console.WriteLine("Available Items:");
                    foreach (var item in library.Items)
                    {
                        if (item.GetAvailability())
                        {
                            Console.WriteLine(item.ToString());
                        }
                    }
                    break;
                case 4:
                    Console.WriteLine("Registered Students:");
                    foreach (var student in library.Students)
                    {
                        Console.WriteLine($"ID: {student.Id}, Name: {student.Name}, Balance: {student.Balance:C}");
                    }
                    break;
                case 5:
                    return;
            }
        }
        catch
        {
            Console.WriteLine("Invalid input. Please enter a number between 1 and 5.");
        }
    }
}

// Startup menu
static int PreMenu()
{
    Console.WriteLine("Choose an option:");
    Console.WriteLine("1. Create Library Item");
    Console.WriteLine("2. Save Library");
    Console.WriteLine("3. View Library");
    Console.WriteLine("4. Continue");
    Console.WriteLine("5. Exit");
    int choice;
    while (true)
    {
        Console.Write("Enter your choice (1-5): ");
        try
        {
            choice = Convert.ToInt32(Console.ReadLine());
            if (choice < 1 || choice > 5)
            {
                Console.WriteLine("Invalid choice. Please enter a number between 1 and 5.");
                continue;
            }

            return choice;
        }
        catch
        {
            Console.WriteLine("Invalid input. Please enter a number between 1 and 5.");
        }
    }
}
static int LoginMenu()
{
    Console.WriteLine("Choose an option:");
    Console.WriteLine("1. Login");
    Console.WriteLine("2. Sign Up");
    Console.WriteLine("3. Exit");
    int choice;
    while (true)
    {
        Console.Write("Enter your choice (1-3): ");
        try
        {
            choice = Convert.ToInt32(Console.ReadLine());
            if (choice < 1 || choice > 3)
            {
                Console.WriteLine("Invalid choice. Please enter a number between 1 and 3.");
                continue;
            }

            return choice;
        }
        catch
        {
            Console.WriteLine("Invalid input. Please enter a number between 1 and 3.");
        }
    }
}
static int StudentMenu()
{
    Console.WriteLine("Choose an option:");
    Console.WriteLine("1. View Checked Out Items");
    Console.WriteLine("2. Check Out Item");
    Console.WriteLine("3. Return Item");
    Console.WriteLine("4. View Balance");
    Console.WriteLine("5. Logout");
    int choice;
    while (true)
    {
        Console.Write("Enter your choice (1-5): ");
        try
        {
            choice = Convert.ToInt32(Console.ReadLine());
            if (choice is < 1 or > 5)
            {
                Console.WriteLine("Invalid choice. Please enter a number between 1 and 5.");
                continue;
            }

            return choice;
        }
        catch
        {
            Console.WriteLine("Invalid input. Please enter a number between 1 and 5.");
        }
    }
}

// File functions
static List<string> LoadFileNames(string fileType = "csv")
{
    var files = new List<string>();
    foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), $"*.{fileType}"))
    {
        files.Add(Path.GetFileNameWithoutExtension(file));
    }

    return files;
}

// The classes for each item in the library
internal class Library
{
    public string Name { get; set; }
    public List<LibraryItem> Items { get; set; }
    public List<CheckOutItem> CheckOutItems { get; set; }
    public List<Student> Students { get; set; }
    public int MaxStudentItems { get; set; }

    public Library(int maxStudentItems = 3, string name = "Campus Library")
    {
        Name = name;
        Items = new List<LibraryItem>();
        CheckOutItems = new List<CheckOutItem>();
        Students = new List<Student>();
        MaxStudentItems = maxStudentItems;
    }
    public string CsvFormat()
    {
        return $"Library,\"{this.Name}\",{this.MaxStudentItems}";
    }
}

internal class LibraryItem
{
    private static int _nextId = 1;
    public int Id { get; }
    private string Title { get; }
    private string Type { get; }
    private DateOnly? ReleaseDate { get; }
    private double DailyLateFee { get; }
    private bool IsAvailable { get; set; }

    public LibraryItem(string title, string type, DateOnly? releaseDate, double dailyLateFee, int id = -1)
    {
        if (id == -1)
        {
            Id = _nextId++;
        }
        else
        {
            Id = id;
        }
        Title = title;
        Type = type;
        ReleaseDate = releaseDate;
        DailyLateFee = dailyLateFee;
        IsAvailable = true;
    }
    // Menu style
    public override string ToString()
    {
        return $"{Id}: {Title} ({Type}, {ReleaseDate?.ToString("yyyy") ?? "Year N/A"}) - Available: {IsAvailable}";
    }
    // Full info
    public string PrintInfo()
    {
        return $"ID: {Id}, Title: {Title}, Type: {Type}, Released: {ReleaseDate?.ToString("yyyy-MM-dd") ?? "N/A"}, Daily Late Fee: {DailyLateFee:C}, Available: {IsAvailable}";
    }

    public string PrintReciept()
    {
        return $"{Id} - {Title} ({Type}) - {ReleaseDate?.ToString("yyyy") ?? "Year N/A"} - {DailyLateFee:C} ";
    }
    public bool GetAvailability()
    {
        return IsAvailable;
    }
    public bool CheckOut()
    {
        if (!IsAvailable) return false;
        IsAvailable = false;
        return true;
    }
    public bool CheckIn()
    {
        if (IsAvailable) return false;
        IsAvailable = true;
        return true;
    }
    public double GetDailyLateFee()
    {
        return DailyLateFee;
    }

    public string CsvFormat()
    {
        return
            $"Item,{this.Id},\"{this.Title}\",\"{this.Type}\",{this.ReleaseDate?.ToString("yyyy-MM-dd") ?? ""},{this.DailyLateFee},{this.IsAvailable}";
    }
    public static void SetIdAutoIncrementPointer(int nextId)
    {
        _nextId = nextId;
    }
}
// The student class, students can check out items where it is assigned to them
internal class Student
{
    private static int _nextId = 1;
    public int Id { get; }
    public string Name { get; set; }
    // a real system would have better security but this is fine for a demo
    private string Email { get; }
    private string Password { get; }
    // If balance is negative, they owe money
    public double Balance { get; set; }
    private List<CheckOutItem> _checkedOutItems = new List<CheckOutItem>();

    public Student(string name, string email, string password, int id = -1)
    {
        if (id == -1)
        {
            Id = _nextId++;
        }
        else
        {
            Id = id;
        }
        Name = name;
        Email = email;
        Password = password;
        Balance = 0;
    }
    public bool ValidateLogin(string email, string password)
    {
        return email == Email && password == Password;
    }
    public IReadOnlyList<CheckOutItem> GetCheckedOutItems()
    {
        return _checkedOutItems.AsReadOnly();
    }
    public string GetEmail()
    {
        return Email;
    }
    public void AddCheckedOutItem(CheckOutItem item)
    {
        _checkedOutItems.Add(item);
    }

    public void RemoveCheckedOutItem(CheckOutItem item)
    {
        _checkedOutItems.Remove(item);
    }
    public int CheckOutSlotsLeft(int maxItems)
    {
       return Math.Max(0, maxItems - _checkedOutItems.Count);
    }
    // Changes the balance by the amount, positive or negative. Can be dry called to do a balance check.
    public double AdjustBalance(double amount=0)
    {
        Balance += amount;
        return Balance;
    }
    //used by csv
    public string CsvFormat()
    {
        return $"Student,{this.Id},\"{this.Name}\",\"{this.Email}\",\"{this.Password}\",{this.Balance}";
    }
    public static void SetIdAutoIncrementPointer(int nextId)
    {
        _nextId = nextId;
    }

}
// The class that checks out items and sets dates
internal class CheckOutItem
{

    public int StudentId { get; }
    private LibraryItem Item { get; }
    private DateOnly CheckOutDate { get; set; }
    private DateOnly DueDate { get; }
    private DateOnly? CheckInDate { get; set; }

    public CheckOutItem(int studentId, LibraryItem item, DateOnly checkOutDate, DateOnly dueDate)
    {
        StudentId = studentId;
        Item = item;
        CheckOutDate = checkOutDate;
        DueDate = dueDate;
        CheckInDate = null;
    }

    public void PrintReciept()
    {
        DateOnly tempDate = DateOnly.FromDateTime(DateTime.Now);
        if (CheckInDate != null)
        {
            tempDate = CheckInDate.Value;
        }
        Console.WriteLine($"{Item.PrintReciept()} - {DueDate} - {this.CalculateLateDays(tempDate)}");
    }

    public double CalculateLateFee(DateOnly returnDate)
    {
        if (returnDate <= DueDate) return 0;
        int lateDays = (returnDate.ToDateTime(new TimeOnly()) - DueDate.ToDateTime(new TimeOnly())).Days;
        return lateDays * Item.GetDailyLateFee();
    }
    public int CalculateLateDays(DateOnly returnDate)
    {
        if (returnDate <= DueDate) return 0;
        return (returnDate.ToDateTime(new TimeOnly()) - DueDate.ToDateTime(new TimeOnly())).Days;
    }
    public LibraryItem GetItem()
    {
        return Item;
    }

    public bool CheckOut(LibraryItem item)
    {
        switch (item.GetAvailability())
        {
            case true:
                Item.CheckOut();
                return true;
            case false:
                return false;
        }
    }
    public bool CheckIn(DateOnly returnDate)
    {
        if (CheckInDate != null) return false;
        if (returnDate < CheckOutDate) return false;
        CheckInDate = returnDate;
        Item.CheckIn();
        return true;
    }

    public string CsvFormat()
    {
        return
            $"CheckOut,{this.StudentId},{this.Item.Id},{this.CheckOutDate},{this.DueDate},{this.CheckInDate?.ToString("yyyy-MM-dd") ?? ""}";
    }
}


/* The original Sqlite idea was going to use this table structure.
 Since SQL is more designed to handle relations, I didn't have to store whole lists of objects.
 I could also store multiple libraries and their contents in one database.
 You just select what library you want to load, and everything else is linked by that id so it would all get pulled in.
 I ran out of time to look into how c# implements SQLite, most of that time was spent on the database project.
 It follows the C# naming conventions to make it easier to map to the classes I made for this assigment.
 
 --- SQL Table Structure ---
CREATE TABLE Library (
    Id INT PRIMARY KEY,
    Name TEXT NOT NULL,
    MaxStudentItems INT NOT NULL DEFAULT 3
);
CREATE TABLE LibraryItem (
    Library INT,
    Id INT,
    Title TEXT NOT NULL,
    Type TEXT NOT NULL,
    ReleaseDate DATE,
    DailyLateFee DECIMAL(5,2) NOT NULL DEFAULT 0.50,
    IsAvailable BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (Library, Id),
    FOREIGN KEY (Library) REFERENCES Library(Id)
);
CREATE TABLE Student (
    Library INT,
    Id INT,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL,
    Password TEXT NOT NULL,
    Balance DECIMAL(7,2) NOT NULL DEFAULT 0.00,
    PRIMARY KEY (Library, Id),
    FOREIGN KEY (Library) REFERENCES Library(Id)
);
CREATE TABLE CheckOutItem (
    Library INT,
    Student INT,
    Item INT,
    CheckOutDate DATE NOT NULL,
    DueDate DATE NOT NULL DEFAULT (DATE('now', '+14 days')),
    CheckInDate DATE,
    PRIMARY KEY (Library, Student, Item),
    FOREIGN KEY (Library) REFERENCES Library(Id),
    FOREIGN KEY (Student) REFERENCES Student(Id),
    FOREIGN KEY (Item) REFERENCES LibraryItem(Id)
);
*/