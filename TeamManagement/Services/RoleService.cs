

using System.Security.Claims;

namespace TeamManagement.Services
{
    public class RoleService
    {
/// <summary>
/// Checks whether the user has role or not
/// </summary>
/// <param name="claims"></param>
/// <param name="rolename"></param>
/// <returns></returns>

        public static bool HasRole(IEnumerable<Claim>claims, string roleName)
        {
            foreach (Claim claim in claims)
            {
                if (claim.Type == ClaimTypes.Role && claim.Value == roleName)
                    return true;
            }
            return false;
        }


    }
}
