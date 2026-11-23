using Pos.Domain;

namespace Pos.Tests;

public class FinalScoreCalculatorTests
{
    [Fact]
    public void Calculate_UsesCurrentScoresMaximaAndRelativeWeights()
    {
        var book = new Gradebook();
        var student = book.AddStudent("Анна");
        var homework = book.AddCriterion("Домашнее задание", 20, 3);
        var project = book.AddCriterion("Проект", 10, 2);
        var calculator = new FinalScoreCalculator(new WeightedScorePolicy());

        Assert.Equal(new FinalScore(0, 0, 2), calculator.Calculate(book, student.Id));

        book.SetEvaluation(student.Id, homework.Id, 15);
        book.SetEvaluation(student.Id, project.Id, 9);
        Assert.Equal(new FinalScore(8.1m, 2, 2), calculator.Calculate(book, student.Id));

        book.SetEvaluation(student.Id, homework.Id, 16);
        Assert.Equal(new FinalScore(8.4m, 2, 2), calculator.Calculate(book, student.Id));

        book.RemoveEvaluation(student.Id, project.Id);
        Assert.Equal(new FinalScore(4.8m, 1, 2), calculator.Calculate(book, student.Id));

        book.SetEvaluation(student.Id, project.Id, 0);
        Assert.Equal(new FinalScore(4.8m, 2, 2), calculator.Calculate(book, student.Id));
    }

    [Fact]
    public void Calculate_EmptyRubricHasNoResult()
    {
        var book = new Gradebook();
        var student = book.AddStudent("Анна");
        var calculator = new FinalScoreCalculator(new WeightedScorePolicy());

        Assert.Equal(new FinalScore(null, 0, 0), calculator.Calculate(book, student.Id));
    }

    [Fact]
    public void Calculate_UsesInjectedPolicy()
    {
        var book = new Gradebook();
        var student = book.AddStudent("Анна");
        var criterion = book.AddCriterion("Проект", 10, 1);
        book.SetEvaluation(student.Id, criterion.Id, 6);
        var calculator = new FinalScoreCalculator(new RawScorePolicy());

        Assert.Equal(new FinalScore(6, 1, 1), calculator.Calculate(book, student.Id));
    }

    private sealed class RawScorePolicy : IFinalScorePolicy
    {
        public decimal Calculate(IReadOnlyList<Criterion> criteria, IReadOnlyList<Evaluation> evaluations)
        {
            Assert.Single(criteria);
            return Assert.Single(evaluations).Score.Value;
        }
    }
}
