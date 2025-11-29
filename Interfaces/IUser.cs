namespace NekoKeepDB.Interfaces
{
    public interface IUser
    {
        int Id { get; set; }
        string DisplayName { get; set; }
        string Email { get; set; }
        int CatPresetId { get; set; }
    }

    public record UserDto : IUser
    {
        public int Id { get; set; }
        public required string DisplayName { get; set; }
        public required string Email { get; set; }
        public required int CatPresetId { get; set; }
    }
}
