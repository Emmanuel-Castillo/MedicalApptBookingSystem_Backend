using MedicalApptBookingSystem.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using MedicalApptBookingSystem.Services;
using MedicalApptBookingSystem.Util;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

// Add services to the container.

builder.Services.AddControllers();

// Configure db
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
        };
    });

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ConvertToDto>();

// Adding CORS (Cross-Origin Resource Sharing) policy
// By default, browsers restrict requests between different origins
// (different domains, ports, protocols, etc.)
// This server must allow requests from frontend, allow credentials in the request, and allow methods in the request
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:3000")    // Allow this specific frontend
        .AllowAnyHeader()   // Allow any custom or default HTTP headers
        .AllowAnyMethod()   // Allow GET, POST, PUT, etc.
        .AllowCredentials();    // CRITICAL when using cookies or sending 'withCredentials: true' from frontend
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();

// MUST BE PLACED BEFORE UseAuthentication() AND UseAuthorization()
app.UseCors("AllowReactFrontend");

// Use authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

