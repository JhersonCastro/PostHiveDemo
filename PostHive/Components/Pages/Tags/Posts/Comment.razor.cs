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
            GC.Collect();
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/CommentHub"))
                .Build();
            
            _hubConnection.On<string>("ReceiveDeleteComment", async (comment) =>
            {
                var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve };
                var commentObj = JsonSerializer.Deserialize<Comments>(comment, options);
                await RemoveComment(commentObj);
            });
            _hubConnection.On<string>("ReceiveComment", async (comment) =>
            {
                var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve };
                var commentObj = JsonSerializer.Deserialize<Comments>(comment, options);
                await AddComment(commentObj);
            });
            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync("JoinGroup", post.PostId.ToString());
           
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing SignalR: {ex.Message}");
        }
    }

    public async Task DeleteId(Comments comment)
    {
        if (_hubConnection != null)
        {
            var response = await DialogService.ShowAsync<ConfirmDialog>("Delete post", new DialogParameters
            {
                { "Title", "Delete comment" },
                { "Content", "Are you sure you want to delete this comment?" }
            });
            var result = await response.Result;
            //TODO: Add signalR to delete comment
            if (result is { Canceled: false })
                await RemoveComment(comment);
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
            _hubConnection.SendAsync("DeleteComment", comment);
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