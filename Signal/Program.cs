using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Signal.Classes;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/login");
builder.Services.AddAuthorization();

builder.Services.AddSignalR();

var connectionString = builder.Configuration.GetConnectionString("ChatDbConnection");
builder.Services.AddDbContext<ChatDBContext>(options => options.UseSqlite(connectionString));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", async context =>
    await SendHtmlAsync(context, "html/login.html"));

app.MapPost("/login", async (string? returnUrl, HttpContext context, ChatDBContext chatDBContext) =>
{
    var form = context.Request.Form;

    if (!form.ContainsKey("email")) 
        return Results.BadRequest("�� ����� Email");
    if (!form.ContainsKey("password")) 
        return Results.BadRequest("�� ����� ������");

    string email = form["email"];
    string password = form["password"];

    User? user = chatDBContext.Users.FirstOrDefault(p => p.Email == email);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    bool passwordComparison = BCrypt.Net.BCrypt.Verify(password, user.Password);
    if (!passwordComparison) return Results.BadRequest("�������� ������");

    var claims = new List<Claim>
    {
        new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
        new Claim(ClaimTypes.NameIdentifier, user.Name)
    };
    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    await context.SignInAsync(claimsPrincipal);

    return Results.Redirect(returnUrl ?? "/");
});

app.MapGet("/register", async context =>
    await SendHtmlAsync(context, "html/register.html"));

app.MapPost("/register", (string? returnUrl, HttpContext context, ChatDBContext chatDBContext) =>
{
    var form = context.Request.Form;

    if (!form.ContainsKey("name"))
        return Results.BadRequest("Name �������� ������������");
    if (!form.ContainsKey("email"))
        return Results.BadRequest("Email �������� ������������");
    if (!form.ContainsKey("password"))
        return Results.BadRequest("Password �������� ������������");

    string name = form["name"];
    string email = form["email"];
    string password = BCrypt.Net.BCrypt.HashPassword(form["password"]);

    User user = new User { Name = name, Email = email, Password = password };
    try
    {
        chatDBContext.Users.Add(user);
        chatDBContext.SaveChanges();
    } 
    catch
    {
        return Results.Problem("�������� ������ ��� ����������� ������������");
    }

    return Results.Redirect(returnUrl ?? "/");
});

app.MapGet("/", [Authorize] async (HttpContext context) =>
    await SendHtmlAsync(context, "html/index.html"));


app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});


app.MapHub<ChatHub>("/chat");
app.Run();

async Task SendHtmlAsync(HttpContext context, string path)
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(path);
}

record class Person(string Email, string Password);
