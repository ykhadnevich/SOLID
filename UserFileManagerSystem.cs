using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;


public class User
{
    public string Username { get; set; }
    public Plan Plan { get; set; }
}

public enum Plan
{
    Basic,
    Gold
}

public class UserDataModel
{
    public List<User> Users { get; set; }
}
public class UserManager : IUserManager
{
    private string _currentDirectory;
    private string lastLoggedInUser;
    private string _userFile = "lastUser.txt"; // File to store the last logged-in user
    private string _directoryFile = "lastDirectory.txt";
    private readonly string _userJsonFile = "users.json";
    private readonly long _basicPlanFileLimit = 10;
    private readonly long _basicPlanSizeLimit = 100 * 1024 * 1024; // 100 MB in bytes
    private readonly long _goldPlanFileLimit = 100;
    private readonly long _goldPlanSizeLimit = 1024 * 1024 * 1024; // 1 GB in bytes
    private readonly string _logFile = "log.json";
    private User _currentUser;

    public UserManager()
    {
        LoadUserInformation();
    }

    public User CurrentUser => _currentUser;

    public void Login(string username)
    {
        _currentDirectory = Path.Combine("C:\\Users\\user\\RiderProjects\\FolderCreation\\FolderCreation", username);
        lastLoggedInUser = username; // Set the last logged-in user

        if (!Directory.Exists(_currentDirectory))
        {
            Directory.CreateDirectory(_currentDirectory);
            Console.WriteLine($"Logged in as {username}. User folder created.{_currentDirectory}");
        }
        else
        {
            Console.WriteLine($"Logged in as {username}. User folder already exists.");
        }
        
        User existingUser = _currentUser = GetUser(username);
        
        File.WriteAllText(_userFile, username);
        File.WriteAllText(_directoryFile, _currentDirectory);

        if (existingUser == null)
        {
            // If the user doesn't exist, create a new user with Basic plan
            _currentUser = new User { Username = username, Plan = Plan.Basic };
            AddOrUpdateUser(_currentUser);
        }
        LogAction("Login", username);
    }
    
    public string GetCurrentDirectory()
    {
        // Read the current directory from the file
        if (File.Exists(_directoryFile))
        {
            _currentDirectory = File.ReadAllText(_directoryFile);
        }
        return _currentDirectory;
    }
    
    public string GetLastLoggedInUser()
    {
        // Read the last logged-in user from the file
        if (File.Exists(_userFile))
        {
            lastLoggedInUser = File.ReadAllText(_userFile);
        }
        return lastLoggedInUser;
    }
    
    public void Logout()
    {
        // Reset session information
        lastLoggedInUser = null;
        _currentDirectory = null;
        File.WriteAllText(_userFile, string.Empty);
        File.WriteAllText(_directoryFile, string.Empty);

        Console.WriteLine("Logged out successfully.");
        LogAction("Logout", _currentUser.Username);
    }
    private void LogAction(string action, string username)
    {
        var logEntry = new LogEntry
        {
            Action = action,
            Timestamp = DateTime.Now
        };

        Dictionary<string, List<LogEntry>> logData = new Dictionary<string, List<LogEntry>>();

        if (File.Exists(_logFile))
        {
            string json = File.ReadAllText(_logFile);
            logData = JsonSerializer.Deserialize<Dictionary<string, List<LogEntry>>>(json);
        }

        if (!logData.ContainsKey(username))
        {
            logData[username] = new List<LogEntry>();
        }

        logData[username].Add(logEntry);

        string updatedJson = JsonSerializer.Serialize(logData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_logFile, updatedJson);
    }
    public class LogEntry
    {
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public void ChangePlan(string planName)
    {
        if (!Enum.TryParse(planName, out Plan newPlan))
        {
            Console.WriteLine($"Invalid plan name: {planName}");
            return;
        }

        // Ensure user is logged in
        if (_currentUser == null)
        {
            Console.WriteLine("No user is currently logged in.");
            return;
        }

        if (_currentUser.Plan == newPlan)
        {
            Console.WriteLine($"Current plan is already {newPlan}.");
            return;
        }

        _currentUser.Plan = newPlan;
        AddOrUpdateUser(_currentUser);
        Console.WriteLine($"Plan changed to: {newPlan}");
    }

    private User GetUser(string username)
    {
        if (!File.Exists(_userJsonFile))
            return null;

        string json = File.ReadAllText(_userJsonFile);
        UserDataModel userData = JsonSerializer.Deserialize<UserDataModel>(json);

        return userData?.Users.FirstOrDefault(u => u.Username == username);
    }

