using DbContext.Models;

namespace PostHive.Services
{
    /// <summary>
    /// Represents the state of the currently logged-in user.
    /// </summary>
    public class UserState
    {
        /// <summary>
        /// Gets or sets the current user.
        /// This property holds the information of the user currently logged into the application.
        /// </summary>
        public User? CurrentUser { get; set; }
    }
}
