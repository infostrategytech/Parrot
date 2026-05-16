using Parrot.Web.Components;
using Parrot.Web.Features.Auth.ForgotPassword.Services;
using Parrot.Web.Features.Dashboard.AiAgents.State;
using Parrot.Web.Features.Dashboard.Catalog.State;
using Parrot.Web.Features.Dashboard.Channels.State;
using Parrot.Web.Features.Dashboard.Contacts.State;
using Parrot.Web.Components.Pages.Dashboard.Conversations;
using Parrot.Web.Features.Dashboard.QaInsights.State;
using Parrot.Web.Features.Dashboard.TaskRequests.State;
using Parrot.Web.Features.Dashboard.Team.State;
using Parrot.Web.Features.Auth.ForgotPassword.State;
using Parrot.Web.Features.Auth.Login.Services;
using Parrot.Web.Features.Auth.Login.State;
using Parrot.Web.Features.Auth.Register.Services;
using Parrot.Web.Features.Auth.Register.State;
using Parrot.Web.Features.Auth.ResetPassword.Services;
using Parrot.Web.Features.Auth.ResetPassword.State;
using Parrot.Web.Features.Auth.State;
using Parrot.Web.Features.Auth.VerifyEmail.Services;
using Parrot.Web.Features.Auth.VerifyEmail.State;
using Parrot.Web.State;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

string apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]!;

builder.Services.AddHttpClient<ILoginService, LoginService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IRegisterService, RegisterService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IForgotPasswordService, ForgotPasswordService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IResetPasswordService, ResetPasswordService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IVerifyEmailService, VerifyEmailService>(c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<LoginState>();
builder.Services.AddScoped<RegisterState>();
builder.Services.AddScoped<ForgotPasswordState>();
builder.Services.AddScoped<ResetPasswordState>();
builder.Services.AddScoped<VerifyEmailState>();
builder.Services.AddScoped<AppState>();

builder.Services.AddScoped<AiAgentsState>();
builder.Services.AddScoped<CatalogState>();
builder.Services.AddScoped<ChannelsState>();
builder.Services.AddScoped<ContactsState>();
builder.Services.AddScoped<ConversationsState>();
builder.Services.AddScoped<QaInsightsState>();
builder.Services.AddScoped<TaskRequestsState>();
builder.Services.AddScoped<TeamState>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Parrot.Web.Client._Imports).Assembly);

app.Run();
