using DbContext;
using DbContext.Models;
using Microsoft.EntityFrameworkCore;

namespace PostHive.Services
{
    /// <summary>
    /// Service for handling user session cookies.
    /// </summary>
    public class CookiesService(
            IDbContextFactory<DatabaseContext> contextFactory,
            LocalStorageService localStorageService)
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
            var user = await context.Cookies.FirstOrDefaultAsync(p => p.CookieCurrentSession == cookie);
            if (user != null)
            {
                //TODO: Friends instance

                // If the cookie is found, retrieve the associated user, including related posts, files, comments, and friends.

                var getUser = await context.Users.AsNoTracking()
                    .Include(u => u.Posts)
                    .ThenInclude(p => p.Files)
                    .Include(u => u.Posts)
                    .ThenInclude(p => p.Comments)
                    .ThenInclude(c => c.User)
                    //.Include(f => f)
                    .FirstOrDefaultAsync(u => u.UserId == user.UserId);
                if (getUser != null)
                {
                    getUser.Friends = await GetFriendsAsync(getUser.UserId);
                    return getUser;
                }
            }
            return null;
        }
        /// <summary>
        /// Retrieves a list of friends for a given user based on their relationships.
        /// </summary>
        /// <param name="userId">The ID of the user whose friends are to be retrieved.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of users who are friends with the specified user.
        /// </returns>
        /// <remarks>
        /// This method queries the database to find all relationships where the user is either the initiator or the receiver of the relationship,
        /// and the relationship status is "accept". It then combines these results to produce a list of friends.
        /// </remarks>
        private async Task<List<User>> GetFriendsAsync(int userId)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var user = await context.Users
                .Include(u => u.RelationshipsInitiated)
                .ThenInclude(r => r.RelatedUser)
                .Include(u => u.RelationshipsReceived)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user != null)
            {
                var friends = user.RelationshipsInitiated
                    .Where(r => r.Status == RelationshipStatus.accept)
                    .Select(r => r.RelatedUser)
                    .Union(user.RelationshipsReceived
                        .Where(r => r.Status == RelationshipStatus.accept)
                        .Select(r => r.User))
                    .ToList();

                return friends;
            }

            return new List<User>();
        }
    }
}
