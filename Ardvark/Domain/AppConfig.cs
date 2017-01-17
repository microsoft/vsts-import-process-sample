using System;
using System.Configuration;

namespace Aardvark.Domain
{
    public class AppConfig: IAppConfig
    {
        public AppConfig()
        {

        }

        public string ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationSettings.AppSettings;

                string result = appSettings[key] ?? "Not Found";
                if (result.Equals("")) { result = "Not Found"; }

                return result;
            }
            catch (ConfigurationException)
            {
                return null;
            }
        }
    }

    public interface IAppConfig
    {
        string ReadSetting(string key);
    }
}
