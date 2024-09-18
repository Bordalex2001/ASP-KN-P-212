namespace ASP_KN_P_212.Models.Content.Location
{
    public class ContentLocationPageModel
    {
        public Data.Entities.Location Location { get; set; } = null!;
        public List<Data.Entities.Room> Rooms { get; set; } = [];
    }
}
