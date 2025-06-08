using DbContext;
using DbContext.Models;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace PostHive.Services
{
    public enum ActionType
    {
        Remove,
        Request,
        Add,
        Block,
        Error
    }
    public class RelationshipService(IDbContextFactory<DatabaseContext> contextFactory)
    {
        /// <summary>
        /// Retrieves a list of users that the specified user has blocked.
        /// </summary>
        /// <param name="user">The user for whom to retrieve blocked users.</param>
        /// <returns>A list of blocked users.</returns>
        public async Task<List<User>> GetBlockedUsersAsync(User user)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var blockedUsers = await context.Users
                .Where(u => context.Relationship.Any(r =>
                    (r.UserId == user.UserId && r.RelationshipUserIdA == u.UserId) ||
                    (r.UserId == u.UserId && r.RelationshipUserIdA == user.UserId) &&
                    r.Status == RelationshipStatus.blocked))
                .ToListAsync();
            return blockedUsers;
        }

        /// <summary>
        /// Blocks a user by creating or updating a relationship with the "blocked" status.
        /// </summary>
        /// <param name="userA">The user initiating the block.</param>
        /// <param name="userB">The user to be blocked.</param>
        /// <returns>An ActionType indicating the result of the operation.</returns>
        public async Task<ActionType> BlockUserAsync(User userA, User userB)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var relationship = await context.Relationship.FirstOrDefaultAsync(p =>
                (p.UserId == userA.UserId && p.RelationshipUserIdA == userB.UserId) ||
                (p.UserId == userB.UserId && p.RelationshipUserIdA == userA.UserId));
            try
            {
                if (relationship == null)
                {
                    Relationship newRelationship = new Relationship
                    {
                        UserId = userA.UserId,
                        RelationshipUserIdA = userB.UserId,
                        Status = RelationshipStatus.blocked
                    };
                    await context.Relationship.AddAsync(newRelationship);
                    await context.SaveChangesAsync();
                    return ActionType.Block;
                }
                relationship.Status = RelationshipStatus.blocked;
                context.Relationship.Update(relationship);
                await context.SaveChangesAsync();
                Console.WriteLine("Se guardo correctamente" + relationship.ToString());
                return ActionType.Block;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return ActionType.Error;
            }
        }

        /// <summary>
        /// Sets or updates the relationship between two users based on the current status.
        /// </summary>
        /// <param name="userA">The user initiating the action.</param>
        /// <param name="userB">The other user involved in the relationship.</param>
        /// <returns>An ActionType indicating the result of the operation.</returns>
        public async Task<ActionType> SetRelationshipAsync(User userA, User userB)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var relationship = await context.Relationship.FirstOrDefaultAsync(p =>
            (p.UserId == userA.UserId && p.RelationshipUserIdA == userB.UserId) ||
            (p.UserId == userB.UserId && p.RelationshipUserIdA == userA.UserId));

            if (relationship == null)
            {
                Relationship newRelationship = new Relationship
                {
                    UserId = userA.UserId,
                    RelationshipUserIdA = userB.UserId
                };
                await context.Relationship.AddAsync(newRelationship);
                await context.SaveChangesAsync();
                return ActionType.Request;
            }

            if (IsGenerateByUserA(relationship, userA))
            {
                context.Relationship.Remove(relationship);
                await context.SaveChangesAsync();
                return ActionType.Remove;
            }
            //Okay, we know that the UserA clicked the button, but he doesn't create the relationship first
            //Rules: 
            //1. If the relationship is in request status, we can change it to accept
            //2. If the relationship is in accept status, we remove the relationship
            ActionType s = ActionType.Remove;
            switch (relationship.Status)
            {
                case RelationshipStatus.request:
                    relationship.Status = RelationshipStatus.accept;
                    context.Relationship.Update(relationship);
                    s = ActionType.Add;
                    break;
                case RelationshipStatus.accept:
                    context.Relationship.Remove(relationship);
                    s = ActionType.Remove;
                    break;
            }
            await context.SaveChangesAsync();
            Console.WriteLine("Se guardo correctamente" + relationship.ToString());
            return s;
        }

        /// <summary>
        /// Retrieves the relationship between two users, if it exists.
        /// </summary>
        /// <param name="userA">The first user.</param>
        /// <param name="userB">The second user.</param>
        /// <returns>The relationship between the two users, or null if none exists.</returns>
        public async Task<Relationship?> GetRelationship(User userA, User userB)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            return await context.Relationship
                .Where(r =>
                    (r.UserId == userA.UserId && r.RelationshipUserIdA == userB.UserId) ||
                    (r.UserId == userB.UserId && r.RelationshipUserIdA == userA.UserId))
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves the appropriate icon for the relationship between two users.
        /// </summary>
        /// <param name="userA">The first user.</param>
        /// <param name="userB">The second user.</param>
        /// <returns>A string representing the icon for the relationship.</returns>
        public async Task<string> GetRelationIcon(User userA, User userB)
        {
            Relationship? relationship = await GetRelationship(userA, userB);
            if (relationship == null)
            {
                return Icons.Material.Filled.PersonAdd;
            }
            if (IsGenerateByUserA(relationship, userA))
            {
                return relationship.Status switch
                {
                    RelationshipStatus.request => Icons.Material.Filled.Cancel,
                    RelationshipStatus.accept => Icons.Material.Filled.PersonRemove,
                    _ => Icons.Material.Filled.PersonAdd,
                };
            }
            return relationship.Status switch
            {
                RelationshipStatus.request => Icons.Material.Filled.PersonAdd,
                RelationshipStatus.accept => Icons.Material.Filled.PersonRemove,
                _ => Icons.Material.Filled.Cancel,
            };
        }

        /// <summary>
        /// Determines if the relationship was initiated by the specified user.
        /// </summary>
        /// <param name="relationship">The relationship to check.</param>
        /// <param name="userA">The user to verify.</param>
        /// <returns>True if the relationship was initiated by the user, otherwise false.</returns>
        private bool IsGenerateByUserA(Relationship? relationship, User userA)
        {
            return relationship != null && relationship.UserId == userA.UserId;
        }
    }
}
