using Pos.Domain;

namespace Pos.Application;

public sealed record StudentResult(Student Student, FinalScore FinalScore);

public sealed record CriterionStatistics(Criterion Criterion, int EvaluatedCount, int StudentCount, decimal? AverageScore);
