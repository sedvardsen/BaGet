using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using BaGet.AWS;
using BaGet.AWS.Configuration;
using BaGet.AWS.Extensions;
using BaGet.Azure.Configuration;
using BaGet.Azure.Extensions;
using BaGet.Azure.Search;
using BaGet.Core.Authentication;
using BaGet.Core.Configuration;
using BaGet.Core.Entities;
using BaGet.Core.Extensions;
using BaGet.Core.Indexing;
using BaGet.Core.Mirror;
using BaGet.Core.Search;
using BaGet.Core.Server.Extensions;
using BaGet.Core.State;
using BaGet.Core.Storage;
using BaGet.Database.MySql;
using BaGet.Database.PostgreSql;
using BaGet.Database.Sqlite;
using BaGet.Database.SqlServer;
using BaGet.GCP.Configuration;
using BaGet.GCP.Extensions;
using BaGet.GCP.Services;
using BaGet.Protocol;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading.Tasks;
using BaGet.Core.Services;

namespace BaGet.Extensions
{



    public static class NugetAuthenticationExtensions
    {

        public static AuthenticationBuilder AddNugetAuthentication(this IServiceCollection services, Func<ICredentials, Task<bool>> credentialsValidation)
        {
            return services.AddAuthentication("Basic").AddNuget(credentialsValidation);
        }


        //public static AuthenticationBuilder AddNuget<TAuthService>(this AuthenticationBuilder builder)
        //    where TAuthService : class, IAuthenticationService
        //{
        //    //return AddBasic<TAuthService>(builder, BasicAuthenticationDefaults.AuthenticationScheme, _ => { });
        //    return AddBasic<TAuthService>(builder);
        //}


        public static AuthenticationBuilder AddNuget<TAuthService>(this AuthenticationBuilder builder, string authenticationScheme, Action<NugetAuthenticationOptions> configureOptions)
               where TAuthService : class, ICredentialsValidationService
        {
            builder.Services.AddSingleton<IPostConfigureOptions<NugetAuthenticationOptions>, NugetAuthenticationPostConfigureOptions>();
            builder.Services.AddTransient<ICredentialsValidationService, TAuthService>();

            return builder.AddScheme<NugetAuthenticationOptions, NugetAuthenticationHandler>(
                authenticationScheme, configureOptions);
        }



        public static AuthenticationBuilder AddNuget(this AuthenticationBuilder builder, Func<ICredentials,Task<bool>> credentialsValidation)
        {
            builder.Services.AddSingleton<IPostConfigureOptions<NugetAuthenticationOptions>, NugetAuthenticationPostConfigureOptions>();
            builder.Services.AddSingleton<ICredentialsValidationService>(new CredentialsValidationService(credentialsValidation));

            return builder.AddScheme<NugetAuthenticationOptions, NugetAuthenticationHandler>("Basic", (opt) =>
            {
                Debug.WriteLine(opt);
            });
        }

         


    }


    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureBaGet(
            this IServiceCollection services,
            IConfiguration configuration,
            bool httpServices = false)
        {
            services.ConfigureAndValidate<BaGetOptions>(configuration);
            services.ConfigureAndValidate<SearchOptions>(configuration.GetSection(nameof(BaGetOptions.Search)));
            services.ConfigureAndValidate<MirrorOptions>(configuration.GetSection(nameof(BaGetOptions.Mirror)));
            services.ConfigureAndValidate<StorageOptions>(configuration.GetSection(nameof(BaGetOptions.Storage)));
            services.ConfigureAndValidate<DatabaseOptions>(configuration.GetSection(nameof(BaGetOptions.Database)));
            services.ConfigureAndValidate<FileSystemStorageOptions>(configuration.GetSection(nameof(BaGetOptions.Storage)));
            services.ConfigureAndValidate<BlobStorageOptions>(configuration.GetSection(nameof(BaGetOptions.Storage)));
            services.ConfigureAndValidate<AzureSearchOptions>(configuration.GetSection(nameof(BaGetOptions.Search)));

            services.ConfigureAzure(configuration);
            services.ConfigureAws(configuration);
            services.ConfigureGcp(configuration);

            if (httpServices)
            {
                services.ConfigureHttpServices();
            }

            services.AddBaGetContext();

            services.AddTransient<IPackageService, PackageService>();
            services.AddTransient<IPackageIndexingService, PackageIndexingService>();
            services.AddTransient<IPackageDeletionService, PackageDeletionService>();
            services.AddTransient<ISymbolIndexingService, SymbolIndexingService>();
            services.AddSingleton<IFrameworkCompatibilityService, FrameworkCompatibilityService>();
            services.AddMirrorServices();

            services.AddStorageProviders();
            services.AddSearchProviders();

            //we need better naming to distinguish Api Key authentication (push)  from feed Authentication (pull)
            services.AddAuthenticationProviders(); //API-Key
            var restrictToAzureDevopsOrg = configuration[nameof(BaGetOptions.RestrictedToAzureDevopsOrg)];
            if (string.IsNullOrEmpty(restrictToAzureDevopsOrg))
            {
                services.AddNugetAuthentication((cred) => Task.FromResult(true));
            }
            else
            {
                services.AddNugetAuthentication((cred) => checkAccessInOrg(cred, restrictToAzureDevopsOrg));
            }
            

            return services;
        }

