public class DependencyRegistrator{

    public static void Register(IServiceCollection services){
        
        services.AddScoped<CustomJwtTokenValidator>();
        services.AddScoped<JwtTokenHelper>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserSessionService, UserSessionService>();


    }
}