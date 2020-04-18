using System;
using System.Collections.Generic;

namespace Protrack
{
    /// <summary>
    /// Represents a single jump data
    /// </summary>
    [Serializable]
    public class JumpData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public JumpData()
        {
            Profile = new List<double>();
        }

        /// <summary>
        /// The jump number
        /// </summary>
        public int JumpNumber { get; set; }

        /// <summary>
        /// The jump date
        /// </summary>
        public DateTime JumpDate { get; set; }

        /// <summary>
        /// Exit altitude
        /// </summary>
        public int ExitAltitude { get; set; }

        /// <summary>
        /// Deployment altitude
        /// </summary>
        public int DeploymentAltitude { get; set; }

        /// <summary>
        /// Freefall time
        /// </summary>
        public int FreefallTime { get; set; }

        /// <summary>
        /// Average speed
        /// </summary>
        public int AverageSpeed { get; set; }

        /// <summary>
        /// Max speed
        /// </summary>
        public int MaxSpeed { get; set; }

        /// <summary>
        /// First half speed
        /// </summary>
        public int FirstHalfSpeed { get; set; }

        /// <summary>
        /// Second half speed
        /// </summary>
        public int SecondHalfSpeed { get; set; }

        /// <summary>
        /// Jump altitude profile
        /// </summary>
        public List<double> Profile { get; set; }
    }
}
