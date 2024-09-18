using System.Text.Json.Serialization;

namespace ASP_KN_P_212.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public String Name { get; set; }
        public String Email { get; set; }
        public String? EmailConfirmCode { get; set; }  // code OR null:  null - ознака підтвердження
        public String? AvatarUrl { get; set; }
        [JsonIgnore] public String Salt { get; set; } // за RFC-2898
        [JsonIgnore] public String DerivedKey { get; set; }  // за RFC-2898
        public DateTime? Birthdate { get; set; }
        public DateTime? DeletedDt { get; set; }
        public String? Role { get; set; }

        [JsonIgnore] public List<Reservation> Reservations { get; set; }
    }
}
/*
 * Category: Hotels Apartments Resorts Villas
 * Location: Hotel1 Hotel2 Hotel3
 * Room: Room101 Room102 Room103
 * 
 * 
 */