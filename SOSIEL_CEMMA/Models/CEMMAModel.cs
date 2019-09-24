using Newtonsoft.Json;

namespace SOSIEL_CEMMA.Configuration
{
    /// <summary>
    /// Used to parse section CEMMA game configuration.
    /// </summary>
    public class CEMMAModel
    {
        [JsonProperty(Required = Required.Always)]
        public double Disturbance { get; set; } // Disturbance size

        [JsonProperty(Required = Required.Always)]
        public double DisturbanceIncrement { get; set; } // Disturbance increment

        [JsonProperty(Required = Required.Always)]
        public double Endowment { get; set; } // Fixed number of extracted resource units

        [JsonProperty(Required = Required.Always)]
        public double Externalities { get; set; } // Marginal benefit from sharing

        [JsonProperty(Required = Required.Always)]
        public double PropOfContrib { get; set; } // Proportion of agents who share all

        [JsonProperty(Required = Required.Always)]
        public double VacantSpots { get; set; } // Proportion of empty spots

        [JsonProperty(Required = Required.Always)]
        public double ResourceRegenerationRate { get; set; } // The rate at which the resource regenerates

        [JsonProperty(Required = Required.Always)]
        public double PopulationGrowthRate { get; set; } // The rate at which the population grows each iteration

        [JsonProperty(Required = Required.Always)]
        public double BaseResourceUnits { get; set; } // Base resource unit requirement - the amount of resource units an agent requires during an iteration in order to survive

        [JsonProperty(Required = Required.Always)]
        public double ExtractionDifficulty { get; set; } // Essentially the difficulty in extracting should increase as the amount of resource decreases. By increase in difficulty is meant that for every desired unit, less and less is actually extracted.

    }
}