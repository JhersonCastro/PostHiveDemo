using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using DbContext.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;

namespace PostHive.Components.Pages.Tags.Posts;

public partial class UserComment : IAsyncDisposable
{
    [Parameter] public DbContext.Models.Post Post { get; set; } = new();

    [Parameter] public User? CurrentUser { get; set; }

    private EditContext? EditContext { get; set; }
    private CommentModel? _commentModel;
    private HubConnection? _hubConnection;

    protected override async Task OnInitializedAsync()
    {
        if (CurrentUser == null)
            return;
        _commentModel = new CommentModel();
        EditContext = new EditContext(_commentModel);
        try
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/CommentHub"))
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing SignalR: {ex.Message}");
        }
    }

    /// <summary>
    /// Send a comment to the post
    /// </summary>
    /// <param name="editContext">The context of the form</param>
    private async Task HandlerSend(EditContext editContext)
    {
        var model = (CommentModel)editContext.Model;
        try
        {
            if (CurrentUser != null)
            {
                Comments comment = new Comments()
                {
                    CommentText = model.CommentText,
                    PostId = Post.PostId,
                    UserId = CurrentUser.UserId
                };
                await PostService.SetCommentToPost(comment);

                if (_hubConnection is { State: HubConnectionState.Connected })
                {
                    comment.User = CurrentUser;
                    comment.Files = new List<Files>();

                    var options = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.Preserve
                    };
                    var commentJson = JsonSerializer.Serialize(comment, options);
                    await _hubConnection.SendAsync("SendComment", Post.PostId.ToString(), commentJson);
                }
            }

            if (_commentModel != null) _commentModel.CommentText = string.Empty;
            StateHasChanged();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
            await _hubConnection.DisposeAsync();
    }

    /// <summary>
    /// Class <b>CommentModel</b> for the comment form
    /// </summary>
    public class CommentModel
    {
        [Required]
        [MinLength(1)]
        [MaxLength(256)]
        public string CommentText { get; set; } = string.Empty;
    }
}