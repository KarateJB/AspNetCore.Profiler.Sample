namespace AspNetCore.Profiler.Gateway.Models
{
    public class AppSettings
    {
       public RedisSetting Redis { get; set; } 
    }

    public class RedisSetting
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }
}