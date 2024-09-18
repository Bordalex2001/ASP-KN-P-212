namespace ASP_KN_P_212.Models.Home.Ioc
{
    public class IocPageModel
    {
        public String Title { get; set; } = null!;
        public String SingleHash { get; set; } = null!;
        public Dictionary<String, String> Hashes { get; set; } = [];
    }
}
/* Д.З. Перевести усі представлення (View) проєкту на роботу з
 * власними моделями.
 * У директорії Models/Home створити директорії для кожної з 
 * сторінок, у них - класи XxxxPageModel, де Xxxx - назва представлення
 * HTML-розмітку представлень переробити на роботу з моделями.
 */