using Application.Api;
using Application.Core.Data;
using Application.Core.Exceptions;
using Application.Core.PermissionHelpers;
using Application.Repositories;
using Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PbApp.Service;
using System.Text;

public static class RegisterStartupServices
    {
        public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
        {
          builder.Services.AddDbContext<GHP_POS_DBContext>(options =>
          options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        //builder.Services.AddClientDbContext(builder.Configuration);
        builder.Services.AddControllers();
           //cors
        builder.Services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy", policy =>
            {
            policy
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Value.Split(","));
            });
        });
        //....cors end
        builder.Services.AddSignalR();
        builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        builder.Services.AddScoped(typeof(IBaseCommandService<>), typeof(BaseCommandService<>));
        builder.Services.AddScoped(typeof(IBaseQueryService<,,>), typeof(BaseQueryService<,,>));
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ICommonService, CommonService>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IPdfService, PdfService>();
        builder.Services.AddScoped<IImportManager, ImportManager>();


        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
           .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
       
        //swagger
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API",
                Version = "v1"
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        }, new List<string>()
                    },
                });

        });
        //end swagger

        //jwt
        builder.Services.AddAuthentication(authentication =>
        {
            authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                           .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
                   ValidateLifetime = true,
                   ValidateIssuer = false,
                   ValidateAudience = false
               };
               options.Events = new JwtBearerEvents
               {
                   OnChallenge = context =>
                   {
                       context.HandleResponse();
                       if (!context.Response.HasStarted)
                       {
                           throw new UnauthorizationException("You are not Authorized.");
                       }

                       return Task.CompletedTask;
                   },
                   OnForbidden = context =>
                   {
                       throw new UnauthorizationException("You are not authorized to access this resource.");
                   },
               };
           });
        //end jwt
 
            return builder;
        }
    }

