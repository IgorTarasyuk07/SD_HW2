using System.Globalization;
using System.Text;
using Pos.Application;

namespace Pos.Adapters;

public sealed class CsvResultExporter : IResultExporter
{
    private readonly string _path;

    public CsvResultExporter(string path)
    {
        _path = Path.GetFullPath(path);
    }

    public string Export(IReadOnlyList<StudentResult> results)
    {
        AtomicFile.Write(_path, stream =>
        {
            using var writer = new StreamWriter(stream, new UTF8Encoding(true), leaveOpen: true);
            writer.NewLine = "\r\n";
            writer.WriteLine("student_id,student_name,final_score,evaluated_criteria,total_criteria");

            foreach (var result in results)
            {
                var final = result.FinalScore;
                var name = result.Student.Name.Replace("\"", "\"\"");
                writer.WriteLine(FormattableString.Invariant(
                    $"{result.Student.Id},\"{name}\",{final.Value?.ToString("0.##", CultureInfo.InvariantCulture)},{final.EvaluatedCount},{final.CriterionCount}"));
            }
        });

        return _path;
    }
}
