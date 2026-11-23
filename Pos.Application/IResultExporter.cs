namespace Pos.Application;

public interface IResultExporter
{
    string Export(IReadOnlyList<StudentResult> results);
}
