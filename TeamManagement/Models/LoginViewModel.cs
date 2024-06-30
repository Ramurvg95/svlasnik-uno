

namespace TeamManagement.Models
{
    using System.ComponentModel.DataAnnotations;
    /// <summary>
    /// DTO for Login view
    /// </summary>
    public class LoginViewModel
    {
        ///<summary>
        ///Gets or sets the password.
        ///</summary>
        [Required(ErrorMessage = "Please enter password.")]
        public string Password  { get; set; }

        ///<summary>
        ///Gets or sets the user name.
        ///</summary>

        [Required(ErrorMessage = "Please enter user name.")]
        public string UserName  { get; set; }


    }
}