        private static async Task<bool> checkAccessInOrg(ICredentials cred, string restrictToAzureDevopsOrg)
        {
            var client = new HttpClient();
            var credentials = cred.GetCredential(new Uri("http://tempuri.org"), "Basic");
            var byteArray = Encoding.ASCII.GetBytes($"notused:{credentials.Password}");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var call = await client.GetAsync($"https://dev.azure.com/{restrictToAzureDevopsOrg}/_apis/build/builds?api-version=5.0&$top=0");
            
            return call.IsSuccessStatusCode;
        }

        public static IServiceCollection AddBaGetContext(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddScoped<IContext>(provider =>
            {
                var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();

                switch (databaseOptions.Value.Type)
                {
                    case DatabaseType.Sqlite:
                        return provider.GetRequiredService<SqliteContext>();

                    case DatabaseType.SqlServer:
                        return provider.GetRequiredService<SqlServerContext>();

                    case DatabaseType.MySql:
                        return provider.GetRequiredService<MySqlContext>();

                    case DatabaseType.PostgreSql:
                        return provider.GetRequiredService<PostgreSqlContext>();

                    default:
                        throw new InvalidOperationException(
                            $"Unsupported database provider: {databaseOptions.Value.Type}");
                }
            });

            services.AddDbContext<SqliteContext>((provider, options) =>
            {
                var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();

                options.UseSqlite(databaseOptions.Value.ConnectionString);
            });

            services.AddDbContext<SqlServerContext>((provider, options) =>
            {
                var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();

                options.UseSqlServer(databaseOptions.Value.ConnectionString);
            });

            services.AddDbContext<MySqlContext>((provider, options) =>
            {
                var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();

                options.UseMySql(databaseOptions.Value.ConnectionString);
            });

            services.AddDbContext<PostgreSqlContext>((provider, options) =>
            {
                var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();

                options.UseNpgsql(databaseOptions.Value.ConnectionString);
            });

            return services;
        }

        public static IServiceCollection ConfigureAzure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.ConfigureAndValidate<BlobStorageOptions>(configuration.GetSection(nameof(BaGetOptions.Storage)));
            services.ConfigureAndValidate<AzureSearchOptions>(configuration.GetSection(nameof(BaGetOptions.Search)));

