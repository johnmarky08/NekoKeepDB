namespace NekoKeepDB.Interfaces
{
    public interface IFilter
    {
        int AccountId { get; set; }
        int TagId { get; set; }
    }

    public record FilterDto : IFilter
    {
        public required int AccountId { get; set; }
        public required int TagId { get; set; }
    }
}
