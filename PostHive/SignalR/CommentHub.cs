using Microsoft.AspNetCore.SignalR;

namespace PostHive.SignalR
{
    public class CommentHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"User connected: {Context.ConnectionId}");
            return Task.CompletedTask;
        }
        [HubMethodName("JoinGroup")]
        public async Task JoinGroup(string postId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, postId);
            Console.WriteLine($"Client {Context.ConnectionId} joined group {postId}");

        }
        public async Task LeaveGroup(string postId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, postId);
        }
        [HubMethodName("SendComment")]
        public async Task SendComment(string postId, string comment)
        {
            await Clients.OthersInGroup(postId).SendAsync("ReceiveComment", comment);
        }
        [HubMethodName("DeleteComment")]
        public async Task DeleteComment(string postId, string comment)
        {
            await Clients.OthersInGroup(postId).SendAsync("ReceiveDeleteComment", comment);
        }
    }
}
