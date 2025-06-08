using Microsoft.AspNetCore.SignalR;

namespace PostHive.SignalR
{
    /// <summary>
    /// SignalR Hub for managing comments in real-time.
    /// </summary>
    public class CommentHub : Hub
    {
        /// <summary>
        /// Called when a client connects to the hub.
        /// </summary>
        /// <returns>A completed task.</returns>
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"User connected: {Context.ConnectionId}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds the client to a specific group based on the post ID.
        /// </summary>
        /// <param name="postId">The ID of the post to join the group for.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [HubMethodName("JoinGroup")]
        public async Task JoinGroup(string postId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, postId);
            Console.WriteLine($"Client {Context.ConnectionId} joined group {postId}");
        }

        /// <summary>
        /// Removes the client from a specific group based on the post ID.
        /// </summary>
        /// <param name="postId">The ID of the post to leave the group for.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LeaveGroup(string postId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, postId);
        }

        /// <summary>
        /// Sends a comment to all other clients in the group associated with the post ID.
        /// </summary>
        /// <param name="postId">The ID of the post to send the comment to.</param>
        /// <param name="comment">The content of the comment.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [HubMethodName("SendComment")]
        public async Task SendComment(string postId, string comment)
        {
            await Clients.OthersInGroup(postId).SendAsync("ReceiveComment", comment);
        }

        /// <summary>
        /// Sends a delete comment notification to all other clients in the group associated with the post ID.
        /// </summary>
        /// <param name="postId">The ID of the post to delete the comment from.</param>
        /// <param name="comment">The content of the comment to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [HubMethodName("DeleteComment")]
        public async Task DeleteComment(string postId, string comment)
        {
            await Clients.OthersInGroup(postId).SendAsync("ReceiveDeleteComment", comment);
        }
    }
}
