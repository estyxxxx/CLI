using System.CommandLine;

var bundleCommand = new Command("bundle", "Bundle code files to a single file");
var createRspCommand = new Command("create-rsp", "Create a response file for the bundle command");

var outputOption = new Option<FileInfo>(new[] { "--output", "-o" }, "File path and name");
var languageOption = new Option<string[]>(new[] { "--language", "-l" }, "List of programming languages");
languageOption.AllowMultipleArgumentsPerToken = true;
var noteOption = new Option<bool>(new[] { "--note", "-n" }, "Add a note to the bundle file");
var sortOption = new Option<string>(new[] { "--sort", "-s" }, getDefaultValue: () => "abc", "Sort files by name (abc) or language");
var removeEmptyLinesOption = new Option<bool>(new[] { "--remove-empty-lines", "-r" }, "Remove empty lines from code before bundling");
var authorOption = new Option<string>(new[] { "--author", "-a" }, "Add author");

bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);
createRspCommand.AddOption(outputOption);

bundleCommand.SetHandler((output, languages, note, sort, removeEmptyLines, author) =>
{
    try
    {
        List<string> codeFiles = new List<string>();
        if (languages[0] == "all")
            codeFiles.AddRange(Directory.GetFiles(".\\", "*", SearchOption.AllDirectories));
        else foreach (string language in languages)
                codeFiles.AddRange(Directory.GetFiles(".\\", $"*.{language}", SearchOption.AllDirectories));
        if (codeFiles.Count == 0)
        {
            Console.WriteLine("ERROR: No files found to concatenate");
            return;
        }
        if (sort == "abc")
        {
            codeFiles = codeFiles.OrderBy(f => f).ToList();
        }
        else if (sort == "language")
        {
            codeFiles = codeFiles.OrderBy(f => Path.GetExtension(f)).ToList();
        }
        using (var outputFile = File.CreateText(output.FullName))
        {
            outputFile.WriteLine("// Author: " + author);
            foreach (var file in codeFiles)
            {
                if (note != null)
                {
                    outputFile.WriteLine("// File: " + Path.GetFileName(file));
                    outputFile.WriteLine("// Location: " + Path.GetFullPath(file));
                    outputFile.WriteLine();
                }
                var fileContent = File.ReadAllText(file);
                if (removeEmptyLines)
                {
                    fileContent = string.Join("", fileContent.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)));
                }
                outputFile.WriteLine(fileContent);
                outputFile.WriteLine();
            }
        }
        Console.WriteLine("Bundle created successfully: " + output.FullName);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error creating bundle: " + ex.Message);
    }
}, outputOption, languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

createRspCommand.SetHandler(() =>
{
    try
    {
        var responseFile = new FileInfo("responseFile.rsp");
        Console.WriteLine("Enter values for the bundle command:");
        using (StreamWriter rspWriter = new StreamWriter(responseFile.FullName))
        {
            Console.Write("Output file path: ");
            var Output = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(Output))
            {
                Console.Write("Enter the output file path: ");
                Output = Console.ReadLine();
            }
            rspWriter.WriteLine($"--output {Output}");
            Console.Write("Languages (comma-separated): ");
            var languages = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(languages))
            {
                Console.Write("Please enter at least one programming language: ");
                languages = Console.ReadLine();
            }
            rspWriter.WriteLine($"--language {languages}");
            Console.Write("Add note (y/n): ");
            rspWriter.WriteLine(Console.ReadLine().Trim().ToLower() == "y" ? "--note" : "");
            Console.Write("Sort by (abc or language): ");
            rspWriter.WriteLine($"--sort {Console.ReadLine()}");
            Console.Write("Remove empty lines (y/n): ");
            rspWriter.WriteLine(Console.ReadLine().Trim().ToLower() == "y" ? "--remove-empty-lines" : "");
            Console.Write("Author: ");
            rspWriter.WriteLine($"--author {Console.ReadLine()}");
        }
        Console.WriteLine("Response file created successfully: "+ responseFile.FullName);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error creating response file: " + ex.Message);
    }
});

var rootCommand = new RootCommand("Root command for file Bundler CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);
rootCommand.InvokeAsync(args);