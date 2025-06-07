using DbContext;
using DbContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using MudBlazor;

namespace PostHive.Services
{
    public enum ActionType
    {
        Remove,
        Request,
        Add,
        Block,
    }
    public class RelationshipService(IDbContextFactory<DatabaseContext> contextFactory)
    {
        
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
            if(IsGenerateByUserA(relationship, userA))
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
            if(relationship == null)
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
