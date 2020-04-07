using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Markup;
using System.Timers;
using MessageBox = System.Windows.Forms.MessageBox;

namespace musicPlayer00
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string,string> namePath = new Dictionary<string, string>(); //Key: name of folder | Value: path of folder
        Dictionary<string, string> songPath = new Dictionary<string, string>(); //Key is combination of song name and folder name | Value : song path
        PlaylistHolder plHolder = new PlaylistHolder();

        WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
        String selectedHeader = null; //holds selected folder
        String currentlyPlaying; //holds currently playing song
        bool playing = false; //if song is playing
        bool paused = false; //if song is paused
        // State it goes through playlest
        bool repeat = false;
        bool cycle = true;



        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            string path = Directory.GetCurrentDirectory();
            player.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_ChangedState); //windows media state change function
            //player.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Playthorugh_ChangedState);
            path += @"\Saved_Data.txt";
            if (File.Exists(path)) //check for folders from previous sessions and add them to treeview
            {
                string[] text = File.ReadAllLines(path, Encoding.UTF8);
                foreach(string line in text)
                {
                    try
                    {
                        Add_Folder_View(line); //add folders form the text file to treeview
                    }
                    catch(NullReferenceException) //folder no longer exists or is in a different path
                    {
                        return;
                    }
                }
            }

            //Moved code to get songs from music directory here so it does it on start

            string[] dir = Directory.GetDirectories(@"c:\Users");  //Enters the C drive and goues staight to the User directory
            foreach (string Users in dir)   // gets the path of each item (file, Folder, directory) in the Users Directory
            {
                try
                {
                    string[] MusicDir = Directory.GetDirectories(Users + @"\Music");
                    foreach (string folders in MusicDir)    // Same thing for music directory
                    {
                        if(!namePath.ContainsValue(folders))
                            Add_Folder_View(folders); //add selected folder
                    }
                }
                catch (UnauthorizedAccessException) { } //if you eneter a directory your not allowed to.
                catch { } // All Other exceptions

            }

        }

        //changes button based on what is selected
        private void SongView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string name;
            string key;
            if (SongView.SelectedItem != null && selectedHeader != null) //if selected and song is playing
            {
                name = SongView.SelectedItem.ToString();
                key = name + selectedHeader;
            }
            else
            {
                if (currentlyPlaying != null) //if nothing is selected but song is playing, change to pause
                    PlayButton.Content = "Pause";
                return;
            }
            if(currentlyPlaying != null && !songPath[key].Equals(currentlyPlaying)) //new song selected to play
            {
                PlayButton.Content = "Play";
            }
            else if(currentlyPlaying != null && songPath[key].Equals(currentlyPlaying)) //moved back to current song to pause
            {
                PlayButton.Content = "Pause";
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                String name = SongView.SelectedItem.ToString();
                String key = name + selectedHeader;
                Slider.Maximum = Song_Duration() + 1; // Sets song length to slider max value
                TxtSliderMaxValue.Text = Song_Duration().ToString(); // shows song length at right of slider
                if ((!playing && !paused) || (SongView.SelectedItem != null && !songPath[key].Equals(currentlyPlaying))) //play new song
                {
                    Console.WriteLine("New Song");
                    String song = songPath[key];
                    currentlyPlaying = song; //set currently playing song to song path
                    player.URL = song;
                    Play_Seek();
                    player.controls.play();
                }
                else if (!playing && paused) //continue playing
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
            catch (NullReferenceException) //no song selected
            {
                Console.WriteLine("No Song Selected");
                if (currentlyPlaying != null && playing) //if song is currently playing it will pause
                    player.controls.pause();
                else if (currentlyPlaying != null && !playing) //if song currently paused it will play
                    player.controls.play();
                return;
            }
        }

        // write in console what second you are in the song only works when playing
        private void Play_Seek()
        {
             Console.WriteLine("time {0} sec", (int)player.controls.currentPosition);
            
        }

        void Player_ChangedState(int state) //Actions when media player changes state
        {
            if(state == (int)WMPLib.WMPPlayState.wmppsMediaEnded) //when media player ends song
            {
                paused = false;
                playing = false;
                currentlyPlaying = null;
                player.controls.currentPosition = 0;
                PlayButton.Content = "Play";
                Play_Click(new object(), new RoutedEventArgs());
            }
            else if(state == (int)WMPLib.WMPPlayState.wmppsPaused) //when media player is paused
            {
                paused = true;
                playing = false;
                PlayButton.Content = "Play";
            } else if(state == (int)WMPLib.WMPPlayState.wmppsPlaying) //when media player is playing
            {
                playing = true;
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
            
             FolderBrowserDialog fbd  = new FolderBrowserDialog(); //to view folders
             if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
             {
                 Add_Folder_View(fbd.SelectedPath); //add selected folder
             }
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {

        }

        //add folder to treeview
        private void Add_Folder_View(String fn)
        {
            String tmp = getFileName(fn); //convert path to just name of folder
            if (tmp == "")
                tmp = fn;
            for(int i = 0; namePath.ContainsKey(tmp) != false; i++) //if there are files with the same name this will append number
            {
                tmp += i.ToString();
            }
            namePath.Add(tmp, fn); //tmp = name that shows up in view, fn = path
            TreeViewItem tmpTVI = new TreeViewItem { Header = tmp }; //make treeviewitem with tmp as header
            folderDisplay.Items.Add(tmpTVI);
        }

        // Delete selected folder from tree view
        private void Delete(object sender, RoutedEventArgs e)   //delete folder
        {
            folderDisplay.Items.Remove(folderDisplay.SelectedItem); // Removes selected folder from TreeView
            try
            {
                namePath.Remove(selectedHeader);
            }
            catch (Exception) { }
        }
        // Delete folders that no longer exist when app starts up
        private void Remove_Folder_View(String fn)
        {
            folderDisplay.Items.Remove(fn); //remove folder from view
            namePath.Remove(selectedHeader); //remove deleted file from dictionary
        }

        private void TreeViewClick()
        {
            //TreeViewItem.
        }

        //shows contents of selected folder
        private void Selected_Folder(object sender, RoutedEventArgs e)
        {
            SongView.Items.Clear(); //clear current songview
            var item = folderDisplay.SelectedItem as TreeViewItem; //current folder
            try
            {
                selectedHeader = item.Header.ToString();
                String path;
                if (namePath.ContainsKey(selectedHeader)) //so it doesn't error when the default files are clicked
                    path = namePath[selectedHeader];
                else return;
                var songs = Directory.EnumerateFiles(path).Where(s => s.EndsWith(".mp3") || s.EndsWith(".wav")); //only select mp3 files can be modified to add mp4 or other types easily
                foreach (String song in songs)
                {
                    string tmp = getFileName(song).Replace(".mp3","").Replace(".wav",""); //convert path to file name and remove .mp3 from end
                    string key = tmp+selectedHeader; //key value pair for dictionary (folder & song path)
                    if (songPath.ContainsKey(key) && songPath[key].Equals(song))
                    {
                        SongView.Items.Add(tmp);
                        continue;
                    }
                    else if (songPath.ContainsKey(key) && !songPath[key].Equals(song))
                        songPath.Remove(key);
                    songPath.Add(key, song); //key = name of song, value (song) = path of song
                    SongView.Items.Add(tmp); //add each song to songview
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

        /*
         Add song to playlist it will take mp3 files dragged into the listview and then add them
         currently if there is another song with the same name it won't overwrite it
         (can take multiple files dragged in at once)
         */
        private void Add_To_Playlist(object sender, System.Windows.DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop, false); //get dragged files
            var songs = files.Where(s => s.EndsWith(".mp3") || s.EndsWith(".wav")); //select only music files
            string currentPath = namePath[selectedHeader]; //gets current path
            foreach (string song in songs)
            {
               try
                {
                File.Copy(song, currentPath + @"\" + getFileName(song), false);
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
            using (StreamWriter file = new StreamWriter("Saved_Data.txt"))
                foreach (var folder in namePath)
                    file.WriteLine("{0}", folder.Value);
        }

        /*
         *Peripheral METHODS 
         */

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
            String key = SongView.SelectedItem.ToString() + selectedHeader;
            string currentDirName = @"" + songPath[key];
                     
            System.IO.FileInfo fi = null;   // Completely Different from String
            try
            {
                fi = new System.IO.FileInfo(songPath[key].ToString());
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
            TxtSliderValue.Text = Slider.Value.ToString();
            player.controls.currentPosition = Slider.Value;
        }

    }
}
