# Video Speecher
Video Speecher turns video subtitles into dubbings. In other words, it converts subtitles into speech without the need to watch the movie. The speech is played over the sound track, lowering its volume automatically while speaking. The video itself is not played, only audio. You could imagine Video Speecher as a way to convert a television film into a radio presentation.

## Getting started
At first, the movie need to be converted into audio and subtitles (video is not needed). **Audio is expected to be in MP3 and subtitles in SRT format.** Double click the audio filename field to select the audio file. After that, the corresponding subtitle file should be preselected, too.

You can jump to a desired position by moving the slider left or right.

The volume slider changes the movie audio volume, the speech volume is fixed.

![image](https://github.com/MKuula/VideoSpeecher/assets/168563015/a26c0095-6206-4ded-9ae4-b64a6b6edfcb)

After you have started playing, the main window minimizes and only the lower part remains visible showing the subtitles. Current subtitle is shown below the previous one.

## Setting the speaker
Chosen speaker can be changed at the bottom and available speakers (voices) are controlled in Speech section of Control Panel.
 
![image](https://github.com/MKuula/VideoSpeecher/assets/168563015/3d6ec640-73da-4eaf-bbf7-91b3f1f9e8de)

Additional voices can be installed, but they need to be for the SAPI speech engine.

https://www.zero2000.com/free-text-to-speech-natural-voices.html

## Future
This version has some known limitations and things to be implemented, like
- only minimal checks for file handling etc. (tends to crash)
- support for net-based speech engines
- localized version

> [!NOTE]
> Current version here is written in Finnish. The English version can be downloaded separately as the file [vspeecher_en.exe](http://www.msoftwares.net/files/vspeecher_en.exe) or you can tweak the MainWindow.xaml by yourself.
