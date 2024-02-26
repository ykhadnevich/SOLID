using System.Collections.Generic;
public interface ICommandHandler
{
    void HandleCommand(string command);
}
public interface IUserManager
{
    void Login(string username);
    
    string GetCurrentDirectory();
    string GetLastLoggedInUser();
    void ChangePlan(string planName);
    bool ExceedsPlanLimits(long totalSizeInBytes, long totalFileCount);
    
}



public interface IFileManager
{ 
    void AddFile(string filename, string shortcut = null);
    void RemoveFile(string shortcut);
    void ListFiles();
}

public interface IPlanManager
{
    /*void ChangePlan(string planName);*/
}

public interface ILogger
{
    /*void LogUserLoggedIn(string username);
    void LogFileAdded(string shortcut, string filetype);
    void LogFileRemoved(string shortcut, string filetype);
    void LogFileActionInvoked(string shortcut, string action);
    void LogPlanChanged(string username, string planName);
    void LogLimitReached(string limitType);
    
    void LogEvent(string eventName, Dictionary<string, string> parameters);*/
}