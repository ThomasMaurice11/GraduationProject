using GP.Models;
using GP;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GP.Services;
using Microsoft.OpenApi.Models;
using GP.Hubs;
using GP.Middleware;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.Extensions.Configuration;
using FluentAssertions.Common;


var builder = WebApplication.CreateBuilder(args);




////For Docker
//var environment = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
//var configFile = environment == "true" ? "appsettings.Docker.json" : "appsettings.json";

//builder.Configuration
//    .SetBasePath(Directory.GetCurrentDirectory())
//    .AddJsonFile(configFile, optional: false, reloadOnChange: true);


builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor(); //for getting id from token 


// Add DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    // Configure JWT for SignalR WebSockets
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});


builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));



builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddTransient<IEmailService, EmailService>();
//builder.Services.AddScoped<AnimalRepository>();
//builder.Services.AddScoped<AnimalService>();
//builder.Services.AddHttpClient<DiseasePredictionService>();
//builder.Services.Configure<DiseasePredictionApiSettings>(
//    builder.Configuration.GetSection("DiseasePredictionApi"));




builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});



//// Add CORS policy
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigin", policy =>
//    {
//        policy.WithOrigins("https://your-frontend-url.com") // Replace with your frontend URL
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials(); // Required for SignalR
//    });
//});


//// Add CORS policy to allow all origins
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin() // Allow any origin
//              .AllowAnyHeader() // Allow any header
//              .AllowAnyMethod() // Allow any HTTP method
//              .AllowCredentials(); // Allow credentials (required for SignalR)
//    });
//});
// Add CORS policy
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSignalR", policy =>
//    {
//        policy.WithOrigins("https://localhost:5173") // Allow your frontend origin
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials(); // Required for SignalR
//    });
//});


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowReactApp", policy =>
//    {
//        policy.WithOrigins("http://localhost:5173") // Allow requests from this origin
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials(); // Allow credentials (e.g., cookies, authorization headers)
//    });
//});
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy
//            .AllowAnyOrigin()
//            .AllowAnyHeader()
//            .AllowAnyMethod();
//    });
//});




////////////////////
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost") // allow localhost with any port
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

      


builder.Services.AddSignalR();


var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AuthDbContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RolesSeeder.SeedRoles(roleManager);
}




//app.UseHttpsRedirection();




//app.UseCors("AllowSignalR"); // Enable CORS
app.UseCors("AllowReactApp");
//app.UseCors("AllowMobile");

app.MapHub<ChatHub>("/chatHub"); // Map SignalR hub
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
// Serve static files from wwwroot
app.UseStaticFiles();
app.Run();
