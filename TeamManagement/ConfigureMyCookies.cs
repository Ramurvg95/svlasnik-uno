namespace TeamManagement
{
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Class to configure the cookies.
    /// </summary>
    public class ConfigureMyCookie : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        /// <summary>
        /// Configures the cookie method.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="options"></param>
        public void Configure(string name, CookieAuthenticationOptions options)
        {
            // Only configure the schemes you want
            if (name == Constants.BearerScheme)
            {
                options.LoginPath = "Account/Login";
            }
        }

        /// <summary>
        /// Configures the cookie method.
        /// </summary>
        /// <param name="options"></param>
        public void Configure(CookieAuthenticationOptions options)
            => Configure(Options.DefaultName, options);
    }
}