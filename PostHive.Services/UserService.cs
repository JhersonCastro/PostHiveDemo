using DbContext;
using DbContext.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace PostHive.Services
{
    public class UserService(IDbContextFactory<DatabaseContext> contextFactory)
    {
        /// <summary>
        /// Creates a new user in the database using a stored procedure and returns the created user with their posts and files.
        /// </summary>
        /// <param name="user">The user entity to create.</param>
        /// <param name="credential">The credential entity containing email and password.</param>
        /// <returns>The newly created user if successful; otherwise, null.</returns>
        public async Task<User?> CreateUserAsync(User user, Credential credential)
        {
            await using var context = await contextFactory.CreateDbContextAsync() ;
            
            var nameParam = new SqlParameter("@Name", user.Name);
            var nickNameParam = new SqlParameter("@NickName", user.NickName);
            var emailParam = new SqlParameter("@Email", credential.Email);
            var passwordParam = new SqlParameter("@Password", HashPassword(credential.Password));
            var userIdOutput = new SqlParameter("@UserId", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            // Execute the stored procedure
            await context.Database.ExecuteSqlRawAsync(
                    "EXEC spUserRegister @Name, @NickName, @Email, @Password, @UserId OUTPUT",
                    nameParam, nickNameParam, emailParam, passwordParam, userIdOutput);

            // Retrieve the output parameter value
            var userId = (int)userIdOutput.Value;
            // Fetch the newly created user
            User? newUser = await context.Users
                .Include(u => u.Posts)
                .ThenInclude(p => p.Files)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (newUser != null)
            {
                Console.WriteLine($"Nuevo usuario registrado: {newUser.Name}, ID: {newUser.UserId}");
            }
            return newUser;
        }
        public async Task UpdateUserAsync(User userChange)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            if(context.Users.Any(u => u.NickName == userChange.NickName && u.UserId != userChange.UserId))
                throw new Exception($"Nickname {userChange.NickName} already use for other user!");
            
            var user = new User { UserId = userChange.UserId };

            context.Users.Attach(user);

            user.Name = userChange.Name;
            user.NickName = userChange.NickName;
            user.Bio = userChange.Bio;

            context.Entry(user).Property(x => x.Name).IsModified = true;
            context.Entry(user).Property(x => x.NickName).IsModified = true;
            context.Entry(user).Property(x => x.Bio).IsModified = true;

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Hashes a plain text password using BCrypt.
        /// </summary>
        /// <param name="password">The plain text password.</param>
        /// <returns>The hashed password.</returns>
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>The user if found; otherwise, null.</returns>
        public async Task<User?> GetUserById(int id)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        }

        /// <summary>
        /// Authenticates a user by verifying their email and password.
        /// </summary>
        /// <param name="credential">The credential containing email and password.</param>
        /// <returns>The authenticated user with posts, files, and friends if successful; otherwise, throws an exception.</returns>
        /// <exception cref="Exception">Thrown if the user is not found or the password does not match.</exception>
        public async Task<User?> AuthenticateLoginAsync(Credential credential)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            var userCredential = await context.Credentials
                .FirstOrDefaultAsync(c => c.Email == credential.Email);

            if (userCredential == null)
                throw new Exception("No user found with the provided email.");

            if (VerifyPassword(credential.Password, userCredential.Password))
            {
                var user = await context.Users
                    .Include(u => u.Posts)
                    .ThenInclude(p => p.Files)
                    .FirstOrDefaultAsync(u => u.UserId == userCredential.UserId);
                user.Friends = await GetFriendsAsync(user.UserId);
                return user;
            }

            throw new Exception("The password does not match the email.");
        }

        /// <summary>
        /// Retrieves a list of friends for a given user by user ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A list of users who are friends with the specified user.</returns>
        public async Task<List<User>> GetFriendsAsync(int userId)
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
                // Filtrar relaciones con estado 'accept' y combinar iniciadas y recibidas
                var friends = user.RelationshipsInitiated
                    .Where(r => r.Status == RelationshipStatus.accept)
                    .Select(r => r.RelatedUser) // Amigos desde las relaciones iniciadas
                    .Union(user.RelationshipsReceived
                        .Where(r => r.Status == RelationshipStatus.accept)
                        .Select(r => r.User)) // Amigos desde las relaciones recibidas
                    .ToList();

                return friends; // Lista final de amigos
            }

            return new List<User>(); // Devuelve una lista vacía si el usuario no existe
        }

        /// <summary>
        /// Verifies a plain text password against a hashed password using BCrypt.
        /// </summary>
        /// <param name="password">The plain text password.</param>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <returns>True if the password matches; otherwise, false.</returns>
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        /// <summary>
        /// Updates the avatar of a user by executing a stored procedure.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="newAvatar">The new avatar file name or URI.</param>
        /// <exception cref="Exception">Thrown if there is an error updating the avatar.</exception>
        public async Task UpdateAvatarAsync(int userId, string newAvatar)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var useridParam = new SqlParameter("@UserId", System.Data.SqlDbType.Int)
            {
                Value = userId
            };

            var avatarParam = new SqlParameter("@Avatar", System.Data.SqlDbType.NVarChar, 50)
            {
                Value = newAvatar
            };
            try
            {
                await context.Database.ExecuteSqlRawAsync(
                    "EXEC spUpdateAvatar @Avatar, @UserId", avatarParam, useridParam);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("Error updating avatar", e);
            }
        }

        /// <summary>
        /// Predicts and retrieves a list of users whose names or nicknames match the given prompt.
        /// </summary>
        /// <param name="nameOrNickname">
        /// A string representing the name or nickname to search for. 
        /// If null, an empty list will be returned.
        /// </param>
        /// <returns>
        /// An asynchronous task that resolves to a list of <see cref="User"/> objects 
        /// whose names or nicknames start with the specified prompt.
        /// </returns>
        /// <remarks>
        /// This method normalizes the input prompt by trimming whitespace and converting it to lowercase. 
        /// It performs a case-insensitive search on both the Name and NickName fields.
        /// </remarks>
        public async Task<List<User>> PredictUserAsync(string? nameOrNickname)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            if (nameOrNickname == null)
                return new List<User>();
            string normalizedPrompt = nameOrNickname.Trim().ToLower();
            return await context.Users
                .Where(u => u.NickName.Trim().ToLower().StartsWith(normalizedPrompt) ||
                            u.Name.Trim().ToLower().StartsWith(normalizedPrompt))
                .ToListAsync();
        }
    }
}
