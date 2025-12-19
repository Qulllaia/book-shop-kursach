namespace models
{
    public class User
    {
        public long userid { get; set; }
        public string login { get; set; }
        public string email { get; set; }

        public string password { get; set; }

        public string role { get; set; }
    }
}
