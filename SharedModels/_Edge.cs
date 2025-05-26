namespace SharedModels
{
    public class _Edge
    {
        public string From { get; set; }
        public string To { get; set; }
        public bool IsLayoutOnly { get; set; } = false; // Default to false
    }
}