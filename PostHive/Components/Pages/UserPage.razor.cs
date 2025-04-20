using DbContext.Models;
using Microsoft.AspNetCore.Components;

namespace PostHive.Components.Pages;

public partial class UserPage
{
    [Parameter] public string Id { get; set; } = string.Empty;

    private User? _userContext;
    private string _previousId = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_previousId != Id)
        {
            _previousId = Id;
            Console.WriteLine("Loading user context");
            await LoadUserContextAsync();
        }
    }

    private async Task LoadUserContextAsync()
    {
        try
        {
            UserState.CurrentUser = await CookiesService.RetrievedUser(UserState.CurrentUser);
            var user = await UserService.GetUserById(int.Parse(Id));

            if (user == null)
                throw new Exception("User not found");

            _userContext = new User
            {
                UserId = user.UserId,
                Name = user.Name,
                NickName = user.NickName,
                Avatar = user.Avatar,
                Posts = await PostService.GetPostsAsync(user, UserState.CurrentUser)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            NavigationManager.NavigateTo("/");
        }

        StateHasChanged();
    }
}