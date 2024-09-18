namespace ASP_KN_P_212.Data.Entities
{
    public class Category
    {
        public Guid      Id          { get; set; }
        public string    Name        { get; set; } = null!;
        public string    Description { get; set; } = null!;
        public DateTime? DeletedDt   { get; set; }  // ознака видалення

        public String?   PhotoUrl    { get; set; }
        public String?   Slug        { get; set; }   // slug - ідентифікатор ресурсу
    }
}