using DbContext.Models;
using Microsoft.AspNetCore.Components;
using PostHive.Services;
using static MudBlazor.CategoryTypes;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PostHive.Components.Pages;

public partial class UserPage
{
    [Parameter] public string Id { get; set; } = string.Empty;

    private User? _userContext;
    private string _previousId = null!;
    public Task RefreshFriends(User user, ActionType actionType)
    {
        if(actionType == ActionType.Remove)
            _userContext?.Friends.Remove(user);
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
                Posts = await PostService.GetPostsAsync(user, UserState.CurrentUser),
                Friends = await UserService.GetFriendsAsync(user.UserId)
            };
            if(UserState.CurrentUser != null)
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

        StateHasChanged();
    }
}