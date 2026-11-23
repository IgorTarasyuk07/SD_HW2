namespace Pos.Domain;

public sealed class Gradebook
{
    private readonly List<Student> _students = new();
    private readonly List<Criterion> _criteria = new();
    private readonly Dictionary<(int StudentId, int CriterionId), Evaluation> _evaluations = new();

    public IReadOnlyList<Student> Students => _students.AsReadOnly();

    public IReadOnlyList<Criterion> Criteria => _criteria.AsReadOnly();

    public IReadOnlyCollection<Evaluation> Evaluations => _evaluations.Values;

    public Student AddStudent(string name)
    {
        var student = new Student(_students.Count + 1, name);
        _students.Add(student);
        return student;
    }

    public Criterion AddCriterion(string name, decimal maxScore, decimal weight)
    {
        var criterion = new Criterion(_criteria.Count + 1, name, maxScore, weight);
        _criteria.Add(criterion);
        return criterion;
    }

    public Student GetStudent(int id)
    {
        if (id <= 0 || id > _students.Count)
        {
            throw new DomainRuleException($"Студент №{id} не найден.");
        }

        return _students[id - 1];
    }

    public Evaluation SetEvaluation(int studentId, int criterionId, decimal score)
    {
        var student = GetStudent(studentId);
        var criterion = GetCriterion(criterionId);
        var evaluation = Evaluation.Create(student, criterion, score);
        _evaluations[(studentId, criterionId)] = evaluation;
        return evaluation;
    }

    public Evaluation RemoveEvaluation(int studentId, int criterionId)
    {
        GetStudent(studentId);
        GetCriterion(criterionId);
        if (!_evaluations.Remove((studentId, criterionId), out var evaluation))
        {
            throw new DomainRuleException("У студента нет оценки по этому критерию.");
        }

        return evaluation;
    }

    public IReadOnlyList<Evaluation> GetEvaluations(int studentId)
    {
        GetStudent(studentId);
        return _evaluations.Values
            .Where(evaluation => evaluation.Student.Id == studentId)
            .OrderBy(evaluation => evaluation.Criterion.Id)
            .ToArray();
    }

    private Criterion GetCriterion(int id)
    {
        if (id <= 0 || id > _criteria.Count)
        {
            throw new DomainRuleException($"Критерий №{id} отсутствует в рубрикаторе.");
        }

        return _criteria[id - 1];
    }
}
