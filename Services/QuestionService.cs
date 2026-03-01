using VeilleNet.Data.SeedData;
using VeilleNet.Models;

namespace VeilleNet.Services;

public interface IQuestionService
{
    List<Question> GetAllQuestions();
    Question? GetQuestionById(int id);
    Question GetRandomQuestion(List<int> excludeIds);
}

public class QuestionService : IQuestionService
{
    private readonly List<Question> _questions;
    private List<Question> _shuffledQuestions;

    public QuestionService()
    {
        _questions = SeedDataLoader.Load<List<Question>>("questions.json");
        _shuffledQuestions = Shuffle(_questions);
    }

    public List<Question> GetAllQuestions() => _questions;

    public Question? GetQuestionById(int id) =>
        _questions.FirstOrDefault(q => q.Id == id);

    public Question GetRandomQuestion(List<int> excludeIds)
    {
        excludeIds ??= new List<int>();

        // Choose uniformly at random among non-excluded questions
        var candidates = _questions.Where(q => !excludeIds.Contains(q.Id)).ToList();
        if (candidates.Count > 0)
        {
            var idx = Random.Shared.Next(candidates.Count);
            return candidates[idx];
        }

        // All questions are excluded; reshuffle and return the first
        _shuffledQuestions = Shuffle(_questions);
        return _shuffledQuestions[0];
    }

    private static List<Question> Shuffle(List<Question> source)
    {
        var list = source.ToList();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}
