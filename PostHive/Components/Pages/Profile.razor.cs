using DbContext.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace PostHive.Components.Pages;

public partial class Profile
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;
    private ElementReference _mudPaperRef;

    private EditContext? _editPostContext;
    private PostModel _postModel = new();
    private UserUpdate _userModelUpdate = new();

    private int _currentUploadFiles;

    public class UserUpdate
    {
        [Required][MinLength(5)] public string? Name { get; set; }

        [Required][MinLength(5)] public string? NickName { get; set; }
        [MinLength(0)] public string Bio { get; set; } = "";
    }

    public class PostModel
    {
        [Required]
        [MinLength(1, ErrorMessage = "The body must have at least 1 character")]
        public string? Body { get; set; }

        [Required] public PostPrivacy Privacy { get; set; }
    }

    protected override Task OnInitializedAsync()
    {
        _editPostContext = new EditContext(_postModel);
        return base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {

                UserState.CurrentUser = await CookiesService.RetrievedUser(UserState.CurrentUser);
                if (UserState.CurrentUser == null)
                    throw new Exception("User not found");
                await Task.Delay(100);

                mediaSectionWidth = 280;
                if (JSRuntime is not null)
                {
                    await JSRuntime.InvokeVoidAsync("startResizeObserver", _mudPaperRef, DotNetObjectReference.Create(this));
                }
                else
                {
                    Console.WriteLine("JSRuntime no está disponible en este contexto.");
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving session: {ex.Message}");
                NavigationManager.NavigateTo("/");
            }
        }
    }



    private async Task BtnAvatarHandlerAsync(IBrowserFile e)
    {
        var browserFile = e;
        try
        {
            var fileStream = browserFile.OpenReadStream(Const.MaxFileSize);

            var uniqueFileName = Guid.NewGuid().ToString() + "." + browserFile.Name.Split('.').Last();
            var path = Path.Combine(WebHotEnv.WebRootPath, "Doctypes/Avatars", uniqueFileName);
            var file = new FileStream(path, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(file);
            file.Close();
            fileStream.Close();
            if (UserState.CurrentUser != null)
            {
                await UserService.UpdateAvatarAsync(UserState.CurrentUser.UserId, uniqueFileName);
                UserState.CurrentUser.Avatar = uniqueFileName;
                await InvokeAsync(StateHasChanged);
                Snackbar.Add("Avatar updated", Severity.Success);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Snackbar.Add("Error updating avatar, try later", Severity.Error);
        }
    }

    private readonly string[] _allowedExtensions = ["jpg", "mp4", "png", "jpeg"];
    private readonly Dictionary<Files, double> _fileProgresses = new Dictionary<Files, double>();

    private async Task BtnUploadPostFiles(IReadOnlyList<IBrowserFile> files)
    {
        _currentUploadFiles += files.Count();
        foreach (var file in files)
        {
            try
            {
                if (!_allowedExtensions.Contains(file.Name.Split('.').Last()))
                    throw new Exception("Invalid file type");

                var fileName = Path.GetFileName(file.Name);
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
                var path = Path.Combine(WebHotEnv.WebRootPath, "Doctypes", uniqueFileName);


                await using var fileStream = new FileStream(path, FileMode.Create);
                var bufferSize = file.Size > 50 * Const.Mb ? 2 * Const.Mb :
                 file.Size > 10 * Const.Mb ? 1 * Const.Mb :
                 524288; // Default 512 KB

                Memory<byte> buffer = new byte[bufferSize];
                var readBytes = 0;
                var readStream = file.OpenReadStream(Const.MaxFileSizePost);

                long totalBytes = file.Size;
                long uploadedBytes = 0;
                var tempFile = new Files { Uri = uniqueFileName, FileType = file.ContentType };

                while ((readBytes = await readStream.ReadAsync(buffer)) > 0)
                {
                    await fileStream.WriteAsync(buffer[..readBytes]);
                    uploadedBytes += readBytes;
                    var progress = (uploadedBytes / (double)totalBytes) * 100;
                    _fileProgresses[tempFile] = progress;
                    StateHasChanged();
                }
            }
            catch (InvalidOperationException ex)
            {
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
            catch (IOException ex)
            {
                Snackbar.Add($"File upload failed: {file.Name}. Error: {ex.Message}", Severity.Error);
            }
            finally
            {
                _currentUploadFiles--;
            }
        }
    }

    private async Task HandlerPost(EditContext editContext)
    {
        if (UserState.CurrentUser == null || _postModel?.Body == null)
            return;

        if (_currentUploadFiles > 0)
        {
            Snackbar.Add("Still uploading files, please wait", Severity.Error);
            return;
        }
        var files = _fileProgresses.Keys.Select(file => new Files() { Uri = file.Uri, FileType = file.FileType })
            .ToList();

        var post = new Post
        {
            UserId = UserState.CurrentUser.UserId,
            Body = _postModel.Body,
            Privacy = _postModel.Privacy,
            CreatedDate = DateTime.UtcNow,
            Files = files,
            Comments = new List<Comments>()
        };
        var t = await PostService.CreatePostAsync(post);

        _postModel.Body = "";
        _postModel.Privacy = PostPrivacy.p_public;
        _fileProgresses.Clear();
        t.User = UserState.CurrentUser;
        UserState.CurrentUser.Posts.Add(t);
        Snackbar.Add("Post created", Severity.Success);
        StateHasChanged();
    }

    private double mediaSectionWidth;

    [JSInvokable]
    public void UpdateWidth(double newWidth)
    {
        // Update the width to be 1/3 of the MudPaper's width
        if (newWidth < 600)
            newWidth = 600;
        
        mediaSectionWidth = newWidth / 3;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        // Stop observing when the component is disposed
        await JSRuntime.InvokeVoidAsync("stopResizeObserver", _mudPaperRef);
    }
}