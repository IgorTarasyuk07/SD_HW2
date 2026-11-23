namespace Pos.Domain;

public sealed class Criterion
{
    internal Criterion(int id, string name, decimal maxScore, decimal weight)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleException("Название критерия не может быть пустым.");
        }

        if (maxScore <= 0)
        {
            throw new DomainRuleException("Максимальный балл должен быть больше нуля.");
        }

        if (weight <= 0)
        {
            throw new DomainRuleException("Вес критерия должен быть больше нуля.");
        }

        Id = id;
        Name = name.Trim();
        MaxScore = maxScore;
        Weight = weight;
    }

    public int Id { get; }

    public string Name { get; }

    public decimal MaxScore { get; }

    public decimal Weight { get; }
}
