namespace NekoKeepDB.Interfaces
{
    public interface ITag
    {
        int Id { get; set; }
        int UserId { get; set; }
        string? DisplayName { get; set; }
    }

    public record TagDto : ITag
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? DisplayName { get; set; }
    }
}
