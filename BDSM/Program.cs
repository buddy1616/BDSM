using BDSM;
using System.IO;
using System.Text;

if (args.Length < 2)
{
    Console.WriteLine("Must provide compilation arguments: bdsm.exe [pathToSourceFile] [pathToDestinationFile]");
    return;
}

string sourcePath = args[0];
string destinationpath = args[1];
bool overwrite = false;
bool showOutput = false;

if (args.Length > 2)
{
    for (int i = 2; i < args.Length; i++)
    {
        if (args[i].ToLower() == "-o") { overwrite = true; continue; }
        if (args[i].ToLower() == "-s") { showOutput = true; continue; }
    }
}

if (Directory.Exists(sourcePath))
{
    Console.WriteLine("Error: Source path is a directory, it must point to a specific file.");
    return;
}
if (!File.Exists(sourcePath))
{
    Console.WriteLine("Error: Path to source file does not exist.");
    return;
}
if (Directory.Exists(destinationpath))
{
    Console.WriteLine("Error: Destination path is a directory, it must reference an existing file to overwrite or new file to create.");
    return;
}
if (File.Exists(destinationpath) && !overwrite)
{
    Console.WriteLine("Error: Destination file already exists, run with -o to overwrite.");
    return;
}

string[] lines = File.ReadAllLines(sourcePath);
if (lines.Length == 0)
{
    Console.WriteLine("Error: Source file is empty.");
    return;
}

BdsmCodeFile codeFile = new BdsmCodeFile(lines);
codeFile.Compile();

if (codeFile.WarningMessages.Count > 0)
{
    Console.WriteLine("WARNING:");
    for (int i = 0; i < codeFile.WarningMessages.Count; i++)
    {
        Console.WriteLine(" - " + codeFile.WarningMessages[i]);
    }
}

if (!codeFile.Success)
{
    Console.WriteLine(Environment.NewLine + "ERROR:");
    for (int i=0;i<codeFile.ErrorMessages.Count;i++)
    {
        Console.WriteLine(" - " + codeFile.ErrorMessages[i]);
    }
    return;
}
if (codeFile.Messages.Count > 0)
{
    Console.WriteLine(Environment.NewLine);
    for (int i = 0; i < codeFile.Messages.Count; i++)
    {
        Console.WriteLine(" - " + codeFile.Messages[i]);
    }
}

if (showOutput)
{

    Console.WriteLine(Environment.NewLine + "LABELS:");
    foreach (string l in codeFile.LabelAddresses.Keys)
    {
        Console.WriteLine(" - " + l + " " + codeFile.LabelAddresses[l].ToString());
    }

    Console.WriteLine(Environment.NewLine + "VARIABLES:");
    foreach (string v in codeFile.Variables.Keys)
    {
        List<int> intValue = new List<int>();
        List<string> binValue = new List<string>();
        string strValue = Encoding.ASCII.GetString(codeFile.Variables[v].Value);
        for (int i = 0; i < codeFile.Variables[v].Value.Length; i++)
        {
            int iv = codeFile.Variables[v].Value[i];
            intValue.Add(iv);
            binValue.Add(Convert.ToString(iv, 2));
        }
        Console.WriteLine(" - " + v + " " + codeFile.Variables[v].Type.ToString() + " = " + String.Join(", ", intValue) + " | " + String.Join(", ", binValue) + " | " + strValue);
    }

    Console.WriteLine(Environment.NewLine + "PROGRAM:");
    for (int i = 0; i < codeFile.ProgramBytes.Count; i++)
    {
        Console.WriteLine(" - " + Convert.ToString(codeFile.ProgramBytes[i], 2).PadLeft(8, '0'));
    }
}


BinaryWriter writer = new BinaryWriter(File.Open(destinationpath, FileMode.Create));
writer.Write(codeFile.ProgramBytes.ToArray());
writer.Flush();
writer.Close();

Console.WriteLine(Environment.NewLine + "Compilation complete.");
Console.WriteLine("File: " + destinationpath + " created.");