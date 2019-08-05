using Newtonsoft.Json;

namespace SOSIEL_CEMMA.Configuration
{
    /// <summary>
    /// Used to parse section CEMMA game configuration.
    /// </summary>
    public class CEMMAModel
    {
        [JsonRequired]
        public static double Disturbance { get; set; } = 1; // Disturbance size

        [JsonRequired]
        public static double DisturbanceIncrement { get; set; } = 0.5; // Disturbance increment

        [JsonRequired]
        public static double Endowment { get; set; } = 10; // Fixed number of extracted resource units

        [JsonRequired]
        public static double Externalities { get; set; } = 4;// Marginal benefit from sharing

        //[JsonRequired]
        //public static int NumOfIterations { get; set; } //= 100;  Number of iterations.

        //[JsonRequired]
        //public static int NumOfAgents { get; set; } //= 100;  The number of agents in a simulation. Must be equal to or greater than ten.

        [JsonRequired]
        public static double PropOfContrib { get; set; } = 0.30; // Proportion of agents who share all

        [JsonRequired]
        public static double VacantSpots { get; set; } = 0.3; // Proportion of empty spots

        //[JsonRequired]
        //public static int SocialSpaceSize { get; set; } = (int)System.Math.Round(NumOfAgents / (1 - VacantSpots), 0); // The number of rows and columns in the SocialSpace (See Subsec. 2.1). When necessary, round up to the nearest integer.
    }
}
