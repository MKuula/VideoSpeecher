using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Reflection;
namespace VideoSpeecher
{
    class ViewModel : INotifyPropertyChanged
    {
        Speecher speecher;

        public System.Windows.Threading.Dispatcher Dispatcher { get; set; }
        public ObservableCollection<String> Speakers { get; set; }

        // Properties and backings
        public String ApplicationTitle
        {
            get
            {
                Assembly currentAssembly = Assembly.GetExecutingAssembly();
                AssemblyTitleAttribute titleAttr = currentAssembly.GetCustomAttribute(typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
                return titleAttr.Title;
            }
        }

        public Boolean AreControlsEnabled
        {
            get
            {
                return IsMediaLoaded && IsSubtitleLoaded;
            }
        }

        public Boolean IsMediaLoaded { get; set; }
        public Boolean IsSubtitleLoaded { get; set; }

        private String selectedSpeaker;
        public String SelectedSpeaker
        {
            get
            {
                return selectedSpeaker;
            }
            set
            {
                selectedSpeaker = value;
                speecher.SelectedSpeaker = selectedSpeaker;
            }
        }

        private int selectedSpeakerIndex;
        public int SelectedSpeakerIndex
        {
            get
            {
                return selectedSpeakerIndex;
            }
            set
            {
                selectedSpeakerIndex = value;
                OnPropertyChanged();
            }
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
                speecher.SelectedVolume = selectedVolume;
            }
        }

        private Boolean isSlowed;
        public Boolean IsSlowed
        {
            get
            {
                return isSlowed;
            }
            set
            {
                isSlowed = value;
                speecher.IsSlowed = isSlowed;
            }
        }

        private long position;
        public long Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                OnPropertyChanged();
            }
        }

        private long maximumLength;
        public long MaximumLength
        {
            get
            {
                return maximumLength;
            }
            set
            {
                maximumLength = value;
                OnPropertyChanged();
            }
        }

        private String audioFilename;
        public String AudioFilename
        {
            get
            {
                return audioFilename;
            }
            set
            {
                audioFilename = value;
                OnPropertyChanged();

            }
        }

        private String subtitleFilename;
        public String SubtitleFilename
        {
            get
            {
                return subtitleFilename;
            }
            set
            {
                subtitleFilename = value;
                OnPropertyChanged();
            }
        }

        private String currentSubtitle;
        public String CurrentSubtitle
        {
            get
            {
                return currentSubtitle;
            }
            set
            {
                currentSubtitle = value;
                OnPropertyChanged();
            }
        }

        private String previousSubtitle;
        public String PreviousSubtitle
        {
            get
            {
                return previousSubtitle;
            }
            set
            {
                previousSubtitle = value;
                OnPropertyChanged();
            }
        }

        public void LoadFiles()
        {
            IsMediaLoaded = speecher.LoadAudio(audioFilename);
            LoadSubtitle();
            OnPropertyChanged(nameof(AreControlsEnabled));
        }

        public void LoadSubtitle()
        {
            IsSubtitleLoaded = speecher.LoadSubtitle(subtitleFilename);
            CurrentSubtitle = PreviousSubtitle = "";
        }

        public void UpdatePosition(long newPos)
        {
            if (newPos > 0)
                speecher.UpdatePosition(newPos);
        }

        public ControlCmd ControlCommand { get; set; }

        public ViewModel()
        {
            speecher = new Speecher(this);
            ControlCommand = new ControlCmd(this);
            Speakers = new ObservableCollection<string>(speecher.GetSpeakers());
            SelectedSpeakerIndex = 0;
            SelectedVolume = 50;
            ControlCommand.CanExecuteChanged += new EventHandler(delegate(Object sender, EventArgs args)
            {
                if (sender is System.Windows.Controls.Button)
                    ((System.Windows.Controls.Button)sender).IsEnabled = ControlCommand.CanExecute(((System.Windows.Controls.Button)sender).CommandParameter);
            });
        }

        public class ControlCmd : ICommand
        {
            ViewModel parent;

            public ControlCmd(ViewModel parent)
            {
                this.parent = parent;
            }

            public bool CanExecute(object parameter)
            {
                return parent.IsMediaLoaded && parent.IsSubtitleLoaded;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void Execute(object parameter)
            {
                String givenCmd = parameter as String;
                switch (givenCmd)
                {
                    case "Start":
                        parent.speecher.Play();
                        break;
                    case "Pause":
                        parent.speecher.Pause();
                        break;
                    default:
                        break;
                }
            }
        }

        public void Exit()
        {
            speecher.Exit();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName]String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


}
