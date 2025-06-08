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
        public async Task<List<User>> GetBlockedUsersAsync(User user)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            // Get all relationships where the user is either UserId or RelationshipUserIdA and the status is blocked
            
            var blockedUsers = await context.Users
                .Where(u => context.Relationship.Any(r =>
                    (r.UserId == user.UserId && r.RelationshipUserIdA == u.UserId) ||
                    (r.UserId == u.UserId && r.RelationshipUserIdA == user.UserId) &&
                    r.Status == RelationshipStatus.blocked))
                .ToListAsync();
            return blockedUsers;
        }
        public async Task<ActionType> BlockUserAsync(User userA, User userB)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            // Check if the relationship exists
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
        public async Task<ActionType> SetRelationshipAsync(User userA, User userB)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            //Check if the Relationship exists
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
            //Now, we know that the relationship exists, but how is its status?            
            //1st Check if the UserA (User that clicked the button)
            if (IsGenerateByUserA(relationship, userA))
            {
                //If the relationship is in request status, we remove the relationship
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
        private bool IsGenerateByUserA(Relationship? relationship, User userA)
        {
            return relationship != null && relationship.UserId == userA.UserId;
        }
    }
}
