namespace NekoKeepDB.Interfaces
{
    public interface IUser
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public int CatPresetId { get; set; }
    }

    public record UserDto : IUser
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public int CatPresetId { get; set; }
    }
}
