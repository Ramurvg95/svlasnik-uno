namespace TeamManagement.Models
{
    public class TeamDetailsViewModel
    {
        public bool CanEditTeams {  get; set; }
               
        public FindUsersViewModel FindUsers { get; set; }

        public List<TeamMember> Members { get; set; }
        public string SelectedUserIds  { get; set; }
        public long TeamId { get; set; }
        public string TeamName  { get; set; }

    }
}
