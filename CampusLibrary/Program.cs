/*
Campus Library remade from scratch.
All the code is mine, but I did reference a StackOverflow lookup.
*/

using CampusLibrary;

var library = new Library("Placeholder");
var fileList = Csv.GetFileNames();
int loggedInUserId = 0;
var fileName = "";
string? input = "-1";


Menu.Startup();
if (fileList.Count == 0)
{
    // Go straight to library builder if there isn't a file
    Console.WriteLine("No libraries found.");
    library = LibraryBuilder();
}
else
{
    Console.WriteLine("Enter the number of the file you want to load, or enter 0 to create a new library:");
    input = Console.ReadLine();
    if (input == "0")
    {
        Console.WriteLine("Loading library builder...");
        library = LibraryBuilder();
    }
    else
    {
        int fileIndex;
        try
        {
            fileIndex = Math.Abs(int.Parse(input)) - 1;
            if (fileIndex < 0 || fileIndex >= fileList.Count)
            {
                Console.WriteLine("Invalid selection. Loading library builder...");
                library = LibraryBuilder();
            }
            else
            {
                fileName = fileList[fileIndex];
                library = Csv.LoadLibrary(fileName);
            }
        }
        catch
        {
            Console.WriteLine("Invalid input. Loading library builder...");
            library = LibraryBuilder();
        }
    }
}

// PreMenu on setting up the library
while (input != "3")
{
    Menu.PreMenu();
    try
    {
        input = Console.ReadLine();
        if (input == "0" || input == "4")
        {
            // Exit and save
            Csv.SaveLibrary(library, fileName);
            Console.WriteLine("Library saved. Exiting...");
            return;
        }
        else if (input == "1")
        {
            // It's in the name
            EditItems();
        }
        else if (input == "2")
        {
            // View users if they exist
            Menu.ListUsers(library.Users);
        }
        else if (input == "3")
        {
                // Continue to User Menu
                break;
        }
    }
    catch
    {
        Console.WriteLine("Invalid input.");
    }
}

// User side menu
while (true)
{
    Menu.UserPreMenu();
    try
    {
        input = Console.ReadLine();
        if (input == "0" || input == "3")
        {
            // Save library and get out
            Csv.SaveLibrary(library, fileName);
            Console.WriteLine("Exiting...");
            break;
        }
        else if (input == "1")
        {
            // Login
            if (library.Users.Count == 0)
            {
                Console.WriteLine("No users found. Please make an account.");
                continue;
            }
            loggedInUserId = LoginUser(library.Users);
            if (loggedInUserId == 0)
            {
                Console.WriteLine("Please try again.");
                continue;
            }
            // Main loop, most of what users would do goes through this function
            UserLoop();
        }
        else if (input == "2")
        {
            // Make account
            if (UserBuilder())
            {
                Console.WriteLine("You can now log in with the account.");
                continue;
            }

            Console.WriteLine("Account creation failed. Please try again.");
        }
    }
    catch
    {
        Console.WriteLine("Invalid input.");
    }
}


return;

// Builder functions for the classes
Library LibraryBuilder()
{
    if (fileName != "" && fileName.Length > 0)
    {
        return Csv.LoadLibrary(fileName);
    }
    else
    {
        Console.WriteLine("Enter the name of your library:");
        var name = Console.ReadLine();
        while (!Csv.ValidateString(name))
        {
            Console.WriteLine("Enter the name of your library:");
            name = Console.ReadLine();
        }

        Console.WriteLine("Enter the student checkout limit (default 3):");
        var limitInput = Console.ReadLine();
        int limit;
        try
        {
            limit = Math.Abs(int.Parse(limitInput));
        }
        catch
        {
            limit = 3;
        }

        Console.WriteLine("Enter the checkout duration in days (default 14):");
        var durationInput = Console.ReadLine();
        int duration;
        try
        {
            duration = Math.Abs(int.Parse(durationInput));
        }
        catch
        {
            duration = 14;
        }

        Console.WriteLine("What name would you like to save the library as? (without .csv)");
        var fileNameInput = Console.ReadLine();
        if (!Csv.UniqueFileName(fileName))
        {
            Console.WriteLine("File already exists. Overwrite? (y/n)");
            var overwriteInput = Console.ReadLine();
            if (overwriteInput.ToLower() == "y")
            {
                fileName = fileNameInput + ".csv";
            }
            else
            {
                fileNameInput = "";
            }
        }
        while (!Csv.ValidateFileName(fileNameInput))
        {
            Console.WriteLine("What name would you like to save the library as? (without .csv)");
            fileNameInput = Console.ReadLine();
        }
        fileName = fileNameInput + ".csv";
        return new Library(name, limit, duration);
    }
}

