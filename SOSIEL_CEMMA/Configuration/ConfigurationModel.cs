using System.Collections.Generic;
using Newtonsoft.Json;
using SOSIEL.Entities;

namespace SOSIEL_CEMMA.Configuration
{
    /// <summary>
    /// Main configuration model.
    /// </summary>
    public class ConfigurationModel
    {
        [JsonRequired]
        public AlgorithmConfiguration AlgorithmConfiguration { get; set; }

        [JsonRequired]
        public Dictionary<string, AgentPrototype> AgentConfiguration { get; set; }

        [JsonRequired]
        public InitialStateConfiguration InitialState { get; set; }

        /*
        [JsonRequired]
        public CEMMAModel CEMMAModel { get; set; }
        */

        public string ConfigurationPath { get; set; }

    }
}
