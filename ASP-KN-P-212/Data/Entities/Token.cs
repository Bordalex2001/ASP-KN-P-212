namespace ASP_KN_P_212.Data.Entities
{
    public class Token
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime SubmitDt { get; set; }
        public DateTime ExpireDt { get; set; }

        public User User { get; set; } = null!;
    }
}