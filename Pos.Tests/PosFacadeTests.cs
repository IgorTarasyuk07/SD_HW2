using Pos.Adapters;
using Pos.Application;
using Pos.Domain;

namespace Pos.Tests;

public class PosFacadeTests : IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("pos-tests-");

    [Fact]
    public void ReloadAndExport_ReflectCurrentEvaluations()
    {
        var pos = CreateFacade();
        var student = pos.AddStudent("Ким, Анна \"А.\"");
        pos.AddStudent("Иван");
        var criterion = pos.AddCriterion("Проект", 20, 1);
        pos.SetEvaluation(student.Id, criterion.Id, 15);

        var reloaded = CreateFacade();
        Assert.Equal(student.Name, reloaded.GetStudents()[0].Name);
        Assert.Equal(15m, Assert.Single(reloaded.GetEvaluations(student.Id)).Score.Value);
        Assert.Throws<DomainRuleException>(() => reloaded.SetEvaluation(student.Id, criterion.Id, 21));

        var statistics = Assert.Single(reloaded.GetStatistics());
        Assert.Equal(1, statistics.EvaluatedCount);
        Assert.Equal(2, statistics.StudentCount);
        Assert.Equal(15m, statistics.AverageScore);

        var path = reloaded.ExportResults();
        Assert.Equal("1,\"Ким, Анна \"\"А.\"\"\",7.5,1,1", File.ReadAllLines(path)[1]);

        reloaded.RemoveEvaluation(student.Id, criterion.Id);
        Assert.Null(Assert.Single(pos.GetStatistics()).AverageScore);
        Assert.Equal("1,\"Ким, Анна \"\"А.\"\"\",0,0,1", File.ReadAllLines(pos.ExportResults())[1]);
    }

    private PosFacade CreateFacade() => new(
        new JsonGradebookRepository(Path.Combine(_directory.FullName, "gradebook.json")),
        new FinalScoreCalculator(new WeightedScorePolicy()),
        new CsvResultExporter(Path.Combine(_directory.FullName, "results.csv")));

    public void Dispose() => _directory.Delete(recursive: true);
}
