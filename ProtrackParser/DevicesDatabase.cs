using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Protrack
{
    /// <summary>
    /// Database containing data from multiple devices
    /// </summary>
    [Serializable]
    public class DevicesDatabase
    {
        /// <summary>
        /// The file name used for the database serialization
        /// </summary>
        private const string c_fileName = "database.bin";

        /// <summary>
        /// Attempts to deserialize the database
        /// </summary>
        /// <returns>Deserialized database or null</returns>
        public static DevicesDatabase FromFile()
        {
            try
            {
                if (System.IO.File.Exists("database.bin"))
                {
                    return BinarySerialization.ReadFromBinaryFile<DevicesDatabase>(c_fileName);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DevicesDatabase()
        {
            Devices = new List<DeviceData>();
        }

        /// <summary>
        /// A collection of devices data
        /// </summary>
        public List<DeviceData> Devices { get; set; }

        /// <summary>
        /// Serializes the database
        /// </summary>
        public void Save()
        {
            BinarySerialization.WriteToBinaryFile(c_fileName, this);
        }

        /// <summary>
        /// Reads jump data from a given path, adds it to the database and
        /// serializes it
        /// </summary>
        /// <param name="path">The path to the jump file</param>
        public void ReadJumpData(string path)
        {
            try
            {
                List<string> lines = new List<string>();
                using (StreamReader sr = new StreamReader(path))
                {
                    lines.Add(sr.ReadLine());
                    if (lines[0] != "JIB")
                    {
                        return;
                    }

                    while (!sr.EndOfStream)
                    {
                        lines.Add(sr.ReadLine());
                    }
                }

                string deviceSerial = lines[4];
                DeviceData device;

                if (Devices.Any(d => d.SerialNumber == deviceSerial))
                {
                    device = Devices.First(d => d.SerialNumber == deviceSerial);
                }
                else
                {
                    device = new DeviceData(deviceSerial);
                    Devices.Add(device);
                }

                JumpData jump = new JumpData();
                jump.JumpNumber = Convert.ToInt32(lines[5]);

                if (device.Jumps.Any(j => j.JumpNumber == jump.JumpNumber))
                {
                    return;
                }

                jump.JumpDate = DateTime.ParseExact(lines[6] + lines[7], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                jump.ExitAltitude = Convert.ToInt32(lines[8]);
                jump.DeploymentAltitude = Convert.ToInt32(lines[9]);
                jump.FreefallTime = Convert.ToInt32(lines[10]);
                jump.AverageSpeed = Convert.ToInt32(lines[11]);
                jump.MaxSpeed = Convert.ToInt32(lines[12]);
                jump.FirstHalfSpeed = Convert.ToInt32(lines[14]);
                jump.SecondHalfSpeed = Convert.ToInt32(lines[15]);
                jump.Profile = new List<double>();

                int groundLevel = Convert.ToInt32(lines[35]);
                int i = 39;
                while (lines[i] != "PIE")
                {
                    jump.Profile.AddRange(lines[i].Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => Convert.ToInt32(x)).Select(x => PressureToAltitude(x, groundLevel)));
                    i++;
                }

                device.Jumps.Add(jump);

                Save();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Converts temperature-compensated pressure difference to altitude difference
        /// </summary>
        /// <param name="pressure">The pressure at target altitude</param>
        /// <param name="groundLevel">The pressure at the ground level</param>
        /// <returns>Altitude difference in meters</returns>
        /// <remarks>https://www.nxp.com/docs/en/data-sheet/MPL3115A2.pdf section 9.1.3.
        /// I don't claim this component is used in Protrack 2 but the formula seems to work and is
        /// common across different temperature-compensated pressure-based altimeters.</remarks>
        private static double PressureToAltitude(int pressure, int groundLevel)
        {
            return 44330.77 * (1 - Math.Pow(pressure / (double)groundLevel, 0.1902632));
        }
    }
}
