namespace NekoKeepDB.Interfaces
{
    public interface IAccount
    {
        int Id { get; set; }
        int UserId { get; set; }
        string DisplayName { get; set; }
        string Email { get; set; }
        string? Note { get; set; }
    }
    public interface IOAuthAccount : IAccount
    {
        string Provider { get; set; }
    }

    public interface ICustomAccount : IAccount
    {
        string Password { get; set; }
    }

    public record OAuthAccountDto : IOAuthAccount
    {
        public int Id { get; set; }
        public required int UserId { get; set; }
        public required string DisplayName { get; set; }
        public required string Email { get; set; }
        public required string Provider { get; set; }
        public string? Note { get; set; }
    }

    public record CustomAccountDto : ICustomAccount
    {
        public int Id { get; set; }
        public required int UserId { get; set; }
        public required string DisplayName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? Note { get; set; }
    }
}
