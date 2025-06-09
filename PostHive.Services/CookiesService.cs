using DbContext;
using DbContext.Models;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace PostHive.Services
{
    /// <summary>
    /// Service for handling user session cookies.
    /// </summary>
    public class CookiesService(
            IDbContextFactory<DatabaseContext> contextFactory,
            LocalStorageService localStorageService,
            UserService userService)
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CookiesService"/> class.
        /// </summary>
        /// <param name="contextFactory">Factory for creating database contexts.</param>
        /// <param name="localStorageService">Service for accessing local storage.</param>
       
        public async Task RemoveLocalCookies()
        {
            await localStorageService.RemoveItemAsync("CurrentSession");
        }

        /// <summary>
        /// Retrieves a user by the session cookie stored in local storage or returns the provided user if not null.
        /// </summary>
        /// <param name="user">The user to return if not null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user or null.</returns>
        public async Task<User?> RetrievedUser(User? user)
        {
            if (user != null)
                return user;

            string t = await localStorageService.GetItemAsync("CurrentSession");


            var tempUser = await GetUserByCookie(t);
            return tempUser;
        }

        /// <summary>
        /// Adds a new session cookie for the specified user.
        /// </summary>
        /// <param name="user">The user to associate with the new session cookie.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the session cookie GUID as a string.</returns>
        public async Task<string> AddCookieCurrentSession(User user)
        {
            // Generate a new GUID for the cookie.
            Guid guid = Guid.NewGuid();

            // Create a new database context for saving the cookie.
            await using var context = await contextFactory.CreateDbContextAsync();

            // Create a new CookiesResearch object with the GUID and user ID.
            var cookie = new CookiesResearch()
            {
                CookieCurrentSession = guid.ToString(),
                UserId = user.UserId
            };

            // Add the new cookie to the database.
            context.Cookies.Add(cookie);
            await context.SaveChangesAsync();

            // Return the GUID as a string, which will be used as the session cookie.
            return guid.ToString();
        }

        /// <summary>
        /// Removes all session cookies associated with the provided cookie value.
        /// </summary>
        /// <param name="cookie">The session cookie to remove.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RemoveCookieCurrentSessions(string cookie)
        {
            // Create a database context for removing the cookies.
            await using var context = await contextFactory.CreateDbContextAsync();

            // Remove all cookies that match the provided session cookie value.
            context.Cookies.RemoveRange(context.Cookies.Where(p => p.CookieCurrentSession == cookie));
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a user associated with the specified session cookie.
        /// </summary>
        /// <param name="cookie">The session cookie to search for.</param>
        /// <param name="context">The database context to use for querying.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user or null if no user is found.</returns>
        private async Task<User?> GetUserByCookie(string cookie)
        {
            // Find the cookie in the database.
            await using var context = await contextFactory.CreateDbContextAsync();
            var user = await context.Cookies
                .Where(p => p.CookieCurrentSession == cookie)
                .FromCacheAsync($"{CacheTags.Cookie}{cookie}");
            if (user != null)
            {
                //TODO: Friends instance
                var transforCookie = user.FirstOrDefault();
                
                // If the cookie is found, retrieve the associated user, including related posts, files, comments, and friends.

                var getUser = await context.Users.AsNoTracking()
                    .Where(u => u.UserId == transforCookie.UserId)
                    .Include(u => u.Posts)
                    .ThenInclude(p => p.Files)
                    .Include(u => u.Posts)
                    .ThenInclude(p => p.Comments)
                    .ThenInclude(c => c.User)
                    .FromCacheAsync($"{CacheTags.CurrentUser}{transforCookie.UserId}");
                if (getUser != null)
                {
                    var currentUser = getUser.FirstOrDefault();
                    currentUser.Friends = await userService.GetFriendsAsync(currentUser.UserId);
                    return currentUser;
                }
            }
            return null;
        }
    }
}
