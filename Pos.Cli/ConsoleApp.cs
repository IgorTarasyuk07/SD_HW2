using System.Globalization;
using Pos.Application;
using Pos.Domain;

namespace Pos.Cli;

public sealed class ConsoleApp(PosFacade pos, TextReader input, TextWriter output)
{
    public void Run()
    {
        output.WriteLine("=== ПОС — преподавательская оценка студентов ===");

        while (true)
        {
            PrintMenu();
            try
            {
                switch (ReadLine("Выберите пункт: "))
                {
                    case "1":
                        AddStudent();
                        break;
                    case "2":
                        PrintStudents();
                        break;
                    case "3":
                        AddCriterion();
                        break;
                    case "4":
                        PrintCriteria();
                        break;
                    case "5":
                        SetEvaluation();
                        break;
                    case "6":
                        PrintEvaluations(ReadId("Номер студента: "));
                        break;
                    case "7":
                        RemoveEvaluation();
                        break;
                    case "8":
                        PrintResult(pos.GetResult(ReadId("Номер студента: ")));
                        break;
                    case "9":
                        PrintStatistics();
                        break;
                    case "10":
                        ExportResults();
                        break;
                    case "0":
                        return;
                    default:
                        output.WriteLine("Нет такого пункта.");
                        break;
                }
            }
            catch (EndOfStreamException)
            {
                output.WriteLine();
                output.WriteLine("Ввод закончился.");
                return;
            }
            catch (Exception error) when (error is DomainRuleException or IOException or UnauthorizedAccessException)
            {
                output.WriteLine($"Ошибка: {error.Message}");
            }
        }
    }

    public void RunDemo()
    {
        output.WriteLine("=== ПОС — демо ===");
        var anna = pos.AddStudent("Анна Ким");
        var ivan = pos.AddStudent("Иван Морозов");
        PrintStudents();

        var homework = pos.AddCriterion("Домашняя работа", 20, 3);
        var project = pos.AddCriterion("Проект", 10, 2);
        PrintCriteria();

        output.WriteLine("Выставляем оценки:");
        PrintEvaluation(pos.SetEvaluation(anna.Id, homework.Id, 15));
        PrintEvaluation(pos.SetEvaluation(anna.Id, project.Id, 9));
        PrintEvaluation(pos.SetEvaluation(ivan.Id, homework.Id, 12));
        PrintEvaluation(pos.SetEvaluation(ivan.Id, project.Id, 7));

        output.WriteLine("Пробуем выставить 12 из 10:");
        try
        {
            pos.SetEvaluation(anna.Id, project.Id, 12);
        }
        catch (DomainRuleException error)
        {
            output.WriteLine($"Оценка отклонена. {error.Message}");
        }

        output.WriteLine("Исправляем балл Анны за домашнюю работу и отменяем балл Ивана за проект:");
        PrintEvaluation(pos.SetEvaluation(anna.Id, homework.Id, 16));
        var removed = pos.RemoveEvaluation(ivan.Id, project.Id);
        output.WriteLine($"Отменена оценка: {removed.Student.Name}, «{removed.Criterion.Name}».");

        PrintResult(pos.GetResult(anna.Id));
        PrintResult(pos.GetResult(ivan.Id));
        PrintStatistics();
        ExportResults();
    }

    private void PrintMenu()
    {
        output.WriteLine();

        output.WriteLine("1. Добавить студента");
        output.WriteLine("2. Список студентов");
        output.WriteLine("3. Добавить критерий");
        output.WriteLine("4. Рубрикатор");
        output.WriteLine("5. Выставить или исправить оценку");
        output.WriteLine("6. Посмотреть оценки студента");
        output.WriteLine("7. Отменить оценку");
        output.WriteLine("8. Рассчитать итог студента");
        output.WriteLine("9. Аналитика по критериям");
        output.WriteLine("10. Экспортировать итоги в CSV");
        output.WriteLine("0. Выход");
    }

    private void AddStudent()
    {
        var student = pos.AddStudent(ReadLine("Имя студента: "));
        output.WriteLine($"Добавлен студент №{student.Id}: {student.Name}.");
    }

