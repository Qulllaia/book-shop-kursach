namespace dtos
{
    public record LoginRequest(string login, string password);

    public record RegRequest(string email, string login, string password);

    public class JWTClaims
    {
        public long userid { get; set; }
        public string email { get; set; }
        public string login { get; set; }
        public string role { get; set; }
    }

    public record AuthResponse(string token);
}
