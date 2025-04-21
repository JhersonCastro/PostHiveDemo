using PostHive.Components;
using PostHive.Services;
using PostHive.SignalR;
using DbContext;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSignalR(options => {
    options.DisableImplicitFromServicesParameters = true;
});

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddResponseCompression(opts =>
    {
        opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "application/octet-stream" });
    });
}

builder.Services.AddDbContextFactory<DatabaseContext>(
    options => 
        options.UseSqlServer(builder.Configuration.GetConnectionString("FreeConnection"),
            sqlOptions =>
            {
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            })
);
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<CookiesService>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RelationshipService>();
builder.Services.AddScoped<PostService>();

builder.Services.AddScoped<UserState>();
builder.Services.AddScoped<ThemeState>();
builder.Services.AddHttpContextAccessor();



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    dataContext.Database.Migrate();
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseResponseCompression();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<CommentHub>("/CommentHub");
app.MapHub<ChatHub>("/ChatHub");

app.Run();