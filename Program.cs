using System;
using System.IO;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Invocation;

class Program
{
    static void Main(string[] args)
    {
        var userManager = new UserManager();
        var fileManager = new FileManager(userManager);
        var planManager = new PlanManager();
        var logger = new Logger();

        var loginCommand = new Command("login", "Login with a username")
        {
            new Argument<string>("username", "The username")
        };

        var addFileCommand = new Command("add", "Add a file")
        {
            new Argument<string>("filename", "The filename"),
            new Option<string>(new string[] { "--shortcut", "-s" }, "The shortcut (optional)"),
            
        };

        var removeFileCommand = new Command("remove", "Remove a file")
        {
            new Argument<string>("shortcut", "The shortcut")
        };

        var changePlanCommand = new Command("change-plan", "Change user's plan")
        {
            new Argument<string>("planName", "The plan name")
        };
        
        var listFilesCommand = new Command("list", "List files in the user's folder");
        var listFoldersCommand = new Command("list-folders", "List folders in the user's folder");
        var logoutCommand = new Command("logout", "Logout from the current user session");


        var rootCommand = new RootCommand
        {
            loginCommand,
            logoutCommand,
            addFileCommand,
            removeFileCommand,
            changePlanCommand,
            listFilesCommand,
            listFoldersCommand
        };
        
        

        loginCommand.Handler = CommandHandler.Create<string>((username) => userManager.Login(username));
        logoutCommand.Handler = CommandHandler.Create(() => userManager.Logout());
        addFileCommand.Handler = CommandHandler.Create<string, string>((filename, shortcut) => fileManager.AddFile(filename, shortcut));
        removeFileCommand.Handler = CommandHandler.Create<string>((shortcut) => fileManager.RemoveFile(shortcut));
        changePlanCommand.Handler = CommandHandler.Create<string>((planName) => planManager.ChangePlan(planName));
        listFilesCommand.Handler = CommandHandler.Create(() => fileManager.ListFiles());
        listFoldersCommand.Handler = CommandHandler.Create(() => fileManager.ListFolders()); // Set handler for list-folders command
        rootCommand.Invoke(args);
    }
}
