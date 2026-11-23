namespace Pos.Domain;

public interface IFinalScorePolicy
{
    decimal Calculate(IReadOnlyList<Criterion> criteria, IReadOnlyList<Evaluation> evaluations);
}
