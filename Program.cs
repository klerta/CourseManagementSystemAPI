using Microsoft.EntityFrameworkCore;
using CourseManagementSystem_API.Data;
using CourseManagementSystem_API.Data.Services;
using System.Text.Json.Serialization;
using CourseManagementSystem_API.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

//DbContext configuration
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

//builder.Services.AddControllers().AddJsonOptions(x =>
//   x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);

//Services configuration
builder.Services.AddTransient<CoursesService>();
builder.Services.AddTransient<MaterialsService>();
builder.Services.AddTransient<CallsService>();
builder.Services.AddTransient<LecturersService>();
builder.Services.AddTransient<StudentCallsService>();
builder.Services.AddTransient<StudentService>();
builder.Services.AddTransient<GroupsService>();
builder.Services.AddTransient<PaymentsService>();
builder.Services.AddTransient<GroupStudentsService>();
builder.Services.AddTransient<UsersService>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<StatisticsService>();


//token validation parameter
var tokenValidationParameter = new TokenValidationParameters()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("this-is-just-a-secret-key-here-stored-somewhere-else")),

    ValidateIssuer = false,
    ValidIssuer = configuration["JWT:Issuer"],

    ValidateAudience = false,
    ValidAudience = configuration["JWT:Audience"],

    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero

};

builder.Services.AddSingleton(tokenValidationParameter);
//add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

//add authentication related code
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

//add jwt bearer
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = tokenValidationParameter;
});
// Add services to the container.

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//seed database
AppDbInitializer.Seed(app);
AppDbInitializer.SeedRoles(app).Wait();


app.Run();
