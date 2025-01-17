namespace BaseLibrary.Entities
{
    public class RefreshTokenInfo
    {
        public int Id { get; set; }
        public string? RefreshToken { get; set; } 
        public int UserId { get; set; }
    }
}
