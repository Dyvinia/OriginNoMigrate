using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

// Kill Origin/EA Desktop
Console.WriteLine("Killing Origin and EA Desktop");
foreach (Process process in Process.GetProcessesByName("EADesktop")) process.Kill();
foreach (Process process in Process.GetProcessesByName("Origin")) process.Kill();

string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Origin\local.xml");
string migrationSetting = "  <Setting value=\"true\" key=\"MigrationDisabled\" type=\"1\"/>";
string autoUpdateSetting = "  <Setting value=\"true\" key=\"AutoUpdate\" type=\"1\"/>";
string originPath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Origin")?.GetValue("OriginPath")?.ToString();


// Version check Origin
try {
    Version originVersion = new(FileVersionInfo.GetVersionInfo(originPath).FileVersion.Replace(",", "."));
    Version finalVersion = new("10.5.119.0");

    // Origin is too recent
    if (originVersion.CompareTo(finalVersion) >= 0) {
        Console.WriteLine("You must install an older version of Origin");
        Console.WriteLine("Origin v10.5.118.52644: https://cdn.discordapp.com/attachments/1054968664807452812/1085342027383832627/OriginSetup.exe");

        Console.Write("Press Enter to Close This Window...");
        Console.ReadLine();
        Environment.Exit(0);
    }
}
catch (Exception ex) {
    Console.WriteLine(ex.ToString());
}

// Prevent Origin from updating itself
try {
    string originSetupInternal = originPath.Replace("Origin.exe", "OriginSetupInternal.exe");
    string originThinSetupInternal = originPath.Replace("Origin.exe", "OriginThinSetupInternal.exe");

    if (File.Exists(originSetupInternal)) {
        Console.WriteLine($"Deleting {originSetupInternal}");
        File.Delete(originSetupInternal);
    }

    if (File.Exists(originThinSetupInternal)) {
        Console.WriteLine($"Deleting {originThinSetupInternal}");
        File.Delete(originThinSetupInternal);
    }
}
catch {
    Console.WriteLine("Missing Permissions to delete file, Restarting as Administrator...");
    Console.Write("Press Any Key to Continue...");
    Console.ReadKey();

    Process.Start(new ProcessStartInfo {
        FileName = Environment.ProcessPath,
        UseShellExecute = true,
        Verb = "runas"
    });
    Environment.Exit(0);
}

// Disable EA App Migration
try {
    Console.WriteLine($"Opening {path}");
    List<string> fileLines = new(File.ReadAllLines(path));

    // remove xml closer (and any accidental duplicates of it)
    foreach (string line in fileLines.FindAll(l => l.Equals("</Settings>"))) {
        fileLines.Remove(line);
    }

    // remove duplicates of new setting
    foreach (string line in fileLines.FindAll(l => l.Equals(migrationSetting))) {
        fileLines.Remove(line);
    }
    
    // disable Auto Updates
    foreach (string line in fileLines.FindAll(l => l.Equals(autoUpdateSetting))) {
        fileLines.Remove(line);
    }

    // remove duplicates of disabled auto update
    foreach (string line in fileLines.FindAll(l => l.Equals(autoUpdateSetting.Replace("true", "false")))) {
        fileLines.Remove(line);
    }

    // add stuff
    fileLines.Add(migrationSetting);
    fileLines.Add(autoUpdateSetting.Replace("true", "false"));
    fileLines.Add("</Settings>");

    // write new text
    Console.WriteLine($"Writing text to {path}");
    File.WriteAllLines(path, fileLines.ToArray());

    Console.WriteLine("Done");
}
catch (Exception ex) {
    Console.WriteLine(ex.ToString());
}

// end
Console.WriteLine("");
Console.Write("Press Any Key to Continue...");
Console.ReadKey();
