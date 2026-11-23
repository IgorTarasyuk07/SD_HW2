using Pos.Domain;

namespace Pos.Tests;

public class GradebookTests
{
    private readonly Gradebook _book = new();

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(10, 0)]
    [InlineData(10, -1)]
    public void AddCriterion_RejectsNonPositiveMaximumOrWeight(int maxScore, int weight)
    {
        Assert.Throws<DomainRuleException>(() => _book.AddCriterion("Проект", maxScore, weight));
        Assert.Empty(_book.Criteria);
    }

    [Fact]
    public void AddStudentAndCriterion_RejectBlankNames()
    {
        Assert.Throws<DomainRuleException>(() => _book.AddStudent("  "));
        Assert.Throws<DomainRuleException>(() => _book.AddCriterion("  ", 10, 1));
        Assert.Empty(_book.Students);
        Assert.Empty(_book.Criteria);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void SetEvaluation_AcceptsBothLimits(int score)
    {
        var student = _book.AddStudent("Анна");
        var criterion = _book.AddCriterion("Проект", 10, 1);

        _book.SetEvaluation(student.Id, criterion.Id, score);

        Assert.Equal((decimal)score, Assert.Single(_book.GetEvaluations(student.Id)).Score.Value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void SetEvaluation_InvalidCorrectionLeavesPreviousScore(int score)
    {
        var student = _book.AddStudent("Анна");
        var criterion = _book.AddCriterion("Проект", 10, 1);
        _book.SetEvaluation(student.Id, criterion.Id, 7);

        Assert.Throws<DomainRuleException>(() => _book.SetEvaluation(student.Id, criterion.Id, score));

        Assert.Equal(7m, Assert.Single(_book.GetEvaluations(student.Id)).Score.Value);
    }

    [Fact]
    public void SetEvaluation_RequiresKnownStudentAndCriterion()
    {
        var student = _book.AddStudent("Анна");
        var criterion = _book.AddCriterion("Проект", 10, 1);

        Assert.Throws<DomainRuleException>(() => _book.SetEvaluation(student.Id + 1, criterion.Id, 5));
        Assert.Throws<DomainRuleException>(() => _book.SetEvaluation(student.Id, criterion.Id + 1, 5));

        Assert.Empty(_book.Evaluations);
    }

    [Fact]
    public void Evaluation_CanBeCorrectedAndRemoved()
    {
        var student = _book.AddStudent("Анна");
        var criterion = _book.AddCriterion("Проект", 10, 1);
        _book.SetEvaluation(student.Id, criterion.Id, 2);

        _book.SetEvaluation(student.Id, criterion.Id, 8);
        Assert.Equal(8m, Assert.Single(_book.GetEvaluations(student.Id)).Score.Value);

        _book.RemoveEvaluation(student.Id, criterion.Id);
        Assert.Empty(_book.GetEvaluations(student.Id));
    }
}