Item ItemBuilder()
{
    Console.WriteLine("Enter the item name:");
    var name = Console.ReadLine();
    while (!Csv.ValidateString(name))
    {
        Console.WriteLine("Enter the item name:");
        name = Console.ReadLine();
    }

    Console.WriteLine("What type of item is this? (e.g., Book, DVD, Magazine)");
    var type = Console.ReadLine();
    while (!Csv.ValidateString(type))
    {
        Console.WriteLine("What type of item is this? (e.g., Book, DVD, Magazine)");
        type = Console.ReadLine();
    }

    Console.WriteLine("Enter the daily late fee for this item (default $0.50):");
    var feeInput = Console.ReadLine();
    double fee;
    try
    {
        fee = Math.Abs(double.Parse(feeInput));
    }
    catch
    {
        Console.WriteLine("Invalid input. Defaulting to $0.50");
        fee = 0.5;
    }
    return new Item(name, type, 0, fee);
}


// This builder works differently because of the email uniqueness check
// It adds it to the library on it's own if it succeeds and just tells if worked or not
bool UserBuilder()
{
    Console.WriteLine("What is your name?");
    var name = Console.ReadLine();
    while (!Csv.ValidateString(name))
    {
        Console.WriteLine("What is your name?");
        name = Console.ReadLine();
    }
    Console.WriteLine("Enter your email address:");
    var email = Console.ReadLine();
    while (!Csv.ValidateString(email))
    {
        Console.WriteLine("Enter your email address:");
        email = Console.ReadLine();
    }
    if(!User.UniqueUsername(email, library.Users))
    {
        Console.WriteLine("A user with that email already exists.");
        return false; 
    }
    Console.WriteLine("Create a password:");
    var password = Console.ReadLine();
    while (!Csv.ValidateString(password))
    {
        Console.WriteLine("Create a password:");
        password = Console.ReadLine();
    }

    Console.WriteLine("Confirm your password:");
    var confirmPassword = Console.ReadLine();
    if (password != confirmPassword)
    {
        // Don't need to string validate since it obviously will not match now
        Console.WriteLine("Passwords do not match. Please try again.");
        return false;
    }
    var newUser = new User(name, email, password);
    library.Users.Add(newUser);
    Console.WriteLine("User registered: " + name);
    return true;
}

// Main program loops
int LoginUser(List<User> users)
{
    Console.WriteLine("Enter your email address:");
    var email = Console.ReadLine();
    Console.WriteLine("Enter your password:");
    var password = Console.ReadLine();

    foreach (var user in users)
    {
        var userId = user.Authenticate(email, password);
        if (userId != 0)
        {
            Console.WriteLine($"Welcome back, {user.Name}!");
            return userId;
        }
    }
    
    Console.WriteLine("Invalid login.");
    return 0;
}

// Originally Edit Items and Create New Item were separate
// I merged them to keep base menus simple but the names are now inconsistent a bit
void EditItems()
{
    if (library.Items.Count == 0)
    {
        Console.WriteLine("No items found. Please add an item first.");
        var newItem = ItemBuilder();
        library.Items.Add(newItem);
        Console.WriteLine("Item added: " + newItem.ToStringFull());
        return;
    }
    Menu.EditItemMenu(library.Items);
    var input = Console.ReadLine();
    int selection;
    try
    {
        selection = Math.Abs(int.Parse(input));
    }
    catch
    {
        Console.WriteLine("Invalid input.");
        return;
    }

    switch (selection)
    {
        case 0: 
        case 3:
            // Exit
            return;
        case 1:
            // Add Item
            var newItem = ItemBuilder();
            library.Items.Add(newItem);
            Console.WriteLine("Item added: " + newItem.ToStringFull());
            break;
        case 2:
            // Remove Item
            Menu.ListItems(library.Items, true, true);
            Console.WriteLine("Enter the ID of the item to remove (0 to cancel):");
            var inputId = Console.ReadLine();
            int selectionId;
            try
            {
                selectionId = Math.Abs(int.Parse(inputId));
            }
            catch
            {
                Console.WriteLine("Invalid input.");
                return;
            }
            if(selectionId == 0) return;
            // https://stackoverflow.com/a/1485775
            // It's just checking against one value, but I prefer using the lookup for this
            // I did one without lookups in User.UniqueUsername() if you want an example of that
            var itemToRemove = library.Items.Find(i => i.Id == selectionId);
            if (itemToRemove == null)
            {
                Console.WriteLine("Item not found.");
                return;
            }
            if(!itemToRemove.IsAvailable())
            {
                Console.WriteLine("Item is not available and cannot be removed.");
                return;
            }
            itemToRemove.CheckOut(-1);
            library.Items.Remove(itemToRemove);
            Console.WriteLine("Item removed: " + itemToRemove.ToStringFull());
            break;
    }
}

