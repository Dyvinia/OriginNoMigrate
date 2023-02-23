using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

// die :3
Console.WriteLine("Killing Origin and EA Desktop");
foreach (Process process in Process.GetProcessesByName("EADesktop")) process.Kill();
foreach (Process process in Process.GetProcessesByName("Origin")) process.Kill();

string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Origin\local.xml");
string newSetting = "  <Setting value=\"true\" key=\"MigrationDisabled\" type=\"1\"/>";

try {
    Console.WriteLine($"Opening {path}");
    List<string> fileLines = new(File.ReadAllLines(path));

    // remove xml closer (and any accidental duplicates of it)
    foreach (string line in fileLines.FindAll(l => l == "</Settings>")) {
        fileLines.Remove(line);
    }

    // remove duplicates of new setting
    foreach (string line in fileLines.FindAll(l => l == newSetting)) {
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
