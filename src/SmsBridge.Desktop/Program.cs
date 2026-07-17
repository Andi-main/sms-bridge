using SmsBridge.Desktop.Components;
using SmsBridge.Desktop.Contracts;
using SmsBridge.Desktop.Models;
using SmsBridge.Desktop.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<IMessageStore, InMemoryMessageStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapPost("/api/messages",
    (IncomingMessageRequest request, IMessageStore messageStore) =>
    {
        string sender = request.Sender?.Trim() ?? string.Empty;
        string body = request.Body?.Trim() ?? string.Empty;

        Dictionary<string, string[]> errors = [];

        if (sender.Length == 0)
        {
            errors["sender"] = ["Sender is required."];
        }

        if (body.Length == 0)
        {
            errors["body"] = ["Message body is required."];
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        SmsMessage message = new()
        {
            Sender = sender,
            Body = body,
            ReceivedAt = request.ReceivedAt ?? DateTime.Now
        };

        messageStore.AddMessage(message);

        return Results.Created("/api/messages", message);

    });

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
