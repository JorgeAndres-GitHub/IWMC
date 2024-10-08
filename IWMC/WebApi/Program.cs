using AccesoDatos.Configuration;
using AccesoDatos.Context;
using AccesoDatos.Operaciones;
using AccesoDatos.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using NLog.Web;
using NLog;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Furniture_Store_API",
            Version = "v1"
        });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = $@"JWT Authorization header using the Bearer scheme.
                            {Environment.NewLine} Enter prefix (Bearer), space, and then your token. 
                            Example: 'Bearer 12314313k413klasd'"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference=new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    }
                },
                new string[] { }

            }
    });
    });

    builder.Services.AddCors(policyBuilder=> policyBuilder.AddDefaultPolicy(policy=> policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod()));


    builder.Services.AddDbContext<AppCarrosContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSql")));

    builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

    //Email
    builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
    builder.Services.AddSingleton<IEmailSender, EmailService>();

    builder.Services.AddScoped<UsuarioDAO>();
    builder.Services.AddScoped<AutoDAO>();
    builder.Services.AddScoped<CuentasBancariasDAO>();
    builder.Services.AddScoped<AutoCompradoDAO>();

    var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtConfig:Secret").Value);

    var tokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,//Al desplegarse tiene que ser true
        ValidateAudience = false,//Al desplegarse tiene que ser true
        RequireExpirationTime = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    builder.Services.AddSingleton(tokenValidationParameters);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(jwt =>
    {
        jwt.SaveToken = true;
        jwt.TokenValidationParameters = tokenValidationParameters;
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("SuperRol", policy => policy.RequireClaim("Rol", "Admin"));
    });


    //NLog
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception e)
{
    logger.Error(e, "There has been an error ");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}


