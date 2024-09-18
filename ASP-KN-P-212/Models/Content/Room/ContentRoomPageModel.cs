
namespace ASP_KN_P_212.Models.Content.Room
{
    public class ContentRoomPageModel
    {
        public Data.Entities.Room Room { get; set; } = null!;
        public int Year { get; set; }
        public int Month { get; set; }
        public int? Day { get; set; }
    }
}
