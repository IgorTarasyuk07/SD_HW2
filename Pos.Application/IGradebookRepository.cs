using Pos.Domain;

namespace Pos.Application;

public interface IGradebookRepository
{
    Gradebook Load();

    T Update<T>(Func<Gradebook, T> change);
}
