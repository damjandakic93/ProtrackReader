using System;
using System.Collections.Generic;

namespace Protrack
{
    /// <summary>
    /// Represents a single ProTrack2 device
    /// </summary>
    [Serializable]
    public class DeviceData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serialNumber">The device serial number</param>
        public DeviceData(string serialNumber)
        {
            SerialNumber = serialNumber;
            Jumps = new List<JumpData>();
        }

        /// <summary>
        /// The device serial number
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// A list of jumps acquired by this device
        /// </summary>
        public List<JumpData> Jumps { get; set; }
    }
}
