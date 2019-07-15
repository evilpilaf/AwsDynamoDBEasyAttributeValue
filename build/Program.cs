using McMaster.Extensions.CommandLineUtils;

using System;
using System.IO;
using System.Linq;

using static Bullseye.Targets;
using static SimpleExec.Command;

namespace Build
{
    public class Program
    {
        private const string ArtifactsDir = "artifacts";

        private const string Restore = nameof(Restore);
        private const string Build = nameof(Build);
        private const string Test = nameof(Test);
        private const string Pack = nameof(Pack);

        public static void Main2(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false);

            var configuration = GetConfiguration(app);
            var solution = Directory.GetFiles(".", "*.sln", SearchOption.TopDirectoryOnly).First();

            CleanArtifacts();

            app.OnExecute(() =>
            {
                Target(Restore, () =>
                {
                    Run("dotnet", $"restore {solution}");
                });

                Target(Build, DependsOn(Restore), () =>
                {
                    Run("dotnet", $"build {solution} -c {configuration}");
                });

                Target(Test, DependsOn(Build), () =>
                {
                    Run("dotnet", $"test {solution} -c {configuration} --collect \"Code coverage\" --no-build");
                });

                Target(Pack, DependsOn(Test), () =>
                {
                    var project = Directory.GetFiles("./src", "*.csproj", SearchOption.AllDirectories).First();

                    Run("dotnet", $"pack {project} -c {configuration} -o ../../{ArtifactsDir} --no-build");
                });

                Target("default", DependsOn(Test));
                RunTargetsAndExit(app.RemainingArguments);
            });

            app.Execute(args);
        }

        private static Configuration GetConfiguration(CommandLineApplication app)
        {
            var configuration = app.Option<Configuration>("-c", "", CommandOptionType.SingleValue);
            return configuration.HasValue() ?
                            (Configuration)Enum.Parse(typeof(Configuration), configuration.Value()) :
                            Configuration.Release;
        }

        private static void CleanArtifacts()
        {
            Directory.CreateDirectory($"./{ArtifactsDir}");

            foreach (var file in Directory.GetFiles($"./{ArtifactsDir}"))
            {
                File.Delete(file);
            }
        }
    }

    public enum Configuration
    {
        Debug,
        Release
    }
}
