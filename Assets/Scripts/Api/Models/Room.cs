namespace Assets.Scripts.Api.Models
{
    public class Room
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        
        public int? ThumbnailAssetId { get; set; }
    }
}
