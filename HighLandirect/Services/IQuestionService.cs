namespace HighLandirect.Services
{
    public interface IQuestionService
    {
        bool? ShowQuestion(string message);

        bool ShowYesNoQuestion(string message);
    }
}
