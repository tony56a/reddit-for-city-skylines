﻿using ColossalFramework;
using ICities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace RedditClient
{
    public class ModInfo : IUserMod
    {
        public string Description
        {
            get { return "Show what's new on Reddit"; }
        }

        public string Name
        {
            get { return "Reddit for Chirpy"; }
        }
    }

    public class RedditUpdater : ChirperExtensionBase
    {
        private Timer timer = new Timer();
        private Dictionary<string, string> lastPostIds = new Dictionary<string,string>();

        public override void OnCreated(IChirper threading)
        {
            try
            {
                Configuration.Load();
                if (Configuration.Subreddits.Count >= 1)
                {
                    DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("Going to show a new message from one of {0} subreddits every {1} seconds", Configuration.Subreddits.Count, Configuration.TimerInSeconds));

                    foreach (string subreddit in Configuration.Subreddits)
                        lastPostIds.Add(subreddit, null);

                    timer.AutoReset = true;
                    timer.Elapsed += new ElapsedEventHandler((sender, e) => UpdateRedditPosts());
                    timer.Interval = Configuration.TimerInSeconds * 1000;
                    timer.Start();
                }
                else
                {
                    DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "No subreddits configured.");
                }
            }
            catch(Exception e)
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, string.Format("[Reddit] {0}: {1}", e.GetType(), e.Message));
            }
        }

        public override void OnReleased()
        {
            timer.Stop();
            timer.Dispose();
        }

        private void UpdateRedditPosts()
        {
            string subreddit = Configuration.Subreddits[new Random().Next(Configuration.Subreddits.Count)];
            try
            {
                string lastPostId = lastPostIds[subreddit];

                RedditPost newestPost = TinyWeb.FindLastPost(subreddit);
                if (newestPost.id != lastPostId)
                {
                    AddMessage(new Message { senderName = string.Format("{0} on /r/{1}", newestPost.author, subreddit), text = newestPost.title });
                    lastPostIds[subreddit] = newestPost.id;
                }
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("[Reddit {0}] {1}: {2}", subreddit, e.GetType().ToString(), e.Message)); 
            }
        }

        private void AddMessage(Message m)
        {
            Singleton<ChirpPanel>.instance.AddMessage(m);
        }
    }
    public class Message : IChirperMessage
    {
        public uint senderID
        {
            get { return 0; }
        }
        public string senderName { get; set; }
        public string text { get; set; }
    }
}