            return services;
        }

        public static IServiceCollection ConfigureAws(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.ConfigureAndValidate<S3StorageOptions>(configuration.GetSection(nameof(BaGetOptions.Storage)));

            return services;
        }

        public static IServiceCollection ConfigureGcp(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.ConfigureAndValidate<GoogleCloudStorageOptions>(configuration.GetSection(nameof(BaGetOptions.Storage)));

            return services;
        }

        public static IServiceCollection AddStorageProviders(this IServiceCollection services)
        {
            services.AddSingleton<NullStorageService>();
            services.AddTransient<FileStorageService>();
            services.AddTransient<IPackageStorageService, PackageStorageService>();
            services.AddTransient<ISymbolStorageService, SymbolStorageService>();

            services.AddBlobStorageService();
            services.AddS3StorageService();
            services.AddGoogleCloudStorageService();

            services.AddTransient<IStorageService>(provider =>
            {
                var options = provider.GetRequiredService<IOptionsSnapshot<BaGetOptions>>();

                switch (options.Value.Storage.Type)
                {
                    case StorageType.FileSystem:
                        return provider.GetRequiredService<FileStorageService>();

                    case StorageType.AzureBlobStorage:
                        return provider.GetRequiredService<BlobStorageService>();

                    case StorageType.AwsS3:
                        return provider.GetRequiredService<S3StorageService>();

                    case StorageType.GoogleCloud:
                        return provider.GetRequiredService<GoogleCloudStorageService>();

                    case StorageType.Null:
                        return provider.GetRequiredService<NullStorageService>();

                    default:
                        throw new InvalidOperationException(
                            $"Unsupported storage service: {options.Value.Storage.Type}");
                }
            });

            return services;
        }

        public static IServiceCollection AddSearchProviders(this IServiceCollection services)
        {
            services.AddTransient<ISearchService>(provider =>
            {
                var options = provider.GetRequiredService<IOptionsSnapshot<SearchOptions>>();

                switch (options.Value.Type)
                {
                    case SearchType.Database:
                        return provider.GetRequiredService<DatabaseSearchService>();

                    case SearchType.Azure:
                        return provider.GetRequiredService<AzureSearchService>();

                    case SearchType.Null:
                        return provider.GetRequiredService<NullSearchService>();

                    default:
                        throw new InvalidOperationException(
                            $"Unsupported search service: {options.Value.Type}");
                }
            });

            services.AddTransient<DatabaseSearchService>();
            services.AddSingleton<NullSearchService>();
            services.AddAzureSearch();

            return services;
        }

        /// <summary>
        /// Add the services that mirror an upstream package source.
        /// </summary>
        /// <param name="services">The defined services.</param>
        public static IServiceCollection AddMirrorServices(this IServiceCollection services)
        {
            services.AddTransient<FakeMirrorService>();
            services.AddTransient<MirrorService>();

            services.AddTransient<IMirrorService>(provider =>
            {
                var options = provider.GetRequiredService<IOptionsSnapshot<MirrorOptions>>();

                if (!options.Value.Enabled)
                {
                    return provider.GetRequiredService<FakeMirrorService>();
                }
                else
                {
                    return provider.GetRequiredService<MirrorService>();
                }
            });

            services.AddTransient<IPackageContentClient, PackageContentClient>();
            services.AddTransient<IRegistrationClient, RegistrationClient>();
            services.AddTransient<IServiceIndexClient, ServiceIndexClient>();
            services.AddTransient<IPackageMetadataService, PackageMetadataService>();

            services.AddSingleton<IServiceIndexService>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MirrorOptions>>();
                var serviceIndexClient = provider.GetRequiredService<IServiceIndexClient>();

                return new ServiceIndexService(
                    options.Value.PackageSource.ToString(),
                    serviceIndexClient);
            });

            services.AddTransient<IPackageDownloader, PackageDownloader>();

            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BaGetOptions>>().Value;

                var assembly = Assembly.GetEntryAssembly();
                var assemblyName = assembly.GetName().Name;
                var assemblyVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

                var client = new HttpClient(new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                });

                client.DefaultRequestHeaders.Add("User-Agent", $"{assemblyName}/{assemblyVersion}");
                client.Timeout = TimeSpan.FromSeconds(options.Mirror.PackageDownloadTimeoutSeconds);

                return client;
            });

            services.AddSingleton<DownloadsImporter>();
            services.AddSingleton<IPackageDownloadsSource, PackageDownloadsJsonSource>();

            return services;
        }

        public static IServiceCollection AddAuthenticationProviders(this IServiceCollection services)
        {
            services.AddTransient<BaGet.Core.Authentication.IAuthenticationService, ApiKeyAuthenticationService>();
            return services;
        }
    }
}
