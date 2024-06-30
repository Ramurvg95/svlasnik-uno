namespace TeamManagement.Models
{
    public class TeamMember
    {
        public string Name  { get; set; }
        public long TeamId  { get; set; }
        public string TeamMemberType  { get; set; }
        public long UserId  { get; set; }

        public string CreatedBy { get; set; }
    }
}
