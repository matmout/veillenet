namespace VeilleNet.Models;

public enum QuestionType
{
    MultipleChoice,
    FreeText
}

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public List<string> Choices { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}

public class QuestionSession
{
    public List<int> AskedQuestionIds { get; set; } = new();
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public DateTime StartTime { get; set; }
}
