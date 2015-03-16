﻿using ColossalFramework.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RedditClient
{
    class Configuration
    {
        private const string TIMER_KEY = "updateFrequency";
        private const string NOSOUND_KEY = "noSound";

        public static List<string> Subreddits;
        public static int TimerInSeconds = 300;
        public static int NoSound = 0;

        private static string ConfigPath
        {
            get
            {
                // base it on the path Cities: Skylines uses
                string path = string.Format("{0}/{1}/", DataLocation.localApplicationData, "ModConfig");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path += "reddit-for-chirpy.txt";
                
                return path;
            }
        }

        /// <summary>
        /// Attempts to load the configuration. If it fails, it'll load the defaults
        /// </summary>
        internal static void Load()
        {
            try
            {
                string[] configLines = File.ReadAllLines(ConfigPath);
                Regex r = new Regex("^[a-zA-Z0-9/]*$");

                Subreddits = new List<string>();
                bool requestSave = false;

                for (int i = 0; i < configLines.Length; ++ i)
                {
                    // Remove unnecessary spaces
                    var line = configLines[i].Trim();

                    // Comment lines
                    if (line.StartsWith("#") || line.StartsWith(";"))
                        continue;

                    // Config options
                    if (line.StartsWith(TIMER_KEY + "="))
                    {
                        var time = line.Substring(TIMER_KEY.Length + 1);

                        int newTimer = -1;
                        if (Int32.TryParse(time, out newTimer) && newTimer >= 10)
                        {
                            TimerInSeconds = newTimer;
                        }
                    }
                    else if (line.StartsWith(NOSOUND_KEY + "="))
                    {
                        var time = line.Substring(NOSOUND_KEY.Length + 1);

                        int newSound = -1;
                        if (Int32.TryParse(time, out newSound) && (newSound >= 0 || newSound <= 2))
                        {
                            NoSound = newSound;
                        }
                    }

                    // Just reddit names, presumably
                    else if (line.Length > 1 && r.IsMatch(line))
                    {
                        if (line.IndexOf('/') == -1)
                        {
                            line = string.Format("/r/{0}/new", line);
                            configLines[i] = line;

                            requestSave = true;
                        }
                        Subreddits.Add(line);
                    }
                }

                if (requestSave)
                {
                    using (StreamWriter sw = new StreamWriter(ConfigPath))
                    {
                        foreach (string line in configLines)
                            sw.WriteLine(line);
                    }
                }
            }
            catch (Exception e)
            {
                TimerInSeconds = 300;
                NoSound = 0;
                Subreddits = DefaultSubreddits;

                using (StreamWriter sw = new StreamWriter(ConfigPath))
                {
                    sw.WriteLine("# Reddit for Chirpy");
                    sw.WriteLine();

                    sw.WriteLine("# How often should new messages be displayed?");
                    sw.WriteLine("# Default: 300 (which is 5 minutes)");
                    sw.WriteLine("{0}={1}", TIMER_KEY, TimerInSeconds);
                    sw.WriteLine();

                    sw.WriteLine("# Set this to 1 to disable chirping sounds.");
                    sw.WriteLine("#   Breaks compatibility with non-default chirper panels (like the marquee one).");
                    sw.WriteLine("#   Set to 2 to force the popup to open after each new message, 1 will leave it collapsed.");
                    sw.WriteLine("{0}={1}", NOSOUND_KEY, NoSound);

                    sw.WriteLine();
                    sw.WriteLine("# One subreddit per line");

                    foreach (string subreddit in Subreddits)
                        sw.WriteLine("{0}", subreddit);

                    sw.WriteLine();
                    sw.WriteLine("# (Info) Your config file was regenerated because of {0}: {1}", e.GetType().ToString(), e.Message);
                }
            }
        }

        private static void SaveConfig()
        {
            
        }
        
        private static List<string> DefaultSubreddits
        {
            get
            {
                var s = new List<string>();
                s.Add("/r/ShowerThoughts/rising/");
                s.Add("/r/CrazyIdeas/new/");
                s.Add("/r/ChirpIt/new/");
                return s;
            }
        }
    }
}