// Main user loop
void UserLoop()
{
    // https://stackoverflow.com/a/1485775
    User currentUser = library.Users.Find(u => u.Id == loggedInUserId);
    while(loggedInUserId != 0 || loggedInUserId != 4)
    {
        Menu.UserMenu();
        input = Console.ReadLine();
        try
        {
            int selection = Math.Abs(int.Parse(input));
            switch (selection)
            {
                case 0:
                case 4:
                    Console.WriteLine("Signing out...");
                    loggedInUserId = 0;
                    return;
                case 1:
                    Menu.ListItems(library.Items, true);
                    Console.WriteLine("Enter an item id to check out, or enter 0 to go back: ");
                    // This part is taken from the EditItems function above
                    var inputId = Console.ReadLine();
                    int selectionId;
                    try
                    {
                        selectionId = Math.Abs(int.Parse(inputId));
                    }
                    catch
                    {
                        Console.WriteLine("Invalid input.");
                        break;
                    }
                    if(selectionId == 0) break;
                    var itemToCheckOut = library.Items.Find(i => i.Id == selectionId);
                    if (itemToCheckOut == null)
                    {
                        Console.WriteLine("Item not found.");
                        break;
                    }
                    if(!itemToCheckOut.IsAvailable())
                    {
                        Console.WriteLine("Item is not available for checkout.");
                        break;
                    }
                    itemToCheckOut.CheckOut(loggedInUserId);
                    currentUser.CheckedOutItems.Add(itemToCheckOut);
                    Console.WriteLine("Item checked out");
                    break;
                case 2:
                    // Similar to the above
                    foreach(var item in currentUser.CheckedOutItems)
                    {
                        Console.WriteLine(item.ToStringFull());
                    }

                    Console.WriteLine("Enter an item id to check in, or enter 0 to go back: ");
                    inputId = Console.ReadLine();
                    try
                    {
                        selectionId = Math.Abs(int.Parse(inputId));
                    }
                    catch
                    {
                        Console.WriteLine("Invalid input.");
                        break;
                    }
                    if(selectionId == 0) break;
                    var itemToCheckIn = library.Items.Find(i => i.Id == selectionId);
                    if (itemToCheckIn == null)
                    {
                        Console.WriteLine("Item not found.");
                        break;
                    }
                    if(itemToCheckIn.IsAvailable())
                    {
                        Console.WriteLine("Item is already checked in.");
                        break;
                    }
                    itemToCheckIn.CheckOut(0);
                    currentUser.AddBalance(-1* itemToCheckIn.CalculateLateFee(library.CheckoutDuration));
                    currentUser.CheckedOutItems.Remove(itemToCheckIn);
                    Console.WriteLine($"Item checked in {itemToCheckIn.CalculateLateDays()} days late.");
                    break;
                case 3:
                    Console.WriteLine($"Your current balance is: ${currentUser.AddBalance()}");
                    Console.WriteLine($"You have ${currentUser.CalculateLateFee(library.CheckoutDuration)} in estimated late fees.");
                    Console.WriteLine("How much would you like to add to your balance? (enter 0 to go back)");
                    var amountInput = Console.ReadLine();
                    double amount;
                    try
                    {
                        amount = Math.Abs(double.Parse(amountInput));
                        currentUser.AddBalance(amount);
                        Console.WriteLine($"New balance: ${currentUser.AddBalance()}");
                    }
                    catch
                    {
                        Console.WriteLine("Invalid input.");
                    }
                    break;
            }

        }
        catch
        {
            Console.WriteLine("Invalid input.");
        }
    }
}