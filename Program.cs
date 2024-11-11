using System.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Set SimpleToken as the default authentication scheme
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = "SimpleToken";
//     options.DefaultChallengeScheme = "SimpleToken";
// }).AddScheme<AuthenticationSchemeOptions, SimpleTokenAuthenticationHandler>("SimpleToken", options => { });

var useJwtAuthentication = builder.Configuration.GetValue<bool>("Authentication:UseJwtAuthentication");

builder.Services.AddAuthentication(options =>
{
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
   
}).AddJwtBearer(options =>
{
    // Configure JWT authentication scheme if it is enabled in the config
    options.Authority = builder.Configuration["Jwt:Issuer"];
    options.Audience = builder.Configuration["Jwt:Audience"];
    if(builder.Environment.IsDevelopment()){
        options.RequireHttpsMetadata = false;
    }else{
        options.RequireHttpsMetadata = true;
    }    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
    }; // Custom validation logic
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                // Resolve CustomJwtTokenValidator from DI container
                var validator = context.HttpContext.RequestServices.GetRequiredService<CustomJwtTokenValidator>();

                return validator.OnTokenValidated(context);
            }
        };
});


builder.Services.AddAuthorization();
    
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
  {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Jwt auth starter kit API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
         Name = "Authorization",
         In = ParameterLocation.Header,
         Type = SecuritySchemeType.ApiKey,
         Scheme = "Bearer"
       });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              },
              Scheme = "oauth2",
              Name = "Bearer",
              In = ParameterLocation.Header,

            },
            new List<string>()
          }
        });   
});


var allowedOrigins = builder.Configuration.GetSection("AllowedOrigin").Get<string[]>();
 builder.Services.AddCors(options =>
            {
                options.AddPolicy("RestrictedDomains", builder =>
                {
                    builder
                        .WithOrigins(allowedOrigins) // Allow requests from any origin (not recommended for production)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });

                // Define more specific policies for different origins or methods
            });

string connectionString = builder.Configuration.GetConnectionString("MyPostgresConnection");
        // Register connection object for DI
builder.Services.AddScoped<IDbConnection>( (con) => {
return new NpgsqlConnection(connectionString);
});  

DependencyRegistrator.Register(builder.Services);
    

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("RestrictedDomains");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
