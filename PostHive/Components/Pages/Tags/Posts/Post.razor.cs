using DbContext.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PostHive.Components.Pages.Tags.Dialogs;

namespace PostHive.Components.Pages.Tags.Posts;

public partial class Post
{
    [Parameter] public required DbContext.Models.Post CurrentPost { get; set; }

    [Parameter] public required User CurrentUser { get; set; }

    [Parameter] public EventCallback<DbContext.Models.Post?> OnDeleteClick { get; set; }

    public void OpenDialogAsync()
    {
        DialogService.Show<ShareDialog>("Share Post",
            new DialogParameters
            {
                { "Title", "Share post" },
                { "Content", $"{Const.url}/Post/{CurrentPost.PostId}" }
            });
    }

    public async Task DeleteId(DbContext.Models.Post post)
    {
        if (OnDeleteClick.HasDelegate)
        {
            var response = await DialogService.ShowAsync<ConfirmDialog>("Delete post", new DialogParameters
            {
                { "Title", "Delete post" },
                { "Content", "Are you sure you want to delete this post?" }
            });
            var result = await response.Result;
            if (result is { Canceled: false })
                await OnDeleteClick.InvokeAsync(post);
        }
    }
}