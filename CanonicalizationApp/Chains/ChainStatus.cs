namespace CanonicalizationApp.Chains
{
    public class ChainStatus
    {
        public ChainStatus()
        {
            IsValid = true;
        }

        public ChainStatus(string message)
        {
            Message = message;
        }

        public bool IsValid { get; }

        public string Message { get; }
    }
}