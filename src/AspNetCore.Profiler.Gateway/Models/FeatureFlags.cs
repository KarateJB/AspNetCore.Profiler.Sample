namespace AspNetCore.Profiler.Gateway.Models
{
    public enum FeatureFlags
    {
      /// <summary>
      /// Render Ocelot configuration by environment variables
      /// </summary>
        OcelotConfigRender,

        /// <summary>
        /// Cache Ocelot response in Redis
        /// </summary>
        OcelotCaching
    }
}
