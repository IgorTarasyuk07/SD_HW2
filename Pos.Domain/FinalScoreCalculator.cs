namespace Pos.Domain;

public sealed record FinalScore(decimal? Value, int EvaluatedCount, int CriterionCount);

public sealed class FinalScoreCalculator(IFinalScorePolicy policy)
{
    public FinalScore Calculate(Gradebook gradebook, int studentId)
    {
        var evaluations = gradebook.GetEvaluations(studentId);
        var criteria = gradebook.Criteria;
        decimal? value = criteria.Count == 0 ? null : policy.Calculate(criteria, evaluations);
        return new FinalScore(value, evaluations.Count, criteria.Count);
    }
}
