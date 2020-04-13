using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Threading;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Runtime.Serialization.Formatters.Binary;

namespace musicPlayer00
{
    public partial class MainWindow : Window
    {

        PlaylistHolder plHolder = new PlaylistHolder();
        WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
        Playlist selectedHeader = null; //holds selected folder
        Song currentlyPlaying; //holds currently playing song
        bool paused = false; //if song is paused
        // State it goes through playlest
        bool repeat = false;
        bool cycle = true;



        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            player.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_ChangedState); //windows media state change function
            if (File.Exists("PlHolderData.dat"))
            {
                Stream stream = File.Open("PlHolderData.dat", FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                plHolder = (PlaylistHolder)bf.Deserialize(stream);
                foreach(Playlist pl in plHolder.getPlaylists())
                {
                    Add_Folder_View(pl);
                }
            }

            string myMusicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic); //using Environment instead of hard-code
            try
            {
                string[] MusicDir = Directory.GetDirectories(myMusicPath);
                foreach (string folders in MusicDir) 
                {
                    Playlist pl = new Playlist(getFileName(folders), folders, plHolder.getMaxPosition());
                    if (!plHolder.containsPlaylist(pl))
                        Add_Folder_View(pl); //add selected folder
                }
            }
            catch (UnauthorizedAccessException) { } //if you enter a directory your not allowed to.
            catch { } // All Other exceptions

