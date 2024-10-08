﻿using System.Text.Json.Serialization;

namespace ASP_KN_P_212.Data.Entities
{
    public class Reservation
    {
        public Guid     Id      { get; set; }
        public Guid     RoomId  { get; set; }
        public Guid     UserId  { get; set; }
        public DateTime Date    { get; set; }
        public Double   Price   { get; set; }
        public DateTime OrderDt { get; set; }
        public DateTime? DeletedDt { get; set; } // ознака видалення: null-активне

        // NAVIGATION PROPS
        [JsonIgnore] public User User { get; set; }
        [JsonIgnore] public Room Room { get; set; }
    }
}