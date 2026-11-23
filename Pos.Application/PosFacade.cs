using Pos.Domain;

namespace Pos.Application;

public sealed class PosFacade(
    IGradebookRepository repository,
    FinalScoreCalculator calculator,
    IResultExporter exporter)
{
    public Student AddStudent(string name) =>
        repository.Update(book => book.AddStudent(name));

    public IReadOnlyList<Student> GetStudents() => repository.Load().Students;

    public Criterion AddCriterion(string name, decimal maxScore, decimal weight) =>
        repository.Update(book => book.AddCriterion(name, maxScore, weight));

    public IReadOnlyList<Criterion> GetCriteria() => repository.Load().Criteria;

    public Evaluation SetEvaluation(int studentId, int criterionId, decimal score) =>
        repository.Update(book => book.SetEvaluation(studentId, criterionId, score));

    public Evaluation RemoveEvaluation(int studentId, int criterionId) =>
        repository.Update(book => book.RemoveEvaluation(studentId, criterionId));

    public IReadOnlyList<Evaluation> GetEvaluations(int studentId) =>
        repository.Load().GetEvaluations(studentId);

    public StudentResult GetResult(int studentId)
    {
        var book = repository.Load();
        return new StudentResult(book.GetStudent(studentId), calculator.Calculate(book, studentId));
    }

    public IReadOnlyList<CriterionStatistics> GetStatistics()
    {
        var book = repository.Load();
        var byCriterion = book.Evaluations.ToLookup(evaluation => evaluation.Criterion.Id);

        return book.Criteria.Select(criterion =>
        {
            var count = 0;
            var average = 0m;
            foreach (var evaluation in byCriterion[criterion.Id])
            {
                count++;
                average += (evaluation.Score.Value - average) / count;
            }

            return new CriterionStatistics(criterion, count, book.Students.Count, count == 0 ? null : average);
        }).ToArray();
    }

    public string ExportResults()
    {
        var book = repository.Load();
        var results = book.Students
            .Select(student => new StudentResult(student, calculator.Calculate(book, student.Id)))
            .ToArray();
        return exporter.Export(results);
    }
}
