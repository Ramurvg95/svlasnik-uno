namespace TeamManagement.Models
{
    public class TeamDashboardViewModel
    {
        public bool IsAdmin {  get; set; }

        public bool IsManager { get; set; }
        public List<Team> Teams { get; set; }
    }
}
