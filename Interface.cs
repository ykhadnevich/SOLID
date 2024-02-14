using System.Collections.Generic;

public interface IUserManager
{
    void Login(string username);
    void SwitchProfile(string profileName);
    string GetCurrentDirectory();
    string GetLastLoggedInUser();
}


public interface IFileManager
{
    void AddFile(string fileName, string shortcut = null);
    void RemoveFile(string shortcut);
    void ListFiles();
}

public interface IPlanManager
{
    void ChangePlan(string planName);
}

public interface ILogger
{
    void LogEvent(string eventName, Dictionary<string, string> parameters);
}