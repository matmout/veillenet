using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services;

namespace VeilleNet.Pages;

public class TrainingModel : PageModel
{
    private readonly IQuestionService _questionService;
    private const string SessionKey = "QuestionSession";

    [BindProperty]
    public string? UserAnswer { get; set; }

    [BindProperty]
    public int? CurrentQuestionId { get; set; }

    public Question? CurrentQuestion { get; set; }
    public QuestionSession Session { get; set; } = new();
    public bool ShowResult { get; set; }
    public bool IsCorrect { get; set; }
    public int TotalQuestions { get; set; }
    public int RemainingQuestions { get; set; }

    public TrainingModel(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    public void OnGet()
    {
        LoadSession();
        TotalQuestions = _questionService.GetAllQuestions().Count;
        RemainingQuestions = TotalQuestions - Session.AskedQuestionIds.Count;
        
        if (Session.AskedQuestionIds.Count == 0)
        {
            Session.StartTime = DateTime.Now;
        }

        LoadNextQuestion();
    }

    public IActionResult OnPostSubmitAnswer()
    {
        LoadSession();
        
        // Load the question that was answered
        if (CurrentQuestionId.HasValue)
        {
            CurrentQuestion = _questionService.GetQuestionById(CurrentQuestionId.Value);
        }

        if (CurrentQuestion != null && !string.IsNullOrWhiteSpace(UserAnswer))
        {
            // Normalize answers for comparison (case-insensitive, trim whitespace)
            var normalizedUserAnswer = UserAnswer.Trim();
            var normalizedCorrectAnswer = CurrentQuestion.CorrectAnswer.Trim();

            // Check if answer is correct
            if (string.Equals(normalizedUserAnswer, normalizedCorrectAnswer, StringComparison.OrdinalIgnoreCase))
            {
                Session.CorrectAnswers++;
                IsCorrect = true;
            }
            else
            {
                Session.IncorrectAnswers++;
                IsCorrect = false;
            }

            ShowResult = true;
            SaveSession();
        }

        TotalQuestions = _questionService.GetAllQuestions().Count;
        RemainingQuestions = TotalQuestions - Session.AskedQuestionIds.Count;

        return Page();
    }

    public IActionResult OnPostNextQuestion()
    {
        LoadSession();
        
        if (CurrentQuestion != null)
        {
            Session.AskedQuestionIds.Add(CurrentQuestion.Id);
            SaveSession();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostResetSession()
    {
        HttpContext.Session.Remove(SessionKey);
        return RedirectToPage();
    }

    private void LoadSession()
    {
        var sessionJson = HttpContext.Session.GetString(SessionKey);
        if (!string.IsNullOrEmpty(sessionJson))
        {
            Session = System.Text.Json.JsonSerializer.Deserialize<QuestionSession>(sessionJson) ?? new QuestionSession();
        }
        else
        {
            Session = new QuestionSession
            {
                StartTime = DateTime.Now
            };
        }
    }

    private void SaveSession()
    {
        var sessionJson = System.Text.Json.JsonSerializer.Serialize(Session);
        HttpContext.Session.SetString(SessionKey, sessionJson);
    }

    private void LoadNextQuestion()
    {
        LoadSession();
        
        // Get a random question that hasn't been asked yet
        CurrentQuestion = _questionService.GetRandomQuestion(Session.AskedQuestionIds);
        CurrentQuestionId = CurrentQuestion?.Id;
    }
}
