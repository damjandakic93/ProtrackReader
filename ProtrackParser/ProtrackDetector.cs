using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Protrack
{
    /// <summary>
    /// Utility class handling ProTrack2 device detection
    /// </summary>
    public class ProtrackDetector
    {
        /// <summary>
        /// Checks if the provided drive is a ProTrack2 device
        /// </summary>
        /// <param name="drive">The drive to be checked</param>
        /// <returns>Device serial number or null</returns>
        public static string CheckForProtrack(DriveInfo drive)
        {
            DirectoryInfo root = drive.RootDirectory;
            string statusPath = Path.Combine(root.FullName, "SETUP", "STATUS.TXT");

            if (File.Exists(statusPath))
            {
                return ParseSerial(statusPath);
            }

            return null;
        }

        /// <summary>
        /// Parses the device serial number from a given status file path
        /// </summary>
        /// <param name="path">The path to the status file</param>
        /// <returns>The device serial number or null</returns>
        private static string ParseSerial(string path)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    if (sr.ReadLine().Split(';')[0] == "ProTrack2")
                    {
                        return sr.ReadLine().Split(';')[0];
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// Detects the list of jump files from a ProTrack2 device
        /// </summary>
        /// <param name="drive">The drive object of the ProTrack2 device</param>
        /// <returns>The list of paths to the jump files</returns>
        public static List<string> DetectJumpFiles(DriveInfo drive)
        {
            FileInfo[] files = drive.RootDirectory.GetFiles("*.txt", SearchOption.TopDirectoryOnly);
            return files.Select(file => file.FullName).ToList();
        }
    }
}
