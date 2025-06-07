using System.Text.Json;
using System.Text.Json.Serialization;
using DbContext.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using PostHive.Components.Pages.Tags.Dialogs;

namespace PostHive.Components.Pages.Tags.Posts;

public partial class Comment : IAsyncDisposable
{
    [Parameter] public required DbContext.Models.Post post { get; set; }

    [Parameter] public required User? CurrentUser { get; set; }

    private HubConnection? _hubConnection;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/CommentHub"))
                .Build();
            _hubConnection.On<string>("ReceiveComment", async (comment) =>
            {
                var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve };
                var commentObj = JsonSerializer.Deserialize<Comments>(comment, options);
                await AddComment(commentObj);
            });
            _hubConnection.On<string>("RecDelComment", async (commentId) =>
            {
                var comment = post.Comments.FirstOrDefault(c => c.CommentId.ToString() == commentId);
                await RemoveComment(comment);
            });
            await _hubConnection.StartAsync();
            _hubConnection.SendAsync("JoinGroup", post.PostId.ToString());
            _hubConnection.Closed += async (error) =>
            {
                Console.WriteLine("Connection closed. Attempting to reconnect...");
                await Task.Delay(5000); // Wait before reconnecting
                try
                {
                    await _hubConnection.StartAsync();
                    await _hubConnection.SendAsync("JoinGroup", post.PostId.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Reconnection failed: {ex.Message}");
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing SignalR: {ex.Message}");
        }
    }

    public async Task DeleteId(int commentId)
    {
        if (_hubConnection != null)
        {
            var response = await DialogService.ShowAsync<ConfirmDialog>("Delete post", new DialogParameters
            {
                { "Title", "Delete comment" },
                { "Content", "Are you sure you want to delete this comment?" }
            });
            var result = await response.Result;
            if (result is { Canceled: false })
                await _hubConnection.SendAsync("DeleteComment", post.PostId.ToString(), commentId.ToString());
        }
    }

    private async Task AddComment(Comments? comment)
    {
        if (comment != null)
        {
            post.Comments.Add(comment);
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task RemoveComment(Comments? comment)
    {
        if (comment != null)
        {
            post.Comments.Remove(comment);
            await PostService.DeleteComment(comment);
            await InvokeAsync(StateHasChanged);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}