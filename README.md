# Dapper.Extensions - Dapper-based extension library,Support circuit breaker based on Polly and AspectCore.

## Components
* [Dapper](https://github.com/StackExchange/Dapper)
* [AspectCore](https://github.com/dotnetcore/AspectCore-Framework)
* [Polly](https://github.com/App-vNext/Polly/)

## Installing via NuGet

Install-Package Dapper.Extensions.NetCore

Install-Package Dapper.Extensions.MySql

Install-Package Dapper.Extensions.PostgreSql

Install-Package Dapper.Extensions.Odbc

Install-Package Dapper.Extensions.CircuitBreaker

Install-Package Dapper.Extensions.CircuitBreaker.Autofac

Install-Package Dapper.Extensions.CircuitBreaker.DependencyInjection


## Usage
```cssharp
public IServiceProvider ConfigureServices(IServiceCollection services)
		{

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();

			var builder = new ContainerBuilder();
			builder.Populate(services);
			builder.Register<Func<string, IDapper>>(c =>
			{
				var container = c.Resolve<IComponentContext>();
				return named => container.ResolveNamed<IDapper>(named);
			});
			builder.RegisterType<MySqlDapper>().Named<IDapper>("mysql-conn").WithParameter("connectionName", "mysql").PropertiesAutowired().InstancePerLifetimeScope();
			builder.RegisterType<MsSqlDapper>().Named<IDapper>("msql-conn").WithParameter("connectionName", "mssql").PropertiesAutowired().InstancePerLifetimeScope();
			builder.RegisterType<OdbcDapper>().Named<IDapper>("obdc-conn").WithParameter("connectionName", "odbc").PropertiesAutowired().InstancePerLifetimeScope();
			builder.RegisterType<PostgreSqlDapper>().Named<IDapper>("postgre-conn").WithParameter("connectionName", "postgresql").InstancePerLifetimeScope();
			builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
				.Where(t => t.Name.EndsWith("Controller"))
				.PropertiesAutowired().InstancePerLifetimeScope();
			ApplicationContainer = builder.Build();
			return new AutofacServiceProvider(ApplicationContainer);
		}
```


```cssharp
public class ValuesController : ControllerBase
	{
		public IDapper Repo { get; set; }

		public ValuesController(Func<string, IDapper> res)
		{
			Repo = res("mysql-conn");
		}
		// GET api/values
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var result = await Repo.QueryPageAsync("select count(*) from tab", "select * from tab limit @Skip,@Take;", 1, 10);

			return Ok(result);
		}
	}
```


## Circuit breaker

### Use Autofac
```cssharp
public IServiceProvider ConfigureServices(IServiceCollection services)
		{

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();

			var builder = new ContainerBuilder();
			builder.Populate(services);
			builder.Register<Func<string, IDapper>>(c =>
			{
				var container = c.Resolve<IComponentContext>();
				return named => container.ResolveNamed<IDapper>(named);
			});
			builder.RegisterType<MySqlDapper>().Named<IDapper>("mysql-conn").WithParameter("connectionName", "mysql").PropertiesAutowired().InstancePerLifetimeScope();
			builder.AddDapperCircuitBreaker();
			ApplicationContainer = builder.Build();
			return new AutofacServiceProvider(ApplicationContainer);
		}
```


### Use DependencyInjection
```cssharp
public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();
			services.AddDapperCircuitBreaker();
		}
```
