namespace Pos.Domain;

public readonly record struct Score
{
    public Score(decimal value)
    {
        if (value < 0)
        {
            throw new DomainRuleException("Балл не может быть отрицательным.");
        }

        Value = value;
    }

    public decimal Value { get; }
}
