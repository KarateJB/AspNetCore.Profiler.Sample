namespace AspNetCore.Profiler.Gateway.Models
{
    public enum FeatureFlags
    {
        /// <summary>
        /// Tempalte configuration for appsettings.*.json.
        /// Enable this feature to render the environment variables into the j2 templating configuration files.
        /// </summary>
        TemplateConfig,

        /// <summary>
        /// Cache Ocelot response in Redis
        /// </summary>
        OcelotCaching
    }
}
