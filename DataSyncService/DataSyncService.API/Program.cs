using Azure.Identity;
using DataSyncService.API.Validation;
using DataSyncService.Database;
using DataSyncService.Domain.Repositories.CoreRepository;
using DataSyncService.Domain.Repositories.SecondaryRepository;
using DataSyncService.Services.Interfaces;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Identity.Web;
using NSwag;
using NSwag.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// App Configuration
if (!builder.Environment.IsDevelopment())
{
	var azureAppConfigurationString = Environment.GetEnvironmentVariable("YOUR_AZURE_APPCONFIGURATION_ENDPOINT");
	builder.Configuration.AddAzureAppConfiguration(options =>
		options
			.Connect(new Uri(azureAppConfigurationString!), new DefaultAzureCredential())
			.Select(KeyFilter.Any, LabelFilter.Null)
			.Select(KeyFilter.Any, AppName)
			.ConfigureKeyVault(kv =>
			{
				kv.SetCredential(new DefaultAzureCredential());
			})
			.UseFeatureFlags());
	builder.Services.AddAzureAppConfiguration();
}

var instance = builder.Configuration.GetValue<string>("AzureAd:Instance");
var tenantId = builder.Configuration.GetValue<string>("AzureAd:TenantId");
var clientId = builder.Configuration.GetValue<string>("AzureAd:ClientId");
var audience = builder.Configuration.GetValue<string>("AzureAd:Audience");
var scope = builder.Configuration.GetValue<string>("AzureAd:Scope");

// Add Azure AD
builder.Services
	.AddAuthentication()
	.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
	.EnableTokenAcquisitionToCallDownstreamApi()
	.AddInMemoryTokenCaches()
	;

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// Add InMemoryCache
builder.Services.AddMemoryCache();

// Fastendpoints
builder.Services
	.AddFastEndpoints()
	.SwaggerDocument(o =>
	{
		o.EnableJWTBearerAuth = false;
		o.DocumentSettings = s =>
		{
			s.DocumentName = "DataSyncService API";
			s.Title = "DataSyncService API";
			s.Version = "v1.0";
			s.AddAuth("oauth2", new()
			{
				Type = OpenApiSecuritySchemeType.OAuth2,
				Flow = OpenApiOAuth2Flow.Implicit,
				AuthorizationUrl = $"{instance}{tenantId}/oauth2/v2.0/authorize",
				TokenUrl = $"{instance}{tenantId}/oauth2/v2.0/token",
				Scopes = new Dictionary<string, string>
				{
					{ $"{audience}/.default", "Default scope" }
				},
			});
		};
	});

// mediator
builder.Services.AddMediatR(cfg => {
	cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// Database
//DataSyncDb
builder.Services.AddDbContext<DataSyncDbContext>(
	options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

// Connect to Core DB
var coreDatabaseConnectionString = builder.Configuration.GetConnectionString("CoreDatabase");
builder.Services.AddScoped<ICoreRepository>(x =>
{
	var conn = new SqlConnection(coreDatabaseConnectionString);
	conn.Open();
	return new CoreRepository(conn);
});

// Connect to Secundary DB
var secundaryDatabaseConnectionString = builder.Configuration.GetConnectionString("SecundaryDatabase");
builder.Services.AddScoped<ISecundaryRepository>(x =>
{
	var conn = new SqlConnection(secundaryDatabaseConnectionString);
	conn.Open();
	return new SecundaryRepository(conn);
});

// Add Application Services
builder.Services.AddScoped<IDataSyncService, DataSyncService.Services.DataSyncService>();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

// Fastendpoints
app
	.UseDefaultExceptionHandler()
	.UseFastEndpoints(c => {
		c.Endpoints.RoutePrefix = "api";
		c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
		c.Errors.UseProblemDetailsResponseBuilder();
	})
	.UseSwaggerGen(o => { }, uiConfig =>
	{
		uiConfig.OAuth2Client = new OAuth2ClientSettings
		{
			ClientId = clientId
		};
	});

app.UseHttpsRedirection();

// Security
app.UseAuthentication();
app.UseAuthorization();


if (!builder.Environment.IsDevelopment())
{
	app.UseAzureAppConfiguration();
}


app.Run();

public partial class Program
{
	public static string Namespace = "DataSyncService.API";
	public static string AppName = "DataSyncService";
}

