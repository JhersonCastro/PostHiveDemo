using DbContext;
using DbContext.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace PostHive.Services
{
    public class PostService(IDbContextFactory<DatabaseContext> contextFactory, IWebHostEnvironment webHotEnv, RelationshipService relationshipService)
    {
        /// <summary>
        /// Retrieves a post by its ID, including related user, files, and comments.
        /// </summary>
        /// <param name="postId">The ID of the post to retrieve.</param>
        /// <returns>The post if found, otherwise null.</returns>
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

        /// <summary>
        /// Adds a comment to a specific post.
        /// </summary>
        /// <param name="comments">The comment to add.</param>
        public async Task SetCommentToPost(Comments comments)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            await context.Comments.AddAsync(comments);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a specific comment from a post.
        /// </summary>
        /// <param name="comment">The comment to delete.</param>
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

        /// <summary>
        /// Retrieves posts created by a specific user, filtered by privacy settings and relationship status.
        /// </summary>
        /// <param name="userRequest">The user whose posts to retrieve.</param>
        /// <param name="user">The user making the request.</param>
        /// <returns>A list of posts visible to the requesting user.</returns>
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

        /// <summary>
        /// Retrieves a specific post by ID, filtered by privacy settings and relationship status.
        /// </summary>
        /// <param name="userRequest">The user whose post to retrieve.</param>
        /// <param name="user">The user making the request.</param>
        /// <param name="postId">The ID of the post to retrieve.</param>
        /// <param name="getUnlistedPost">Whether to include unlisted posts in the search.</param>
        /// <returns>The post if found and visible to the requesting user, otherwise null.</returns>
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

        /// <summary>
        /// Deletes a specific post by its ID, including related files and comments.
        /// </summary>
        /// <param name="postId">The ID of the post to delete.</param>
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
        /// <summary>
        /// Creates a new post, including its related files.
        /// </summary>
        /// <param name="post">The post to create.</param>
        /// <returns>The created post with its generated ID.</returns>
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
        }
    }
}