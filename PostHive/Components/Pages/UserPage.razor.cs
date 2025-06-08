using DbContext.Models;
using Microsoft.AspNetCore.Components;
using PostHive.Services;

namespace PostHive.Components.Pages;

public partial class UserPage
{
    [Parameter] public string Id { get; set; } = string.Empty;
    private bool loading = true;
    private User? _userContext;
    private string _previousId = null!;
    public Task RefreshFriends(User? user, ActionType actionType)
    {
        if (_userContext == null || user == null)
            return Task.CompletedTask;
        switch (actionType)
        {
            case ActionType.Remove:
                user.Friends.Remove(_userContext);
                _userContext.Friends.Remove(user);
                break;
            case ActionType.Add:
                user.Friends.Add(_userContext);
                _userContext.Friends.Add(user);
                break;
            case ActionType.Block:
                NavigationManager.NavigateTo("/");
                break;
        }
        StateHasChanged();
        return Task.CompletedTask;
    }
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
            loading = true;
            StateHasChanged();
            UserState.CurrentUser = await CookiesService.RetrievedUser(UserState.CurrentUser);
            var user = await UserService.GetUserById(int.Parse(Id));

            if (user == null)
                throw new Exception("User not found");
            if (user.UserId.Equals(UserState.CurrentUser?.UserId))
                NavigationManager.NavigateTo("/profile");
            _userContext = new User
            {
                UserId = user.UserId,
                Name = user.Name,
                NickName = user.NickName,
                Bio = user.Bio,
                Avatar = user.Avatar,
                Posts = await PostService.GetPostsAsync(user, UserState.CurrentUser),
                Friends = await UserService.GetFriendsAsync(user.UserId)
            };
            if (UserState.CurrentUser != null)
            {
                var relationship = await RelationshipService.GetRelationship(UserState.CurrentUser, user);
                if (relationship is { Status: RelationshipStatus.blocked }) NavigationManager.NavigateTo("/");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            NavigationManager.NavigateTo("/");
        }
        loading = false;
        StateHasChanged();
    }
}