using Microsoft.Extensions.DependencyInjection;
using Pos.Adapters;
using Pos.Application;
using Pos.Domain;

namespace Pos.Cli;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            Run(args.Contains("--demo"));
            return 0;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or DomainRuleException)
        {
            Console.Error.WriteLine($"Ошибка: {error.Message}");
            return 1;
        }
    }

    private static void Run(bool demo)
    {
        var demoDirectory = demo ? Directory.CreateTempSubdirectory("pos-demo-") : null;
        try
        {
            var repositoryPath = demoDirectory is null
                ? "gradebook.json"
                : Path.Combine(demoDirectory.FullName, "gradebook.json");

            var services = new ServiceCollection();
            services.AddSingleton<IGradebookRepository>(_ => new JsonGradebookRepository(repositoryPath));
            services.AddSingleton<IResultExporter>(_ => new CsvResultExporter(demo ? "demo-results.csv" : "results.csv"));
            services.AddSingleton<IFinalScorePolicy, WeightedScorePolicy>();
            services.AddSingleton<FinalScoreCalculator>();
            services.AddSingleton<PosFacade>();
            services.AddSingleton(Console.In);
            services.AddSingleton(Console.Out);
            services.AddSingleton<ConsoleApp>();

            using var container = services.BuildServiceProvider();
            var app = container.GetRequiredService<ConsoleApp>();
            if (demo)
            {
                app.RunDemo();
            }
            else
            {
                app.Run();
            }
        }
        finally
        {
            demoDirectory?.Delete(recursive: true);
        }
    }
}
