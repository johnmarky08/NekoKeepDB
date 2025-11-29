namespace NekoKeepDB.Interfaces
{
    public interface ITag
    {
        int Id { get; set; }
        string? DisplayName { get; set; }
    }

    public record TagDto : ITag
    {
        public int Id { get; set; }
        public string? DisplayName { get; set; }
    }
}
