using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;
using Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RepositoryContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<PasswordHasher<Repositories.Entities.User>>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ChatRepository>();
builder.Services.AddScoped<MessageRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<MessageService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
