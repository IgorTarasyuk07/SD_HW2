namespace Pos.Domain;

public sealed class WeightedScorePolicy : IFinalScorePolicy
{
    public decimal Calculate(IReadOnlyList<Criterion> criteria, IReadOnlyList<Evaluation> evaluations)
    {
        // Масштабирование весов сохраняет их пропорции и защищает сумму от переполнения.
        var maxWeight = criteria.Max(criterion => criterion.Weight);
        var totalWeight = criteria.Sum(criterion => criterion.Weight / maxWeight);
        var weightedScore = evaluations.Sum(evaluation =>
            evaluation.Score.Value / evaluation.Criterion.MaxScore * (evaluation.Criterion.Weight / maxWeight));

        return weightedScore / totalWeight * 10;
    }
}
