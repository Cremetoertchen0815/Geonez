using LocaliSaatana;
using Newtonsoft.Json;

if (args.Length < 1)
{
    Console.WriteLine("Please specify the project file to compile!");
    return;
}

if (!File.Exists(args[0]))
{
    Console.WriteLine("Project file does not exist!");
    return;
}

Project project;

try
{
    var data = await File.ReadAllTextAsync(args[0]);
    project = JsonConvert.DeserializeObject<Project>(data) ?? throw new InvalidDataException();
    project.FilePath = Path.GetFullPath(args[0]);
}
catch (Exception)
{
    Console.WriteLine("Project file is invalid!");
    throw;
}

await project.Build();

Console.WriteLine("Build successful!");