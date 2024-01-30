namespace DBL.Identity
{
    public class AuthorizationResult
    {
        public bool Succeded { get; set; }
        public string? RefreshToken { get; set; }
        public string? AccessToken { get; set; }
        public string? Error { get; set; }

    }
}
