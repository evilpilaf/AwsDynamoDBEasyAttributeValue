using McMaster.Extensions.CommandLineUtils;

using System;
using System.IO;
using System.Linq;

using static Bullseye.Targets;
using static SimpleExec.Command;

namespace Build
{
    public class Build
    {
        private static class BuildTarget
        {
            public const string Restore = nameof(Restore);
            public const string Build = nameof(Build);
            public const string Test = nameof(Test);
            public const string Pack = nameof(Pack);
            public const string Clean = nameof(Clean);
        }

        private static class BuildFolders
        {
            public static readonly string ArtifactsDir = Path.GetFullPath("artifacts");
            public static readonly string LogsDir = Path.Combine(ArtifactsDir, "logs");
            public static readonly string BuildLogFile = Path.Combine(LogsDir, "build.binlog");
            public static readonly string PackagesDir = Path.Combine(ArtifactsDir, "packages");
        }

        public static void Main(string[] args)
            => CommandLineApplication.Execute<Build>(args);

        [Option("-h|-?|--help", "Show help message", CommandOptionType.NoValue)]
        public bool ShowHelp { get; } = false;

        [Option("-v|--version", "The version to build", CommandOptionType.SingleValue)]
        public string Version { get; } = null;

        [Option("-c|--configuration", "The configuration to build", CommandOptionType.SingleValue)]
        public string Configuration { get; } = "Release";

        public string[] RemainingArguments { get; } = null;

        public void OnExecute(CommandLineApplication app)
        {
            if (ShowHelp)
            {
                app.ShowHelp();
                app.Out.WriteLine("Bullseye help:");
                app.Out.WriteLine();
                RunTargetsAndExit(new[] { "-h" });
                return;
            }
            Directory.SetCurrentDirectory(GetSolutionDirectory());

            const string solutionFile = "DynamoDBTransactionUtilities.sln";

            var version = GetVersion();

            Target(BuildTarget.Clean, () =>
            {
                CleanDirectory(BuildFolders.ArtifactsDir);
                CleanDirectory(BuildFolders.LogsDir);
                CleanDirectory(BuildFolders.PackagesDir);
                Run("dotnet", $"clean {solutionFile}");
            });

            Target(BuildTarget.Restore, () =>
                Run("dotnet", $"restore {solutionFile}"));

            Target(
                BuildTarget.Build,
                DependsOn(BuildTarget.Restore),
                () => Run(
                    "dotnet",
                    $"build -c \"{Configuration}\" /p:Version=\"{version}\" /bl:\"{BuildFolders.BuildLogFile}\" \"{solutionFile}\""));

            Target(
                BuildTarget.Test,
                DependsOn(BuildTarget.Build),
                () => Run(
                    "dotnet",
                    $"test -c \"{Configuration}\" --no-build \"{solutionFile}\""));

            Target(
                BuildTarget.Pack,
                DependsOn(BuildTarget.Test),
                () =>
                {
                    var project = Directory.GetFiles("./src", "*.csproj", SearchOption.AllDirectories).First();

                    Run("dotnet",
                        $"pack -c \"{Configuration}\" --no-build /p:Version=\"{version}\" -o \"{BuildFolders.PackagesDir}\" \"{project}\"");
                });

            Target("default", DependsOn(BuildTarget.Pack));

            RunTargetsAndExit(RemainingArguments);

            string GetVersion()
            {
                return !string.IsNullOrEmpty(Version) ? Version : "0.0.0.1";
            }
        }

        private static string GetSolutionDirectory()
        {
            var currentPath = Directory.GetCurrentDirectory();
            Console.WriteLine($"Current path is: {currentPath}");
            if (currentPath.Contains("build"))
            {
                return currentPath.Substring(0, currentPath.LastIndexOf("build", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return currentPath;
            }
        }

        private static void CleanDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                foreach (var file in Directory.EnumerateFiles(directory))
                {
                    new FileInfo(file).Delete();
                }

                foreach (var dir in Directory.GetDirectories(directory))
                {
                    CleanDirectory(dir);
                }
            }
        }
    }
}
