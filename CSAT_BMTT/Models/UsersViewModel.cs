namespace CSAT_BMTT.Models
{
    public class UsersViewModel
    {
        public User? CurrentUser { get; set; }
        public List<User> UsersList { get; set; } = new();
    }
}
