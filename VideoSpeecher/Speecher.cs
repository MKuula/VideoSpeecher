using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Threading;
using System.Threading.Tasks;

namespace VideoSpeecher
{
    class Speecher
    {
        const uint TIMER_INTERVAL = 500;

        ViewModel viewModel;

        MediaPlayer player;
        System.Speech.Synthesis.SpeechSynthesizer synth;
        Timer timer;

        Dictionary<long, String> subtitles;
        Dictionary<long, String> remainingSubtitles;

        long previousSentenceTime = 0;

        public String SelectedSpeaker
        {
            get;
            set;
        }

        private int selectedVolume;
        public int SelectedVolume
        {
            get
            {
                return selectedVolume;
            }
            set
            {
                selectedVolume = value;
                player.Volume = selectedVolume * 0.01;
            }
        }

        public Boolean IsSlowed { get; set; }

        public void UpdatePosition(long newPos)
        {
            Pause();
            player.Position = new TimeSpan(newPos);
            viewModel.Position = newPos;
            remainingSubtitles = new Dictionary<long, string>(subtitles);

            for (int i = remainingSubtitles.Count - 1; i >= 0; i--)
            {
                if (remainingSubtitles.ElementAt<KeyValuePair<long, String>>(i).Key < newPos)
                    remainingSubtitles.Remove(remainingSubtitles.ElementAt<KeyValuePair<long, String>>(i).Key);
            }
            Play();
        }

        public Speecher(ViewModel viewModel)
        {
            this.viewModel = viewModel;

            synth = new System.Speech.Synthesis.SpeechSynthesizer();
            player = new MediaPlayer();
            subtitles = new Dictionary<long, string>();
        }

        public List<String> GetSpeakers()
        {
            List<string> voices = new List<string>(synth.GetInstalledVoices().Select<System.Speech.Synthesis.InstalledVoice, String>(i => i.VoiceInfo.Name).ToList<String>());
            voices.Insert(0, "<Ei puhujaa>");
            return voices;
        }

        public Boolean LoadAudio(String fileName)
        {
            if (File.Exists(fileName))
            {
                player.MediaOpened += delegate(Object sender, EventArgs args)
                {
                    viewModel.MaximumLength = player.NaturalDuration.TimeSpan.Ticks;

                    timer = new Timer(state =>
                    {
                        if (!viewModel.Dispatcher.HasShutdownStarted)   // Timer being disposed?
                        {
                            viewModel.Dispatcher.Invoke(() =>
                            {
                                long position = player.Position.Ticks;
                                viewModel.Position = position;

                                // Have we reached the subtitle start?
                                if (remainingSubtitles.Count > 0)
                                {
                                    KeyValuePair<long, String> subtitle = remainingSubtitles.First();

                                    if (subtitle.Key < position)
                                    {
                                        String sentence = subtitle.Value;   // Subtitle content

                                        previousSentenceTime = position;
                                        viewModel.PreviousSubtitle = viewModel.CurrentSubtitle;
                                        viewModel.CurrentSubtitle = sentence;
                                        Speak(sentence);

                                        remainingSubtitles.Remove(subtitle.Key);    // Subtitle handled so get rid of it
                                    }
                                }

                                // Remove current and previous subtitles after a brief delay
                                if (previousSentenceTime + TimeSpan.FromSeconds(5).Ticks < position)
                                {
                                    if (!String.IsNullOrEmpty(viewModel.CurrentSubtitle))
                                    {
                                        viewModel.PreviousSubtitle = viewModel.CurrentSubtitle;
                                        viewModel.CurrentSubtitle = ""; 
                                    }
                                }

                                if (previousSentenceTime + TimeSpan.FromSeconds(10).Ticks < position)
                                    viewModel.PreviousSubtitle = "";
                            });
                        }

                    });  // Start the timer while playing
                };
                player.Open(new Uri("file://" + fileName));
                return true;
            }
            return false;
        }

        public Boolean LoadSubtitle(String filename)
        {
            if (!File.Exists(filename))
            {
                filename = filename.Replace(".fin.srt", ".srt");
                viewModel.SubtitleFilename = filename;
            }

            if (File.Exists(filename))
            {
                String[] lines = File.ReadAllLines(filename);
                subtitles.Clear();

                long lastTickValue = 0;

                foreach (String line in lines)
                {
                    if (line.Contains("-->"))   // Beginning and ending time (last ignored)
                    {
                        if (line.IndexOf(' ') != -1)
                        {
                            lastTickValue = TimeSpan.Parse(line.Substring(0, line.IndexOf(' '))).Ticks;
                            subtitles.Add(lastTickValue, "");
                        }
                    }
                    else if (String.IsNullOrEmpty(line) || System.Text.RegularExpressions.Regex.IsMatch(line, @"^\d"))  // Empty line or sequence number -> ignore
                        continue;
                    else
                    {
                        subtitles[lastTickValue] += " " + line; // Actual subtitle string
                    }
                }
                remainingSubtitles = new Dictionary<long, string>(subtitles);
                return true;
            }
            return false;
        }

        public void Play()
        {
            player.Play();
            if (timer != null && remainingSubtitles != null) timer.Change(0, 500);
        }

        public void Pause()
        {
            player.Pause();
            if (timer != null) timer.Change(Timeout.Infinite, 0);
        }


        public void Speak(String sentence)
        {
            if (!SelectedSpeaker.Contains("<"))
            {
                synth.Volume = 75;
                synth.Rate = IsSlowed ? -5 : 1;
                synth.SelectVoice(SelectedSpeaker);
                player.Volume = 0.05;

                // Speak under a different thread
                System.Threading.Thread th = new Thread(new ThreadStart(delegate()
                {
                    if (!viewModel.Dispatcher.HasShutdownStarted)
                    {
                        synth.Speak(sentence);

                        if (!viewModel.Dispatcher.HasShutdownStarted)
                            viewModel.Dispatcher.Invoke(() => player.Volume = SelectedVolume * 0.01);
                    }
                }));
                th.Start();
            }
        }

        public void Exit()
        {
            Pause();
            if (timer != null) timer.Dispose();
        }
    }
}
