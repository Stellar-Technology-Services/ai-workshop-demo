namespace ClaimsChat;

// Starter questions shown as clickable chips on the Chat page. Each one is
// deliberately phrased around a distinctive peril token (tornado, lightning,
// truck, vandalism) that lives in the same passage as its answer, so lexical
// retrieval reliably surfaces the intended claim. SuggestedQuestionTests proves
// each entry retrieves its target claim; keep that test in sync when editing.
public static class SuggestedQuestions
{
    public static readonly string[] All =
    {
        "What caused the damage in the tornado claim?",
        "Summarize the lightning strike claim.",
        "What damage did the vehicle cause to the garage?",
        "Tell me about the vandalism and graffiti claim.",
    };
}
