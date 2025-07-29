using FakeFacebook.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.OpenApi.Models;
using FakeFacebook.Hubs;
using FakeFacebook.Service;


var builder = WebApplication.CreateBuilder(args);
var key = builder.Configuration["JwtSettings:SecretKey"] ?? "2372003HungsssDepZaiSieuCapVuTru";
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("https://0.0.0.0:7158", "http://0.0.0.0:5176");
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5176);
        options.ListenAnyIP(7158, listenOptions =>
        {
            listenOptions.UseHttps();
        });
    });
} else {
    var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(Int32.Parse(port));
    });
}
builder.Services.AddCors(options => {
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200",
                                "https://angular-fb-beta.vercel.app",
                                "https://angular-fb-deploy.vercel.app")
                  .AllowCredentials()
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                 ;
        });
});
builder.Services.AddSignalR();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // có thể bật true khi dùng HTTPS
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
            {
                context.Token = accessToken;
            }
            else if (context.Request.Headers.ContainsKey("Authorization"))
            {
                // Ưu tiên lấy token từ Authorization header
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    context.Token = authHeader["Bearer ".Length..].Trim();
                }
              
            }
            else
            {
                // Nếu không có header, lấy từ cookie
                var tokenFromCookie = context.Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(tokenFromCookie))
                {
                    context.Token = tokenFromCookie;
                }
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\":\"Bạn cần đăng nhập để truy cập vào hệ thống.\"}");
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewSensitiveDataPolicy", policy =>
        policy.RequireRole("Admin")
              .RequireClaim("Permission", "ViewSensitiveData"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API (Try with: Admin - 1234)",
        Description = "An ASP.NET Core Web API for managing ToDo items",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "FontendTest",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {{
        new OpenApiSecurityScheme{
            Reference = new OpenApiReference{
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }});
});
builder.Services.AddScoped<GoogleAuthService>();
builder.Services.AddControllers();
builder.Services.AddDbContext<FakeFacebookDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SmarterDatabase")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<GitHubUploaderSevice>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    //c.RoutePrefix = string.Empty;
});
app.UseDefaultFiles();
//if (app.Environment.IsDevelopment()) { app.UseHttpsRedirection(); }
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/hub");
app.MapControllers();
app.Run();
//Tools > NuGet Package Manager > Package Manager Console
//Add-Migration InitialCreate
//Update-Database
