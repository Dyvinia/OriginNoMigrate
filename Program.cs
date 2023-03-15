﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

// die :3
Console.WriteLine("Killing Origin and EA Desktop");
foreach (Process process in Process.GetProcessesByName("EADesktop")) process.Kill();
foreach (Process process in Process.GetProcessesByName("Origin")) process.Kill();

string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Origin\local.xml");
string newSetting = "  <Setting value=\"true\" key=\"MigrationDisabled\" type=\"1\"/>";
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

    // No need for arguments since it can only reach this code if there are no arguments
    Process.Start(new ProcessStartInfo {
        FileName = Environment.ProcessPath,
        UseShellExecute = true,
        Verb = "runas"
    });
    Environment.Exit(0);
}

try {
    Console.WriteLine($"Opening {path}");
    List<string> fileLines = new(File.ReadAllLines(path));

    // remove xml closer (and any accidental duplicates of it)
    foreach (string line in fileLines.FindAll(l => l.Equals("</Settings>"))) {
        fileLines.Remove(line);
    }

    // remove duplicates of new setting
    foreach (string line in fileLines.FindAll(l => l.Equals(newSetting))) {
        fileLines.Remove(line);
    }

    // add stuff
    fileLines.Add(newSetting);
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