    private void AddOrUpdateUser(User user)
    {
        UserDataModel userData = new UserDataModel { Users = new List<User>() };

        if (File.Exists(_userJsonFile))
        {
            string json = File.ReadAllText(_userJsonFile);
            userData = JsonSerializer.Deserialize<UserDataModel>(json);
        }

        // Remove existing user if already exists
        userData.Users.RemoveAll(u => u.Username == user.Username);
        // Add or update user
        userData.Users.Add(user);

        string updatedJson = JsonSerializer.Serialize(userData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_userJsonFile, updatedJson);
    }

    private void LoadUserInformation()
    {
        string lastLoggedInUser = GetLastLoggedInUser();
        if (!string.IsNullOrEmpty(lastLoggedInUser))
        {
            _currentUser = GetUser(lastLoggedInUser);
        }
    }

    public  bool ExceedsPlanLimits(long totalSizeInBytes, long totalFileCount)
    {
        if (_currentUser.Plan == Plan.Basic)
        {
            return totalSizeInBytes > _basicPlanSizeLimit || totalFileCount > _basicPlanFileLimit;
        }
        else if (_currentUser.Plan == Plan.Gold)
        {
            return totalSizeInBytes > _goldPlanSizeLimit || totalFileCount > _goldPlanFileLimit;
        }
        return false; // Default case
    }
}

public class FileManager : IFileManager
{
    
    
    private readonly IUserManager userManager;
    private readonly string _logFile = "log.json";
    public FileManager(IUserManager userManager)
    {
        this.userManager = userManager;
    }
    public class LogEntry
    {
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
    private void LogAction(string action)
    {
        string username = userManager.GetLastLoggedInUser();
        var logEntry = new LogEntry
        {
            Action = action,
            Timestamp = DateTime.Now
        };

        Dictionary<string, List<LogEntry>> logData = new Dictionary<string, List<LogEntry>>();

        if (File.Exists(_logFile))
        {
            string json = File.ReadAllText(_logFile);
            logData = JsonSerializer.Deserialize<Dictionary<string, List<LogEntry>>>(json);
        }

        if (!logData.ContainsKey(username))
        {
            logData[username] = new List<LogEntry>();
        }

        logData[username].Add(logEntry);

        string updatedJson = JsonSerializer.Serialize(logData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_logFile, updatedJson);
    }

    public void AddFile(string sourceFilePath, string shortcut = null)
    {
        /*string username = userManager.GetLastLoggedInUser();*/
        string userFolderPath = userManager.GetCurrentDirectory();
        long totalSizeInBytes = 0; // Get total size of all files in the directory
        string[] files = Directory.GetFiles(userFolderPath);
        long totalFileCount = files.Length; // Get total count of files in the directory
        
        
        
        if (Directory.Exists(userFolderPath))
        {
            string[] filesInDirectory = Directory.GetFiles(userFolderPath);
            foreach (string file in filesInDirectory)
            {
                totalSizeInBytes += new FileInfo(file).Length;
            }
            totalFileCount = filesInDirectory.Length;
        }

        // Check if user has exceeded plan limits
        if (userManager.ExceedsPlanLimits(totalSizeInBytes, totalFileCount))
        {
            Console.WriteLine("You have exceeded your plan limits.");
            return;
        }
        string lastLoggedInUser = userManager.GetLastLoggedInUser();
        

        if (!Directory.Exists(userFolderPath))
        {
            Console.WriteLine($"User folder for '{lastLoggedInUser}' does not exist.");
            return;
        }
        
        string fileName = Path.GetFileName(sourceFilePath);
        string fileExtension = Path.GetExtension(sourceFilePath); // Get the file extension
        string destinationFilePath;

        // Check if a shortcut is provided
        if (!string.IsNullOrEmpty(shortcut))
        {
            // Append the shortcut to the file name
            fileName = $"{shortcut}{fileExtension}";
            destinationFilePath = Path.Combine(userFolderPath, fileName);
        }
        else
        {
            destinationFilePath = Path.Combine(userFolderPath, fileName);
        }

        // Check if the file exists
        if (File.Exists(destinationFilePath))
        {
            string baseFileName = Path.GetFileNameWithoutExtension(fileName);
            int numberIndex = baseFileName.LastIndexOf('(');
            int count = 1;

            if (numberIndex != -1 && baseFileName.EndsWith(")"))
            {
                string numberString = baseFileName.Substring(numberIndex + 1, baseFileName.Length - numberIndex - 2);
                if (int.TryParse(numberString, out count))
                {
                    count++;
                    baseFileName = baseFileName.Substring(0, numberIndex).TrimEnd();
                }
            }

            // Iterate until a unique filename is found
            do
            {
                fileName = $"{baseFileName}({count}){fileExtension}";
                destinationFilePath = Path.Combine(userFolderPath, fileName);
                count++;
            }
            while (File.Exists(destinationFilePath));
            
        }

        // Copy the file
        try
        {
            File.Copy(sourceFilePath, destinationFilePath);
            Console.WriteLine($"File '{totalFileCount}' added to user folder '{lastLoggedInUser}' with full path '{destinationFilePath}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding file '{fileName}' to user folder '{lastLoggedInUser}': {ex.Message}");
        }
        LogAction("AddFile");
    }
    
    
    

