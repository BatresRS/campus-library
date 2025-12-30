namespace CampusLibrary
{
    using System;
    using System.IO;
    
    public static class Csv
    {
        public static bool ValidateString(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Contains('"'))
            {
                Console.WriteLine("Input cannot be empty or contain quotation marks.");
                return false;
            }
            return true;
        }
        public static bool ValidateFileName(string fileName)
        {
            var badChars = Path.GetInvalidFileNameChars();
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Console.WriteLine("File name cannot be empty.");
                return false;
            }
            foreach (var c in badChars)
            {
                if (fileName.Contains(c))
                {
                    Console.WriteLine("A file name can't contain any of the following character:");
                    Console.WriteLine(badChars);
                    return false;
                }
            }
            return true;
        }

        public static List<string> GetFileNames()
        {
            var files = new List<string>();
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), $"*.csv"))
            {
                files.Add(Path.GetFileName(file));
            }
            return files;
        }
        
        public static bool UniqueFileName(string fileName)
        {
            List<string> files = GetFileNames();
            foreach (var file in files)
            {
                if (file == fileName)
                {
                    Console.WriteLine("A file with that name already exists.");
                    return false;
                }
            }
            return true;
        }

        public static Library LoadLibrary(string fileName)
        {
            // Fallback library if it's missing or not the first line
            var library = new Library("Library", 3, 14);
            var lines = File.ReadAllLines(fileName);
            foreach (var line in lines)
            {
                
                var cells = line.Split(',');
                try
                {
                    if (cells[0] == "LIBRARY")
                    {
                        library.Name = cells[1].Trim('"');
                        library.StudentCheckoutLimit = int.Parse(cells[2]);
                        library.CheckoutDuration = int.Parse(cells[3]);
                        Console.WriteLine("Loaded library: " + library.Name);
                    }
                    else if (cells[0] == "USER")
                    {
                        var user = new User(
                            cells[2].Trim('"'),
                            cells[3].Trim('"'),
                            cells[4].Trim('"'),
                            int.Parse(cells[1]),
                            double.Parse(cells[5])
                        );
                        library.Users.Add(user);
                        Console.WriteLine("Loaded user: " + user.ToString());
                    }
                    else if (cells[0] == "ITEM" && cells[4] != "-1")
                    {
                        // CheckedOutBy of -1 marks it as deleted
                        // It won't be loaded back, but you can manually go back and recover it if needed
                        var item = new Item(
                            cells[2].Trim('"'),
                            cells[3].Trim('"'),
                            int.Parse(cells[1]),
                            double.Parse(cells[5]),
                            int.Parse(cells[4]),
                            DateOnly.Parse(cells[6])
                        );
                        library.Items.Add(item);
                        Console.WriteLine("Loaded item: " + item.ToStringFull());
                    }
                    else if (cells[0] == "ITEM" && cells[4] != "-1")
                    {
                        Console.WriteLine($"Line {line} marked as deleted, skipping");
                    }
                    else
                    {
                        Console.WriteLine($"Bad line {line}, skipping");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error loading line {line}: {e.Message}, skipping");
                }
            }
            
            // Fill each user's checked out items
            // This is used when users see what items they have checked out
            foreach (var item in library.Items)
            {
                if (item.CheckedOutBy != 0)
                {
                    // https://stackoverflow.com/a/1485775
                    // Used a lookup to compare two different lists of values
                    var user = library.Users.Find(u => u.Id == item.CheckedOutBy);
                    if (user != null)
                    {
                        user.CheckedOutItems.Add(item);
                    }
                }
            }

            Console.WriteLine("Library loaded successfully.");
            return library;
        }

        public static void SaveLibrary(Library library, string fileName)
        {
            var lines = new List<string>
            {
                library.ExportData()
            };
            foreach (var user in library.Users)
            {
                lines.Add(user.ExportData());
            }
            foreach (var item in library.Items)
            {
                lines.Add(item.ExportData());
            }
            File.WriteAllLines(fileName, lines);
            Console.WriteLine("Library saved successfully.");
        }
    }
}