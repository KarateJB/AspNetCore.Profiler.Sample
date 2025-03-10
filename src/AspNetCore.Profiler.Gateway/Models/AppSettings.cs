namespace AspNetCore.Profiler.Gateway.Models;

public class AppSettings
{
    public RedisSetting Redis { get; set; }
    public JwtSetting Jwt { get; set; }
}

public class RedisSetting
{
    public string Name { get; set; }
    public string ConnectionString { get; set; }
}

public class JwtSetting
{
    public string Provider  { get; set; }
    public string SecretKey { get; set; }
    public string Issuer  { get; set; }
    public string Audience  { get; set; }
}

