using DisasterLink_API.Data;
using Microsoft.EntityFrameworkCore;        // Para AddDbContext e EntityFrameworkCore
using Microsoft.OpenApi.Models;             // Para AddSwaggerGen
using AutoMapper;
using DisasterLink_API.Mappings;
using DisasterLink_API.Interfaces.Repositories;
using DisasterLink_API.Repositories;
using DisasterLink_API.Interfaces.Services;
using DisasterLink_API.Services;
using DisasterLink_API.Config;              // Para SwaggerDefaultValues
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.IO;
using DisasterLink_API.Middleware;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IAbrigoTemporarioRepository, AbrigoTemporarioRepository>();
builder.Services.AddScoped<IAbrigoTemporarioService, AbrigoTemporarioService>();
builder.Services.AddScoped<IAlertaRepository, AlertaRepository>();
builder.Services.AddScoped<IAlertaService, AlertaService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Repositórios e Serviços para Admin
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Serviços e Repositórios para Pontos de Coleta de Doações
builder.Services.AddScoped<IPontoDeColetaDeDoacoesRepository, PontoDeColetaDeDoacoesRepository>();
builder.Services.AddScoped<IParticipacaoPontoColetaRepository, ParticipacaoPontoColetaRepository>();
builder.Services.AddScoped<IPontoDeColetaDeDoacoesService, PontoDeColetaDeDoacoesService>();
builder.Services.AddScoped<IVisualizacaoAlertaRepository, VisualizacaoAlertaRepository>();

// Adicionar serviços para Swashbuckle.AspNetCore.Filters
builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

// Customização de respostas de validação
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.First().ErrorMessage);
        return new BadRequestObjectResult(new { errors });
    };
});

// Swagger com XML Comments
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    
    c.UseInlineDefinitionsForEnums();
    c.SupportNonNullableReferenceTypes();
    c.SchemaFilter<SwaggerDefaultValues>();
    c.ExampleFilters();

    c.AddSecurityDefinition("CustomToken", new OpenApiSecurityScheme
    {
        Description = "Token de autorização personalizado. Exemplo: 'Bearer {seu token de 32 caracteres}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "CustomToken" }
            },
            new string[] { }
        }
    });
});

builder.Services.AddDbContext<DisasterLinkDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException("A chave JWT (Jwt:Key) deve estar definida em appsettings.json e ter pelo menos 32 caracteres.");
}
var key = Encoding.ASCII.GetBytes(jwtKey);

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
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IRecomendacaoPontoColetaService, RecomendacaoPontoColetaService>();

builder.WebHost.UseUrls("http://0.0.0.0:8080");

var app = builder.Build();

// Inicializa o cache do modelo de recomendação na inicialização da aplicação
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        // Garantir que o diretório MLModels exista
        var mlModelsDir = Path.Combine(AppContext.BaseDirectory, "MLModels");
        if (!Directory.Exists(mlModelsDir))
        {
            Directory.CreateDirectory(mlModelsDir);
            logger.LogInformation("Diretório MLModels criado.");
        }
        
        var recomendacaoService = services.GetRequiredService<IRecomendacaoPontoColetaService>();
        await recomendacaoService.InicializarModeloAsync(); // Alterado para usar o novo método de inicialização
        logger.LogInformation("Serviço de Recomendação ML.NET inicializado (modelo carregado ou treinado se necessário).");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Um erro ocorreu durante a inicialização do modelo de recomendação ML.NET.");
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<DisasterLinkDbContext>();
        await DataSeeder.SeedDatabaseAsync(dbContext!, services);
        logger.LogInformation("Banco de dados semeado (se necessário).");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Um erro ocorreu durante a semeadura do banco de dados.");
    }
}

app.UseStaticFiles();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseIpRateLimiting();
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.InjectStylesheet("/swagger-ui.css");
    c.InjectJavascript("/swagger-custom.js");
});
app.UseHttpsRedirection();
app.MapControllers();
app.Run();