namespace TeamManagement.Models
{
    public class CreateNewTeamViewModel
    {
        public ErrorViewModel ErrorViewModel { get; set; }

        ///<summary>
        ///Gets or sets the findusers.
        ///</summary>
        
        public FindUsersViewModel FindUsers {  get; set; }

        public string  SelectedUserIds  { get; set; }
        public string TeamName  { get; set; }
        public string TeamNameErrorMessage  { get; set; }
        public string TeamUsersErrorMessage  { get; set; }

    }
}
