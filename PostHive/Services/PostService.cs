using DbContext;
using DbContext.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.Data;

namespace PostHive.Services
{
    public class PostService(IDbContextFactory<DatabaseContext> contextFactory, IWebHostEnvironment webHotEnv, RelationshipService relationshipService)
    {
        public async Task<Post?> GetPostByIdAsync(int postId)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var post = await context.Posts
                .Include(x => x.User)
                .Include(p => p.Files)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User) 
                .FirstOrDefaultAsync(p => p.PostId == postId);
            return post;
        }
        public async Task<List<Post>> GetRandomPosts(int max = 25)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            var totalPosts = await context.Posts.CountAsync();

            var random = new Random();
            var randomIdx = Enumerable.Range(0, totalPosts)
                .OrderBy(x => random.Next())
                .Take(max)
                .ToList();

            var randomPosts = await context.Posts
                .Include(p => p.Files)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .Where(p => randomIdx.Contains(p.PostId))
                .ToListAsync();

            return randomPosts;
        }

        public async Task<List<Post>> GetPostsByUserId(int UserId)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.Posts
                .Where(p => p.UserId == UserId)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPostsAsync()
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.Posts.ToListAsync();
        }
        public async Task SetCommentToPost(Comments comments)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            await context.Comments.AddAsync(comments);
            await context.SaveChangesAsync();
        }

        public async Task DeleteComment(Comments comment)
        {
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync();
                var c = await context.Comments.AsNoTracking().FirstAsync(c => c.CommentId == comment.CommentId);
                context.Comments.Remove(c);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task<List<Post>> GetPostsAsync(User userRequest, User? user)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            if (user == null)
                user = new User()
                {
                    UserId = -1
                };

            bool isFriend = false;
            var relationship = await relationshipService.GetRelationship(user, userRequest);

            if (relationship != null && relationship.Status == RelationshipStatus.blocked)
                    return new List<Post>();
            
            isFriend = relationship?.Status == RelationshipStatus.accept ? true : false;

            var posts = await context.Posts
                    .Where(p => p.UserId == userRequest.UserId &&
                                (p.Privacy == PostPrivacy.p_public ||
                                 isFriend && p.Privacy == PostPrivacy.p_only_friends))
                    .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                    .Include(p => p.Files)
                    .Include(p => p.User)
                    .ToListAsync();

            return posts;
        }
        public async Task<Post?> GetPostByIdAsync(User userRequest, User? user, int postId, bool getUnlistedPost)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            if (user == null)
                user = new User()
                {
                    UserId = -1
                };

            bool isFriend = false;
            var relationship = await relationshipService.GetRelationship(user, userRequest);

            if (relationship is { Status: RelationshipStatus.blocked })
                return null;

            isFriend = relationship?.Status == RelationshipStatus.accept;

            var posts = await context.Posts
                .Where(p => p.UserId == userRequest.UserId &&
                            (p.Privacy == PostPrivacy.p_public ||
                             isFriend && p.Privacy == PostPrivacy.p_only_friends ||
                             getUnlistedPost && p.Privacy == PostPrivacy.p_unlisted))
                .Include(p => p.User)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .Include(p => p.Files)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            return posts;
        }
        public async Task DeletePostAsync(int postId)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var post = await context.Posts
                .Include(p => p.Files)
                .Include(p => p.Comments)
                .ThenInclude(c => c.Files)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post != null)
            {
                foreach (var file in post.Files)
                {
                    string filePath = Path.Combine(webHotEnv.WebRootPath, "Doctypes", file.Uri);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                foreach (var comment in post.Comments)
                {
                    foreach (var file in comment.Files)
                    {
                        string filePath = Path.Combine(webHotEnv.WebRootPath, "Doctypes", file.Uri);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                }

                context.Files.RemoveRange(post.Files);

                foreach (var comment in post.Comments)
                {
                    context.Files.RemoveRange(comment.Files);
                }
                context.Comments.RemoveRange(post.Comments);

                context.Posts.Remove(post);

                await context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("CurrentPost not found");
            }
        }

        public async Task DeletePostAsync(Post post)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            context.Posts.Remove(post);
            await context.SaveChangesAsync();
        }
        public async Task UpdatePostAsync(Post post)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var existingPost = await context.Posts
                                             .Include(p => p.Files)
                                             .FirstOrDefaultAsync(p => p.PostId == post.PostId);

            if (existingPost == null)
            {
                throw new Exception("CurrentPost not found");
            }

            foreach (var file in existingPost.Files)
            {
                var trackedFile = context.ChangeTracker.Entries<Files>()
                                  .FirstOrDefault(e => e.Entity.FilesId == file.FilesId);
                if (trackedFile != null)
                {
                    context.Entry(trackedFile.Entity).State = EntityState.Detached;
                }
            }

            var filesToRemove = existingPost.Files
                                            .Where(f => post.Files.All(pf => pf.FilesId != f.FilesId))
                                            .ToList();
            foreach (var file in filesToRemove)
            {
                context.Remove(file);
            }

            context.Entry(existingPost).CurrentValues.SetValues(post);

            foreach (var file in post.Files)
            {
                var trackedFile = context.ChangeTracker.Entries<Files>()
                                  .FirstOrDefault(e => e.Entity.FilesId == file.FilesId);
                if (trackedFile != null)
                {
                    context.Entry(trackedFile.Entity).State = EntityState.Detached;
                }
                existingPost.Files.Add(file);
            }

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<Post?> CreatePostAsync(Post post)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            DataTable filesTable = new DataTable();
            filesTable.Columns.Add("FileType", typeof(string));
            filesTable.Columns.Add("URI", typeof(string));

            foreach (var file in post.Files)
            {
                filesTable.Rows.Add(file.FileType, file.Uri);
            }
            var userIdParam = new SqlParameter("@UserId", post.UserId);
            var bodyParam = new SqlParameter("@Body", post.Body);
            var privacyParam = new SqlParameter("@Privacy", (int)post.Privacy);
            var createDateParam = new SqlParameter("@CreatedDate", post.CreatedDate);
            var filesParam = new SqlParameter("@Files", SqlDbType.Structured)
            {
                TypeName = "dbo.FilesColecction",
                Value = filesTable
            };
            var postIdOutput = new SqlParameter("@ReturnPostId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            try
            {
                await context.Database.ExecuteSqlRawAsync(
                    "EXEC INSERT_POST_FILES @UserId, @Body, @CreatedDate, @Privacy, @Files, @ReturnPostId OUTPUT",
                    userIdParam, bodyParam, createDateParam, privacyParam, filesParam, postIdOutput);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            post.PostId = (int)postIdOutput.Value;
            return post;
            //await using var context = await contextFactory.CreateDbContextAsync();
            //await context.Posts.AddAsync(post);
            //await context.SaveChangesAsync();
            //return post;
        }

    }
}