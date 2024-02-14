using System;
using System.IO;
using System.Collections.Generic;

public class UserManager : IUserManager
{
    private string _currentDirectory;
    private string lastLoggedInUser;
    private string _userFile = "lastUser.txt"; // File to store the last logged-in user
    private string _directoryFile = "lastDirectory.txt";
    
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

        // Write the username to the file
        File.WriteAllText(_userFile, username);
        File.WriteAllText(_directoryFile, _currentDirectory);
    }

    public void SwitchProfile(string profileName)
    {
        // Implement switch profile logic if needed
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
    }
    
}



public class FileManager : IFileManager
{
    private readonly IUserManager userManager;

    public FileManager(IUserManager userManager)
    {
        this.userManager = userManager;
    }

    public void AddFile(string sourceFilePath, string shortcut = null)
    {
        string lastLoggedInUser = userManager.GetLastLoggedInUser();
        string userFolderPath = userManager.GetCurrentDirectory();

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
            Console.WriteLine($"File '{fileName}' added to user folder '{lastLoggedInUser}' with full path '{destinationFilePath}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding file '{fileName}' to user folder '{lastLoggedInUser}': {ex.Message}");
        }
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
    }
    
    public void ListFolders()
    {
        string lastLoggedInUser = userManager.GetLastLoggedInUser();
        string userFolderPath = userManager.GetCurrentDirectory();

        if (!Directory.Exists(userFolderPath))
        {
            Console.WriteLine($"User folder for '{lastLoggedInUser}' does not exist.");
            return;
        }

        string[] folders = Directory.GetDirectories(userFolderPath);

        if (folders.Length == 0)
        {
            Console.WriteLine($"No folders found in user folder '{lastLoggedInUser}'.");
            return;
        }

        Console.WriteLine($"Folders in user folder '{lastLoggedInUser}':");
        foreach (string folder in folders)
        {
            Console.WriteLine(Path.GetFileName(folder));
        }
    }

}


public class PlanManager : IPlanManager
{
    public void ChangePlan(string planName)
    {
        Console.WriteLine($"Changing plan to: {planName}...");
        // Perform change plan operation
    }
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