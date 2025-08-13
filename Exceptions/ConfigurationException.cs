namespace Exceptions
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string configName)
            : base($"{configName} is null.")
        {
        }
    }
}