    public void RemoveFile(string shortcut)
    {
        string lastLoggedInUser = userManager.GetLastLoggedInUser();
        string userFolderPath = userManager.GetCurrentDirectory();

        if (!Directory.Exists(userFolderPath))
        {
            Console.WriteLine($"User folder for '{lastLoggedInUser}' does not exist.");
            return;
        }

        string[] files = Directory.GetFiles(userFolderPath, $"*{shortcut}*");
        if (files.Length == 0)
        {
            Console.WriteLine($"File with shortcut '{shortcut}' not found in user folder '{lastLoggedInUser}'.");
            return;
        }
        else if (files.Length > 1)
        {
            Console.WriteLine($"Multiple files found with shortcut '{shortcut}' in user folder '{lastLoggedInUser}'.");
            return;
        }

        string filePath = files[0];

        try
        {
            File.Delete(filePath);
            Console.WriteLine($"File with shortcut '{shortcut}' removed from user folder '{lastLoggedInUser}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing file with shortcut '{shortcut}' from user folder '{lastLoggedInUser}': {ex.Message}");
        }
        LogAction("RemoveFile");
    }


    public void ListFiles()
    {
        string lastLoggedInUser = userManager.GetLastLoggedInUser();
        string userFolderPath = userManager.GetCurrentDirectory();

        if (!Directory.Exists(userFolderPath))
        {
            Console.WriteLine($"User folder for '{lastLoggedInUser}' does not exist.");
            return;
        }

        string[] files = Directory.GetFiles(userFolderPath);

        if (files.Length == 0)
        {
            Console.WriteLine($"No files found in user folder '{lastLoggedInUser}'.");
            return;
        }

        Console.WriteLine($"Files in user folder '{lastLoggedInUser}':");
        foreach (string file in files)
        {
            Console.WriteLine(Path.GetFileName(file));
        }

        Console.WriteLine($"Total number of files: {files.Length}");
        LogAction("CheckListFiles");
    }

    private static void PrintCsv(string filePath)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var columns = line.Split(',');
                Console.WriteLine(string.Join("\t", columns));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error printing CSV file: {ex.Message}");
        }
    }
    
