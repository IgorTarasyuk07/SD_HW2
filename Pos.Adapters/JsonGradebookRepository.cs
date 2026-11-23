using System.Text.Json;
using System.Text.Json.Serialization;
using Pos.Application;
using Pos.Domain;

namespace Pos.Adapters;

public sealed class JsonGradebookRepository : IGradebookRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _path;

    public JsonGradebookRepository(string path)
    {
        _path = Path.GetFullPath(path);
    }

    public Gradebook Load()
    {
        try
        {
            using var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            var data = JsonSerializer.Deserialize<GradebookData>(stream, JsonOptions);
            return Restore(data ?? throw new JsonException("Пустая ведомость."));
        }
        catch (FileNotFoundException)
        {
            return new Gradebook();
        }
        catch (Exception error) when (error is JsonException or DomainRuleException)
        {
            throw new IOException($"Не удалось прочитать ведомость «{_path}»: {error.Message}", error);
        }
    }

    public T Update<T>(Func<Gradebook, T> change)
    {
        using var writeLock = new FileStream(_path + ".lock", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        var book = Load();
        var result = change(book);
        var data = new GradebookData
        {
            Students = book.Students.Select(student => student.Name).ToArray(),
            Criteria = book.Criteria.Select(criterion =>
                new CriterionData(criterion.Name, criterion.MaxScore, criterion.Weight)).ToArray(),
            Evaluations = book.Evaluations
                .OrderBy(evaluation => evaluation.Student.Id)
                .ThenBy(evaluation => evaluation.Criterion.Id)
                .Select(evaluation =>
                    new EvaluationData(evaluation.Student.Id, evaluation.Criterion.Id, evaluation.Score.Value))
                .ToArray(),
        };

        AtomicFile.Write(_path, stream => JsonSerializer.Serialize(stream, data, JsonOptions));
        return result;
    }

    private static Gradebook Restore(GradebookData data)
    {
        if (data.Students is null || data.Criteria is null || data.Evaluations is null)
        {
            throw new JsonException("В ведомости отсутствуют обязательные списки.");
        }

        var book = new Gradebook();
        foreach (var name in data.Students)
        {
            book.AddStudent(name);
        }

        foreach (var criterion in data.Criteria)
        {
            if (criterion is null)
            {
                throw new JsonException("Пустой критерий в рубрикаторе.");
            }

            book.AddCriterion(criterion.Name, criterion.MaxScore, criterion.Weight);
        }

        foreach (var evaluation in data.Evaluations)
        {
            if (evaluation is null)
            {
                throw new JsonException("Пустая запись об оценке.");
            }

            book.SetEvaluation(evaluation.StudentId, evaluation.CriterionId, evaluation.Score);
        }

        return book;
    }

    private sealed class GradebookData
    {
        public required string[] Students { get; init; }

        public required CriterionData[] Criteria { get; init; }

        public required EvaluationData[] Evaluations { get; init; }
    }

    private sealed record CriterionData(string Name, decimal MaxScore, decimal Weight);

    private sealed record EvaluationData(int StudentId, int CriterionId, [property: JsonRequired] decimal Score);
}
