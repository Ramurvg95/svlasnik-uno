
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using MimeKit;
using MimeKit.Text;
using TeamManagement.Models;
using TeamManagement.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;


namespace TeamManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataAccessService dataAccessService;

        public HomeController()
        {
            this.dataAccessService = new DataAccessService();
        }

        [Authorize]
        [HttpPost]

        public async Task<IActionResult> AddTeam([FromBody] CreateNewTeamViewModel model)
        {

            Claim userName = this.HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(model.TeamName))
            {
                return Json(new { TeamNameErrorMessage = "Please enter team name" });
            }
            else if (string.IsNullOrWhiteSpace(model.SelectedUserIds))
            {
                return Json(new { TeamUsersErrorMessage = "Please select members to add to team" });
            }

            Team _newTeam = new();
            _newTeam.Name = model.TeamName;
            _newTeam.CreatedBy = userName.Value;

            await this.dataAccessService.SaveTeamToDbAsync(_newTeam);

            // fetch the team again to get the team id for team members
            Team _team = await this.dataAccessService.GetTeamAsync(0, _newTeam.Name);
            List<string> _selectedUserIds = model.SelectedUserIds.Contains(",") ? model.SelectedUserIds.Split(',').ToList() : new List<string> { model.SelectedUserIds };

            List<long> _userIds = new List<long>();
            foreach (string s in _selectedUserIds)
            {
                _userIds.Add(Convert.ToInt64(s));
            }

            List<User> _users = await this.dataAccessService.FindTeamMembersAsync("", _userIds.ToArray());

            if (_users.Count != _userIds.ToHashSet().Count)
            {
                return Json(new { TeamUsersErrorMessage = "Invalid users passed to add to team" });
            }

            foreach (User user in _users)
            {
                await this.dataAccessService.SaveTeamMemberToDbAsync(new TeamMember { TeamMemberType = user.MemberType, CreatedBy = userName.Value, TeamId = _team.TeamId, UserId = user.UserId });
                 this.SendEmailAsync(user.Email, model.TeamName, true);
            }
            return Json(new { redirectToUrl = Url.Action("Index", "Home") });

        }

        [Authorize]
        [HttpGet]
        public IActionResult CreateNew()
        {
            CreateNewTeamViewModel _createNew = new();
            _createNew.FindUsers = new();
            _createNew.FindUsers.UserResults = null;
            _createNew.ErrorViewModel = new();

            return View(_createNew);
        }

        [Authorize]
        [HttpGet]

        public async Task<IActionResult> EditViewTeams(long teamId)
        {
            TeamDetailsViewModel _teamDetails = new();
            _teamDetails.TeamId = teamId;
            _teamDetails.Members = new List<TeamMember>();
            _teamDetails.CanEditTeams = RoleService.HasRole(this.User.Claims, "TeamAdministrator") || (this.User.Claims.FirstOrDefault(m => m.Type == "MemberType" && m.Value == "Manager") != null);
            _teamDetails.FindUsers = new();
            _teamDetails.FindUsers.UserResults = null;
            List<TeamMember> _teamMembers = await this.dataAccessService.GetTeamMembersAsync(teamId);
            Team _team = await this.dataAccessService.GetTeamAsync(teamId);
            _teamDetails.TeamName = _team.Name;
            foreach (TeamMember member in _teamMembers)
            {
                _teamDetails.Members.Add(member);
            }
            return View(_teamDetails);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        [HttpGet]

        public async Task<PartialViewResult> FindUsers(string searchText)
        {
            CreateNewTeamViewModel _viewModel = new();
            _viewModel.FindUsers = new();
            _viewModel.FindUsers.UserResults = new();
            _viewModel.ErrorViewModel = new();

            List<User> _users = await this.dataAccessService.FindTeamMembersAsync(searchText);
            foreach (User user in _users)
            {
                _viewModel.FindUsers.UserResults.Add(new UserResult {MemberType = user.MemberType, Email = user.Email, FirstName = user.FirstName, LastName = user.LastName, UserName = user.UserName, UserId = user.UserId });

            }
            return PartialView("UserResults", _viewModel.FindUsers);
        }
        [Authorize]
        [HttpGet]

        public async Task<IActionResult> Index()
        {
            bool _canEditTeams = RoleService.HasRole(this.User.Claims, "TeamAdministrator");
            long? userId = null;
            if (!_canEditTeams)
            {
                userId = Convert.ToInt64(this.User.Claims.FirstOrDefault(m => m.Type == "UserId").Value);

            }

            //if the user is admin, then no need to filter teams
            List<Team> _teams = await dataAccessService.GetTeamsAsync(_canEditTeams ? null : userId);
            TeamDashboardViewModel _dashboard = new();
            _dashboard.IsAdmin = _canEditTeams;
            _dashboard.IsManager = _canEditTeams || this.User.Claims.FirstOrDefault(m => m.Type == "MemberType").Value == "Manager";
            _dashboard.Teams = new List<Team>();
            foreach (Team team in _teams)
            {
                _dashboard.Teams.Add(team);
            }
            return View(_dashboard);

        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> RemoveTeam(long teamId)
        {
            Claim userName = this.HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.Name);
            await this.dataAccessService.RemoveTeamAsync(teamId, userName.Value);

            return Ok("Success");

        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> RemoveTeamMember(long teamId, long userId)
        {
            Claim userName = this.HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.Name);
            await this.dataAccessService.RemoveTeamMemberAsync(teamId, userId, userName.Value);
            List<User> _users = await this.dataAccessService.FindTeamMembersAsync("", new long[] { Convert.ToInt64(userId.ToString()) });
            Team _team = await this.dataAccessService.GetTeamAsync(teamId);
             this.SendEmailAsync(_users[0].Email, _team.Name, false);

            return Ok("Success");

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendTestEmail(long teamId, long userId)
        {
            List<User> _users = await this.dataAccessService.FindTeamMembersAsync("", new long[] { Convert.ToInt64(userId.ToString()) });
            Team _team = await this.dataAccessService.GetTeamAsync(teamId);
            this.SendTestEmailAsync(_users[0].Email, _team.Name);

            return Ok("Success");

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateTeam([FromBody] TeamDetailsViewModel model)
        {
            Claim userName = this.HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(model.TeamName))
            {
                return Json(new { TeamNameErrorMessage = "Please enter team name" });

            }

            await this.dataAccessService.UpdateTeamToDbAsync(model, userName.Value);
            if (!string.IsNullOrWhiteSpace(model.SelectedUserIds))
            {
                List<string> _selectedUsersIDs = model.SelectedUserIds.Contains(",") ? model.SelectedUserIds.Split(',').ToList() : new List<string> { model.SelectedUserIds };
                List<long> _userIds = new List<long>();
                foreach (string s in _selectedUsersIDs)
                {
                    _userIds.Add(Convert.ToInt64(s));
                }
                List<User> _users = await this.dataAccessService.FindTeamMembersAsync("", _userIds.ToArray());

                if (_users.Count != _userIds.ToHashSet().Count)
                {
                    return Json(new { TeamUsersErrorMessage = "Invalid users passed to add to team" });
                }

                foreach (User user in _users)
                {
                    await this.dataAccessService.SaveTeamMemberToDbAsync(new TeamMember { TeamMemberType = user.MemberType, TeamId = model.TeamId, UserId = user.UserId , CreatedBy = userName.Value});
                     this.SendEmailAsync(user.Email, model.TeamName, true);
                }
            }
           
            return Json(new { redirectToUrl = Url.Action("Index", "Home") });
        }

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="email">user email</param>

        private void SendEmailAsync(string toemail, string teamName, bool added)
        {
            try
            {
                //create message
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(MailboxAddress.Parse("rramurvg@gmail.com"));
                mailMessage.To.Add(MailboxAddress.Parse(toemail));
                mailMessage.Subject = added ? "You have been added to the team" : "You have been removed from the team";

                if (added)
                {
                    mailMessage.Body = new TextPart(TextFormat.Html) { Text = $"<h1>User Added</h1><br /><p>You have been added to the team {teamName}</p>" };

                }

                else
                {
                    mailMessage.Body = new TextPart(TextFormat.Html) { Text = $"<h1>User Removed</h1><br /><p>You have been removed from the team {teamName}</p>" };

                }

                //send email
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("rramurvg@gmail.com", "gvah ynse zcie fuua");
                smtp.Send(mailMessage);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Sends the test email.
        /// </summary>
        /// <param name="email">user email</param>

        private void SendTestEmailAsync(string toemail, string teamName)
        {
            try
            {
                //create message
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(MailboxAddress.Parse("rramurvg@gmail.com"));
                mailMessage.To.Add(MailboxAddress.Parse(toemail));
                mailMessage.Subject = "email from your team";

                mailMessage.Body = new TextPart(TextFormat.Html) { Text = $"<h1>Test Email</h1><br /><p>Email from your team {teamName}</p>" };

                //send email
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("rramurvg@gmail.com", "gvah ynse zcie fuua");
                smtp.Send(mailMessage);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
            
        


        
        
        
        
        
        
       

