using FluentValidation;
using HotelFlow.API.BackgroundServices;
using HotelFlow.API.Middleware;
using HotelFlow.API.Services;
using HotelFlow.Application.Interfaces;
using HotelFlow.Application.Services;
using HotelFlow.Application.Validators;
using HotelFlow.Infrastructure.Data.DbContext;
using HotelFlow.Infrastructure.Data.Seeders;
using HotelFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
var jwtKey = builder.Configuration["Jwt:Key"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<HotelFlowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRoomService, RoomService>();

builder.Services.AddValidatorsFromAssembly(typeof(CreateRoomRequestValidator).Assembly);

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtTokenGenerator>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddValidatorsFromAssembly(typeof(ChangeUserRoleRequestValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(RegisterRequestValidator).Assembly);

builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddValidatorsFromAssembly(typeof(CreateReservationRequestValidator).Assembly);

builder.Services.AddHostedService<NoShowBackgroundService>();
builder.Services.AddHostedService<ExpiredTasksBackgroundService>();

builder.Services.AddScoped<IHousekeepingService, HousekeepingService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HotelFlowDbContext>();
    await DatabaseSeeder.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