            // Threading a clock for the timer & slider
            DispatcherTimer dTimer = new DispatcherTimer();
            dTimer.Interval = new TimeSpan(0, 0, 1);
            dTimer.Tick += dTimer_sec;
            dTimer.Start();
        }

        // What happens every second (Threading timer)
        private void dTimer_sec(Object sender, EventArgs e)
        {
            if ((paused == false))
            {
                Slider.Value = (int)player.controls.currentPosition;
            }
        }

        //changes button based on what is selected
        private void SongView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Song song;
            if (SongView.SelectedItem != null && selectedHeader != null) //if selected and song is playing
            {
                System.Windows.Controls.ListViewItem lvi = SongView.SelectedItem as System.Windows.Controls.ListViewItem;
                song = lvi.Tag as Song;
            }
            else
            {
                if (currentlyPlaying != null) //if nothing is selected but song is playing, change to pause
                    PlayButton.Content = "Pause";
                return;
            }
            if(currentlyPlaying != null && !(song == currentlyPlaying)) //new song selected to play
            {
                PlayButton.Content = "Play";
            }
            else if(currentlyPlaying != null && song == currentlyPlaying) //moved back to current song to pause
            {
                PlayButton.Content = "Pause";
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Playlist pl = (Playlist)folderDisplay.Tag;
                ListViewItem lvi = SongView.SelectedItem as ListViewItem;
                Song song = lvi.Tag as Song;
                Play(pl, song);
            }

            catch (NullReferenceException) //no song selected
            {
                Console.WriteLine("No Song Selected");
                if (currentlyPlaying != null && !paused) //if song is currently playing it will pause
                    player.controls.pause();
                else if (currentlyPlaying != null && paused) //if song currently paused it will play
                    player.controls.play();
                return;
            }
        }
        private void Play(Playlist pl, Song song)
        {
            if (!(song == currentlyPlaying)) //play new song
            {
                currentlyPlaying = song;
                Slider.Maximum = Song_Duration(); // Sets song length to slider max value
                TxtSliderMaxValue.Content = Song_Duration().ToString(); // shows song length at right of slider
                Console.WriteLine("New Song");
                player.URL = song.getPath();
                Play_Seek();
                player.controls.play();
            }
            if (paused) //continue playing
            {
                Console.WriteLine("Continue playing");
                player.controls.play();
                Play_Seek();
            }
            else
            {
                Console.WriteLine("Paused");
                Play_Seek();
                player.controls.pause();
            }
        }

        // write in console what second you are in the song only works when playing
        private void Play_Seek()
        {
             Console.WriteLine("time {0} sec", (int)player.controls.currentPosition);
            
        }

        void Player_ChangedState(int state) //Actions when media player changes state
        {
            if (state == (int)WMPLib.WMPPlayState.wmppsMediaEnded) //when media player ends song
            {
                paused = false;
                currentlyPlaying = null;
                player.controls.currentPosition = 0;
                PlayButton.Content = "Play";
                Play_Click(new object(), new RoutedEventArgs());
            }
            else if (state == (int)WMPLib.WMPPlayState.wmppsPaused) //when media player is paused
            {
                paused = true;
                PlayButton.Content = "Play";
            }
            else if (state == (int)WMPLib.WMPPlayState.wmppsPlaying) //when media player is playing
            {
                paused = false;
                PlayButton.Content = "Pause";
            }

        }

        //add folder to TreeView
        /*
         *I moved the code to grab the music files to the
         * function that runs on start so it will automatically
         * grab all folders from music
         */
        private void Add_Folder(object sender, RoutedEventArgs e)
        {
            
             System.Windows.Forms.FolderBrowserDialog fbd  = new System.Windows.Forms.FolderBrowserDialog(); //to view folders
             if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
             {
                Playlist pl = new Playlist(getFileName(fbd.SelectedPath), fbd.SelectedPath, plHolder.getMaxPosition());
                if (!plHolder.containsPlaylist(pl))
                {
                    plHolder.addPlaylist(pl); //add selected folder
                    Add_Folder_View(pl);
                }
             }
        }

        //add folder to treeview
        private void Add_Folder_View(Playlist pl)
        {
            TreeViewItem tmpTVI = new TreeViewItem{Header = pl.getName(), Tag = pl}; //make treeviewitem with tmp as header
            folderDisplay.Items.Add(tmpTVI);
        }

        // Delete selected folder from tree view
        // Delete selected folder from tree view
        private void Delete(object sender, RoutedEventArgs e)   //delete folder
        {
            TreeViewItem tvi = folderDisplay.SelectedItem as TreeViewItem;
            folderDisplay.Items.Remove(folderDisplay.SelectedItem); // Removes selected folder from TreeView
            SongView.Items.Clear();
            try
            {
                plHolder.removePlaylist(tvi.Tag as Playlist);
            }
            catch (Exception) { }
        }

        // Delete folders that no longer exist when app starts up
        private void Remove_Folder_View(Playlist fn)
        {
            folderDisplay.Items.Remove(fn); //remove folder from view
            plHolder.removePlaylist(fn); //remove deleted file from dictionary
        }

        //shows contents of selected folder
        private void Selected_Folder(object sender, RoutedEventArgs e)
        {
            SongView.Items.Clear(); //clear current songview
            TreeViewItem item = folderDisplay.SelectedItem as TreeViewItem;
            try
            {
                selectedHeader = (Playlist)item.Tag; //current folder
                Playlist pl;
                pl = selectedHeader;
                var songs = Directory.EnumerateFiles(pl.getPath()).Where(s => s.EndsWith(".mp3") || s.EndsWith(".wav")); //only select mp3 files can be modified to add mp4 or other types easily
                foreach (String song in songs)
                {
                    string tmp = getFileName(song).Replace(".mp3","").Replace(".wav",""); //convert path to file name and remove .mp3 from end
                    Song tmpSong = new Song(pl, tmp, pl.getMaxPosition() + 1, song);
                    if (!pl.alreadyHasSong(tmpSong))
                    {
                        pl.addSong(tmpSong);
                    }
                }
                pl.sortSongs();
                    foreach (Song song in pl.getSongs())
                    {
                        try
                        {
                            if (File.Exists(song.getPath()))
                            {
                                System.Windows.Controls.ListViewItem lvi = new System.Windows.Controls.ListViewItem { Content = song.getName(), Tag = song };
                                SongView.Items.Add(lvi);
                            }
                        } catch (DirectoryNotFoundException ex) { //If song moved out of folder
                            Console.WriteLine(ex);
                            pl.removeSong(song); 
                        }
                }
            }
            catch (DirectoryNotFoundException) //if directory deleted remove folder from treeview
            {
                Remove_Folder_View(selectedHeader); //remove file from view
                return;
            }
            catch (NullReferenceException)
            {
                return;
            }
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            Shuffle_Songs();
        }

        private void Shuffle_Songs()
        {
            try
            {
                Playlist randPL = plHolder.getRandomPlaylist();
                Song randSong = randPL.getRandomSong();
                selectedHeader = randPL;
                SongView.SelectedItem = null;
                paused = false;
                currentlyPlaying = null;
                Play(randPL, randSong);
            }
            catch (Exception)
            {
                MessageBox.Show("There are no songs to shuffle!", "Error");
            }
        }

        /*
         Add song to playlist it will take mp3 files dragged into the listview and then add them
         currently if there is another song with the same name it won't overwrite it
         (can take multiple files dragged in at once)
         */
        private void Add_To_Playlist(object sender, System.Windows.DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop, false); //get dragged files
            var songs = files.Where(s => s.EndsWith(".mp3") || s.EndsWith(".wav")); //select only music files
            Playlist pl = selectedHeader;
            string currentPath = selectedHeader.getPath(); //gets current path
            foreach (string song in songs)
            {
               try
                {
                    File.Copy(song, currentPath + @"\" + getFileName(song), false);
                    Song songToAdd = new Song(pl, getFileName(song), pl.getMaxPosition() + 1, currentPath + @"\" + getFileName(song));
                }
               catch (System.IO.IOException)
               {
                return;
               }
                Selected_Folder(folderDisplay.SelectedItem, e); //refresh view
            }
        }

        //Saves current folders
        private void On_Close(object sender, EventArgs e)
        {
            Stream stream = File.Create("PlHolderData.dat");
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(stream, plHolder);
        }

         //reverse string
        private String reverse(String str)
        {
            Char[] chrarr = str.ToCharArray();
            for (int i = 0; i < (int)(str.Length / 2); i++)
            {
                char tmp = chrarr[i];
                chrarr[i] = chrarr[str.Length - i - 1];
                chrarr[str.Length - i - 1] = tmp;
            }
            return new String(chrarr);
        }

        private String getFileName(String fn) //convert path to just name of folder
        {
            String tmp = "";
            for (int i = fn.Length - 1; i > 0; i--) //make it just file name rather than entire path name
            {
                if (fn[i] == '\\') break;
                tmp += fn[i];
            }
            return reverse(tmp);
        }

        /*
         * Currently doesnt seek but I set it up so that every time you click the play button
         * it will get the song length and print it in the output.
         */
        private int Song_Duration()
        {
            int SongLength = 0;
            Playlist pl = selectedHeader;
            Song key = currentlyPlaying;

            string currentDirName = @"" + key.getPath();

            System.IO.FileInfo fi = null;   // Completely Different from String
            try
            {
                fi = new System.IO.FileInfo(currentDirName);
            }
            catch
            { Console.WriteLine("Failed to get song Legnth"); }

            //Console.WriteLine("{0} : {1}", fi.Length, fi.Directory);  //length and name of file

            TagLib.File l = TagLib.File.Create(fi.ToString());//create taglib file
            SongLength = (int)l.Properties.Duration.TotalSeconds;//get the song length in seconds
            //Console.WriteLine("double {0}", l.Properties.Duration.TotalSeconds);
            return SongLength;
        }

        private void Play_Prev(object sender, RoutedEventArgs e)//play the previous song in the current playlist
        {
            if (SongView.SelectedItem == null)
            {
                return;
            }
            if (SongView.SelectedItem != SongView.Items[0]) // Current song is not the first in the ItemList
            {
                SongView.SelectedItem = SongView.Items[SongView.SelectedIndex - 1]; // goes to the previous song in ItemList
                Play_Click(sender, e);
            }
            else
            {
                SongView.SelectedItem = SongView.Items[SongView.Items.Count - 1];   // Goes to last song in ItemList
                Play_Click(sender, e);
            }
            
        }

        private void Play_Next(object sender, RoutedEventArgs e)//play the next song in the current playlist
        {
            if(SongView.SelectedItem == null)
            {
                return;
            }
            if (SongView.SelectedItem != SongView.Items[SongView.Items.Count - 1])  // current song is not the last song in the ItemList
            {
                SongView.SelectedItem = SongView.Items[SongView.SelectedIndex + 1]; // Goes to next song in ItemList
                Play_Click(sender, e);
            }
            else
            {
                SongView.SelectedItem = SongView.Items[0];  //Goes to the beginning of the ItemList
                Play_Click(sender, e);
            }

        }

        // Half seek, it doesn't move the slider every second but if you move the slider it will go to that second of the song.
        private void Slider_Value_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            //TimeSpan min = TimeSpan.FromMinutes(Slider.Value);
            TxtSliderValue.Content = convertToString(Slider.Value);
            if ((int)player.controls.currentPosition != Slider.Value)
            {
                player.controls.currentPosition = Slider.Value;
            }
            //player.controls.currentPosition = Slider.Value;
        }
        public string convertToString(double timeInSec)
        {
            int sec = (int)timeInSec % 60;
            int min = (int)Math.Floor(timeInSec / 60);
            if (sec >= 10)
            {
                return min.ToString() + ":" + sec.ToString();
            }
            else
            {
                return min.ToString() + ":0" + sec.ToString();
            }

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

   

   
}
