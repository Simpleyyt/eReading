using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace eReading
{
    public class ConfigSectionData : ConfigurationSection
    {
        [ConfigurationProperty("Path")]
        public String Path
        {
            get { return (String)this["Path"]; }
            set { this["Path"] = value; }
        }

        [ConfigurationProperty("BookInfo")]
        public String BookInfo
        {
            get { return (String)this["BookInfo"]; }
            set { this["BookInfo"] = value; }
        }
    }

    public class ConfigureHelper
    {
        private Configuration config;
        private int i = 0;
        public ConfigureHelper(String path)
        {
            ExeConfigurationFileMap file = new ExeConfigurationFileMap();
            file.ExeConfigFilename = path;
            config = ConfigurationManager.OpenMappedExeConfiguration(file, ConfigurationUserLevel.None);
        }

        public String ReadValue(String Key, String Defaule = null)
        {
            try
            {
                return config.AppSettings.Settings[Key].Value;
            }
            catch
            {
                return Defaule;
            }
        }

        public void Save()
        {
            config.Save();
        }

        public void WriteValue(String Key, String Value)
        {
            try
            {
                config.AppSettings.Settings[Key].Value = Value;
            }
            catch
            {
                config.AppSettings.Settings.Add(Key, Value);
            }
        }

        public void CreatGrouop(String group)
        {
            try
            {
                config.SectionGroups.Remove(group);
            }
            catch
            { }
            i = 0;
            config.SectionGroups.Add(group, new ConfigurationSectionGroup());
        }

        public void AddValue(String group, ConfigSectionData data)
        {
            config.SectionGroups[group].Sections.Add("add"+i, data);
            i++;
        }

        public ConfigurationSectionCollection GetValue(String group)
        {
            return config.SectionGroups[group].Sections;
        }
    }
}
