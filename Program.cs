using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

// Kill Origin/EA Desktop
Console.WriteLine("Killing Origin and EA Desktop");
foreach (Process process in Process.GetProcessesByName("EADesktop")) process.Kill();
foreach (Process process in Process.GetProcessesByName("Origin")) process.Kill();

// Path to local.xml
string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Origin\local.xml");
// Settings
string migrationSetting = "  <Setting value=\"true\" key=\"MigrationDisabled\" type=\"1\"/>";
string updateUrlSetting = "  <Setting key=\"UpdateURL\" value=\"http://victorpl.troll/\" type=\"10\"/>";
string autoPatchGlobalSetting = "  <Setting key=\"AutoPatchGlobal\" value=\"true\" type=\"1\"/>";
string autoUpdateSetting = "  <Setting value=\"true\" key=\"AutoUpdate\" type=\"1\"/>";
// Origin Path
string originPath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Origin")?.GetValue("OriginPath")?.ToString();


// Version check Origin
try {
    Version originVersion = new(FileVersionInfo.GetVersionInfo(originPath).FileVersion.Replace(",", "."));
    Version finalVersion = new("10.5.119.0");

    // Origin is too recent
    if (originVersion.CompareTo(finalVersion) >= 0) {
        Console.WriteLine("You must install an older version of Origin");
        Console.WriteLine("Origin v10.5.118.52644: https://drive.google.com/file/d/15n3z_6-4P5d56y2y8s0UbWHgaheBi2a4");

        Console.Write("Press Enter to Close This Window...");
        Console.ReadLine();
        Environment.Exit(0);
    }
}
catch (Exception ex) {
    Console.WriteLine(ex.ToString());
}

// Prevent Origin from updating itself
try
{
    string originSetupInternal = originPath.Replace("Origin.exe", "OriginSetupInternal.exe");
    string originThinSetupInternal = originPath.Replace("Origin.exe", "OriginThinSetupInternal.exe");

    // Check if OriginSetupInternal exists and is not read-only (if it is, it's already been patched)
    if (!File.Exists(originSetupInternal) || File.GetAttributes(originSetupInternal).HasFlag(FileAttributes.ReadOnly))
        Console.WriteLine($"{originSetupInternal} has already been patched or doesn't exist.");
    // If it exists and isn't read-only, patch it
    else {
        Console.WriteLine($"Backing up {originSetupInternal}");
        File.Copy(originSetupInternal, originSetupInternal + ".bak");
        Console.WriteLine($"Patching {originSetupInternal}");
        // Replace the contents of the file with nothing
        File.WriteAllText(originSetupInternal, "");
        // Set the file to read-only
        File.SetAttributes(originSetupInternal, FileAttributes.ReadOnly);
    }

    // Check if OriginThinSetupInternal exists and is not read-only (if it is, it's already been patched)
    if (!File.Exists(originThinSetupInternal) || File.GetAttributes(originThinSetupInternal).HasFlag(FileAttributes.ReadOnly))
        Console.WriteLine($"{originThinSetupInternal} has already been patched or doesn't exist.");
    else
    {
        Console.WriteLine($"Backing up {originThinSetupInternal}");
        File.Copy(originThinSetupInternal, originThinSetupInternal + ".bak");
        Console.WriteLine($"Patching {originThinSetupInternal}");
        // Replace the contents of the file with nothing
        File.WriteAllText(originThinSetupInternal, "");
        // Set the file to read-only
        File.SetAttributes(originThinSetupInternal, FileAttributes.ReadOnly);
    }
}
catch {
    Console.WriteLine("Missing Permissions to patch file, Restarting as Administrator...");
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

    // Remove xml closer (and any accidental duplicates of it)
    foreach (string line in fileLines.FindAll(l => l.Equals("</Settings>"))) {
        fileLines.Remove(line);
    }

    // Remove duplicates of migration setting
    foreach (string line in fileLines.FindAll(l => l.Equals(migrationSetting))) {
        fileLines.Remove(line);
    }
    
    // Remove duplicates of Update URL setting
    foreach (string line in fileLines.FindAll(l => l.Equals(updateUrlSetting)))
    {
        fileLines.Remove(line);
    }

    // Remove duplicates of Auto Patch setting
    foreach (string line in fileLines.FindAll(l => l.Equals(autoPatchGlobalSetting)))
    {
        fileLines.Remove(line);
    }

    // Remove duplicates of disabled auto patch
    foreach (string line in fileLines.FindAll(l => l.Equals(autoPatchGlobalSetting.Replace("true", "false"))))
    {
        fileLines.Remove(line);
    }

    // Remove duplicates of Auto Update setting
    foreach (string line in fileLines.FindAll(l => l.Equals(autoUpdateSetting))) {
        fileLines.Remove(line);
    }

    // Remove duplicates of disabled auto update
    foreach (string line in fileLines.FindAll(l => l.Equals(autoUpdateSetting.Replace("true", "false")))) {
        fileLines.Remove(line);
    }

    // Add stuff
    fileLines.Add(migrationSetting);
    fileLines.Add(updateUrlSetting);
    fileLines.Add(autoPatchGlobalSetting.Replace("true", "false"));
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
