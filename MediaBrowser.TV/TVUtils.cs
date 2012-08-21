﻿using System.Linq;
using System.Text.RegularExpressions;
using MediaBrowser.Controller.IO;

namespace MediaBrowser.TV
{
    public static class TVUtils
    {
        private static readonly Regex[] seasonPathExpressions = new Regex[] {
                        new Regex(@".+\\[s|S]eason\s?(?<seasonnumber>\d{1,2})$"),
                        new Regex(@".+\\[s|S]æson\s?(?<seasonnumber>\d{1,2})$"),
                        new Regex(@".+\\[t|T]emporada\s?(?<seasonnumber>\d{1,2})$"),
                        new Regex(@".+\\[s|S]aison\s?(?<seasonnumber>\d{1,2})$"),
                        new Regex(@".+\\[s|S]taffel\s?(?<seasonnumber>\d{1,2})$"),
                        new Regex(@".+\\[s|S](?<seasonnumber>\d{1,2})$"),
                        new Regex(@".+\\[s|S]eason\s?(?<seasonnumber>\d{1,2})[^\\]*$")

        };

        /// <summary>
        /// Used to detect paths that represent episodes, need to make sure they don't also
        /// match movie titles like "2001 A Space..."
        /// Currently we limit the numbers here to 2 digits to try and avoid this
        /// </summary>
        /// <remarks>
        /// The order here is important, if the order is changed some of the later
        /// ones might incorrectly match things that higher ones would have caught.
        /// The most restrictive expressions should appear first
        /// </remarks>
        private static readonly Regex[] episodeExpressions = new Regex[] {
                        new Regex(@".*\\[s|S]?(?<seasonnumber>\d{1,2})[x|X](?<epnumber>\d{1,3})[^\\]*$"),   // 01x02 blah.avi S01x01 balh.avi
                        new Regex(@".*\\[s|S](?<seasonnumber>\d{1,2})x?[e|E](?<epnumber>\d{1,3})[^\\]*$"), // S01E02 blah.avi, S01xE01 blah.avi
                        new Regex(@".*\\(?<seriesname>[^\\]*)[s|S]?(?<seasonnumber>\d{1,2})[x|X](?<epnumber>\d{1,3})[^\\]*$"),   // 01x02 blah.avi S01x01 balh.avi
                        new Regex(@".*\\(?<seriesname>[^\\]*)[s|S](?<seasonnumber>\d{1,2})[x|X|\.]?[e|E](?<epnumber>\d{1,3})[^\\]*$") // S01E02 blah.avi, S01xE01 blah.avi
        };
        /// <summary>
        /// To avoid the following matching movies they are only valid when contained in a folder which has been matched as a being season
        /// </summary>
        private static readonly Regex[] episodeExpressionsInASeasonFolder = new Regex[] {
                        new Regex(@".*\\(?<epnumber>\d{1,2})\s?-\s?[^\\]*$"), // 01 - blah.avi, 01-blah.avi
                        new Regex(@".*\\(?<epnumber>\d{1,2})[^\d\\]*[^\\]*$"), // 01.avi, 01.blah.avi "01 - 22 blah.avi" 
                        new Regex(@".*\\(?<seasonnumber>\d)(?<epnumber>\d{1,2})[^\d\\]+[^\\]*$"), // 01.avi, 01.blah.avi
                        new Regex(@".*\\\D*\d+(?<epnumber>\d{2})") // hell0 - 101 -  hello.avi

        };

        public static bool IsSeasonFolder(string path)
        {
            path = path.ToLower();

            return seasonPathExpressions.Any(r => r.IsMatch(path));
        }

        public static bool IsSeriesFolder(string path, LazyFileInfo[] fileSystemChildren)
        {
            for (int i = 0; i < fileSystemChildren.Length; i++)
            {
                var child = fileSystemChildren[i];

                if (child.FileInfo.IsDirectory)
                {
                    if (IsSeasonFolder(child.Path))
                    {
                        return true;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(EpisodeNumberFromFile(child.Path, false)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string EpisodeNumberFromFile(string fullPath, bool isInSeason)
        {
            string fl = fullPath.ToLower();
            foreach (Regex r in episodeExpressions)
            {
                Match m = r.Match(fl);
                if (m.Success)
                    return m.Groups["epnumber"].Value;
            }
            if (isInSeason)
            {
                foreach (Regex r in episodeExpressionsInASeasonFolder)
                {
                    Match m = r.Match(fl);
                    if (m.Success)
                        return m.Groups["epnumber"].Value;
                }

            }

            return null;
        }
    }
}