    private void PrintStudents()
    {
        var students = pos.GetStudents();
        output.WriteLine(students.Count == 0 ? "Список студентов пуст." : "Студенты:");
        foreach (var student in students)
        {
            output.WriteLine($"  №{student.Id} {student.Name}");
        }
    }

    private void AddCriterion()
    {
        var name = ReadLine("Название критерия: ");
        var maximum = ReadDecimal("Максимальный балл: ");
        var weight = ReadDecimal("Вес: ");
        var criterion = pos.AddCriterion(name, maximum, weight);
        output.WriteLine($"Добавлен критерий №{criterion.Id}: {criterion.Name}.");
    }

    private void PrintCriteria()
    {
        var criteria = pos.GetCriteria();
        output.WriteLine(criteria.Count == 0 ? "Рубрикатор пуст." : "Рубрикатор:");
        foreach (var criterion in criteria)
        {
            output.WriteLine($"  №{criterion.Id} {criterion.Name}, максимум {Format(criterion.MaxScore)}, вес {Format(criterion.Weight)}");
        }
    }

    private void SetEvaluation()
    {
        var studentId = ReadId("Номер студента: ");
        var criterionId = ReadId("Номер критерия: ");
        var score = ReadDecimal("Балл: ");
        PrintEvaluation(pos.SetEvaluation(studentId, criterionId, score));
    }

    private void RemoveEvaluation()
    {
        var studentId = ReadId("Номер студента: ");
        var criterionId = ReadId("Номер критерия: ");
        var removed = pos.RemoveEvaluation(studentId, criterionId);
        output.WriteLine($"Отменена оценка: {removed.Student.Name}, «{removed.Criterion.Name}».");
    }

    private void PrintEvaluations(int studentId)
    {
        var evaluations = pos.GetEvaluations(studentId);
        if (evaluations.Count == 0)
        {
            output.WriteLine("У студента пока нет оценок.");
            return;
        }

        foreach (var evaluation in evaluations)
        {
            PrintEvaluation(evaluation);
        }
    }

    private void PrintEvaluation(Evaluation evaluation)
    {
        var criterion = evaluation.Criterion;
        output.WriteLine($"{evaluation.Student.Name}, №{criterion.Id} «{criterion.Name}»: {Format(evaluation.Score.Value)} из {Format(criterion.MaxScore)}.");
    }

    private void PrintResult(StudentResult result)
    {
        var final = result.FinalScore;
        if (final.Value is null)
        {
            output.WriteLine($"{result.Student.Name}: итог не рассчитывается, рубрикатор пуст.");
            return;
        }

        output.WriteLine($"{result.Student.Name}: итог {FormatRounded(final.Value.Value)} из 10 (оценено {final.EvaluatedCount} из {final.CriterionCount}).");
    }

    private void PrintStatistics()
    {
        var statistics = pos.GetStatistics();
        output.WriteLine(statistics.Count == 0 ? "Рубрикатор пуст." : "Аналитика по критериям:");
        foreach (var item in statistics)
        {
            var average = item.AverageScore is decimal score
                ? $"{FormatRounded(score)} из {Format(item.Criterion.MaxScore)}"
                : "—";
            output.WriteLine($"  {item.Criterion.Name}: оценено {item.EvaluatedCount} из {item.StudentCount}, средний балл {average}");
        }
    }

    private void ExportResults()
    {
        output.WriteLine($"Итоги сохранены в {pos.ExportResults()}.");
    }

    private string ReadLine(string prompt)
    {
        output.Write(prompt);
        return input.ReadLine()?.Trim() ?? throw new EndOfStreamException();
    }

    private int ReadId(string prompt)
    {
        while (true)
        {
            if (int.TryParse(ReadLine(prompt), out var id) && id > 0)
            {
                return id;
            }

            output.WriteLine("Введите целый положительный номер.");
        }
    }

    private decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            var line = ReadLine(prompt).Replace(',', '.');
            const NumberStyles style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
            if (decimal.TryParse(line, style, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            output.WriteLine("Введите число, например 7,5.");
        }
    }

    private static string Format(decimal value) =>
        value.ToString("0.############################", CultureInfo.InvariantCulture);

    private static string FormatRounded(decimal value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);
}
