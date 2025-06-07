using DbContext.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    private MudTextField<string>? _textFieldRef;
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
                    comment.User = new User
                    {
                        UserId = CurrentUser.UserId,
                        Name = CurrentUser.Name,
                        NickName = CurrentUser.NickName,
                        Avatar = CurrentUser.Avatar
                    };
                    //TODO: Add files to comment if needed
                    comment.Files = null;

                    var options = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.Preserve
                    };
                    Console.WriteLine($"Enviado a {Post.PostId} grupo");
                    var commentJson = JsonSerializer.Serialize(comment, options);
                    await _hubConnection.SendAsync("SendComment", Post.PostId.ToString(), commentJson);
                }
            }

            if (_commentModel != null)
            {
                _commentModel.CommentText = string.Empty;
                if (_textFieldRef != null)
                    await _textFieldRef.Clear();
                Snackbar.Add("Comment send successfully!", Severity.Success);
            }
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