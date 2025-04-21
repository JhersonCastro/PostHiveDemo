using DbContext;
using DbContext.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace PostHive.Services
{
    public class UserService(IDbContextFactory<DatabaseContext> contextFactory)
    {
        public async Task<List<User>> GetUsersAsync()
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.Users
                .Where(u => u.Posts.Count > 0)
                .Include(u => u.Posts)
                .ThenInclude(p => p.Comments)
                .ThenInclude(c => c.User)
                .Include(u => u.Posts)
                .ThenInclude(p => p.Files)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.Users
                .Include(u => u.Posts)
                .ThenInclude(p => p.Files)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

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


        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public async Task<User?> GetUserById(int id)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        }
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

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
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
        public async Task UpdateUserAsync(User userChange)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userChange.UserId);
            if (user != null)
            {
                user.Avatar = userChange.Avatar ?? user.Avatar;
                user.Name = userChange.Name ?? user.Name;
                user.NickName = userChange.NickName ?? user.NickName;

                context.Users.Update(user);
                await context.SaveChangesAsync();
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