    private static void PrintJson(string filePath)
    {
        try
        {
            string jsonString = File.ReadAllText(filePath);
            var jsonDocument = JsonDocument.Parse(jsonString);
            Console.WriteLine(JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error printing JSON file: {ex.Message}");
        }
    }
    
    
    public void ShowOptions(string shortcut)
    {
    string lastLoggedInUser = userManager.GetLastLoggedInUser();
    string userFolderPath = userManager.GetCurrentDirectory();
    string filePath = Path.Combine(userFolderPath, shortcut);

    if (!File.Exists(filePath))
    {
        Console.WriteLine($"File with shortcut '{shortcut}' does not exist in user folder '{lastLoggedInUser}'.");
        return;
    }

    // Determine file type
    string extension = Path.GetExtension(filePath).ToLower();

    Console.WriteLine($"Available actions for file '{shortcut}':");

    switch (extension)
    {
        case ".txt":
            Console.WriteLine("info - View file size and location on a local computer");
            Console.WriteLine("summary - Show basic information about text (number of symbols, words, paragraphs)");
            break;
        case ".csv":
            Console.WriteLine("info - View file size and location on a local computer");
            Console.WriteLine("print - Print nicely formatted table to the screen");
            Console.WriteLine("validate - Validate the CSV file");
            break;
        case ".json":
            Console.WriteLine("info - View file size and location on a local computer");
            Console.WriteLine("print - Print correctly indented JSON on the screen");
            Console.WriteLine("validate - Validate the JSON file");
            break;
        default:
            Console.WriteLine("info - View file size and location on a local computer");
            break;
    }
    LogAction("ShowOptions");
}
    private static void ValidateCsv(string filePath)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            int numColumns = lines[0].Split(',').Length;

            foreach (string line in lines)
            {
                if (line.Split(',').Length != numColumns)
                {
                    Console.WriteLine("CSV validation failed: Inconsistent number of columns.");
                    return;
                }
            }
        
            Console.WriteLine("CSV format is valid.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CSV validation failed: {ex.Message}");
        }
    }
    
    private static void ValidateJson(string filePath)
    {
        try
        {
            string jsonString = File.ReadAllText(filePath);
            JsonDocument.Parse(jsonString);
            Console.WriteLine("JSON format is valid.");
        }
        catch (JsonException)
        {
            Console.WriteLine("JSON validation failed: Invalid JSON format.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JSON validation failed: {ex.Message}");
        }
    }

    public void InvokeAction(string actionName, string shortcut)
    {
    string lastLoggedInUser = userManager.GetLastLoggedInUser();
    string userFolderPath = userManager.GetCurrentDirectory();
    string filePath = Path.Combine(userFolderPath, shortcut);

    if (!File.Exists(filePath))
    {
        Console.WriteLine($"File with shortcut '{shortcut}' does not exist in user folder '{lastLoggedInUser}'.");
        return;
    }

    // Determine file type
    string extension = Path.GetExtension(filePath).ToLower();
    

    switch (actionName.ToLower())
    {
        case "info":
            Console.WriteLine($"File size: {new FileInfo(filePath).Length} bytes");
            Console.WriteLine($"Location: {filePath}");
            break;
        case "summary":
            if (extension != ".txt")
            {
                Console.WriteLine($"Action 'summary' is not applicable for files with extension '{extension}'.");
                return;
            }

            // Read all lines from the text file
            string[] lines = File.ReadAllLines(filePath);

            // Count the number of symbols, words, and paragraphs
            int symbolsCount = 0;
            int wordsCount = 0;
            int paragraphsCount = 0;

            foreach (string line in lines)
            {
                symbolsCount += line.Length; // Count symbols in each line
                wordsCount += line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length; // Split each line into words and count them
                if (line.Trim() == "") // If line is empty (just whitespace), it's a paragraph
                {
                    paragraphsCount++;
                }
            }
            // Increment paragraphs count for the last line if it's not empty
            if (lines.Length > 0 && lines[lines.Length - 1].Trim() != "")
            {
                paragraphsCount++;
            }

            // Print the summary
            Console.WriteLine($"Number of symbols: {symbolsCount}");
            Console.WriteLine($"Number of words: {wordsCount}");
            Console.WriteLine($"Number of paragraphs: {paragraphsCount}");
            break;

        case "print":
            if (extension == ".csv")
            {
                // Print CSV file as a nicely formatted table
                PrintCsv(filePath);
            }
            else if (extension == ".json")
            {
                // Print JSON file with correct indentation
                PrintJson(filePath);
            }
            else
            {
                Console.WriteLine($"Action 'print' is not applicable for files with extension '{extension}'.");
                return;
            }
            break;

        case "validate":
            if (extension == ".csv")
            {
                // Validate CSV file
                ValidateCsv(filePath);
            }
            else if (extension == ".json")
            {
                // Validate JSON file
                ValidateJson(filePath);
            }
            else
            {
                Console.WriteLine($"Action 'validate' is not applicable for files with extension '{extension}'.");
                return;
            }
            break;
        default:
            Console.WriteLine($"Unknown action '{actionName}'.");
            break;
        
        
    } 
    LogAction("InvokeAction");
    }
}
public class PlanManager : IPlanManager
{
    /*public void ChangePlan1(string planName)
    {
        Console.WriteLine($"Changing plan to: {planName}...");
        // Perform change plan operation
    }*/
}

public class Logger : ILogger
{
    public void LogEvent(string eventName, Dictionary<string, string> parameters)
    {
        Console.WriteLine($"Logged event: {eventName}");
        foreach (var parameter in parameters)
        {
            Console.WriteLine($"{parameter.Key}: {parameter.Value}");
        }
    }
}

