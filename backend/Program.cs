using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RawMessageWorker;
using Repositories.Context;
using Repositories.Repositories;
using Serilog;
using Services;
using Services.AutoMapper;
using Services.Helpers.Mail_Helpers;
using Services.Helpers.WebSocket_Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RepositoryContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(UserProfile));

builder.Services.AddScoped<DbContext, RepositoryContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
            ValidAudience = builder.Configuration["JwtConfig:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("izi-pizi-lemon-squeezy-oguz-celal-ankaraa")),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
builder.Services.AddAuthorization();


builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PasswordHasher<Repositories.Entities.User>>();
builder.Services.AddScoped<MailSender>();
builder.Services.AddScoped<MessageReadRepository>();
builder.Services.AddScoped<MessageReadService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ChatRepository>();
builder.Services.AddScoped<MessageRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddSingleton<WSListManager>();
builder.Services.AddScoped<WSManager>();
builder.Services.AddScoped<ProcessMessage>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<RepositoryContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database migration error");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();
app.MapControllers();

app.Run();
