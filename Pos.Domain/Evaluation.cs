namespace Pos.Domain;

public sealed class Evaluation
{
    private Evaluation(Student student, Criterion criterion, Score score)
    {
        Student = student;
        Criterion = criterion;
        Score = score;
    }

    public Student Student { get; }

    public Criterion Criterion { get; }

    public Score Score { get; }

    internal static Evaluation Create(Student student, Criterion criterion, decimal value)
    {
        var score = new Score(value);
        if (score.Value > criterion.MaxScore)
        {
            throw new DomainRuleException($"Балл превышает максимум по критерию «{criterion.Name}» ({criterion.MaxScore}).");
        }

        return new Evaluation(student, criterion, score);
    }
}
