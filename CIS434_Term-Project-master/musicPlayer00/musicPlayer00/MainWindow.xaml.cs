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

namespace musicPlayer00
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string,string> namePath = new Dictionary<string, string>(); //Key: name of folder | Value: path of folder
        Dictionary<string, string> songPath = new Dictionary<string, string>(); //Key is combination of song name and folder name | Value : song path
        WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
        String selectedHeader = null; //holds selected folder
        String currentlyPlaying; //holds currently playing song
        bool playing = false; //if song is playing
        bool paused = false; //if song is paused
        double CurrentTime = 10;
        
        
        
        public MainWindow()
        {
            InitializeComponent();
            string path = Directory.GetCurrentDirectory();
            player.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_ChangedState); //windows media state change function
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
            
        }
        public void OpenFolder() //refactor as needed
        {
        }

        public void LoadMusic() //refactor as needed
        {

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
                if ((!playing && !paused) || (SongView.SelectedItem != null && !songPath[key].Equals(currentlyPlaying))) //play new song
                {
                    String song = songPath[key];
                    currentlyPlaying = song; //set currently playing song to song path
                    player.URL = song;
                    player.controls.play();
                } else if (!playing && paused) //continue playing
                {
                    player.controls.play();
                }
                else
                {
                    player.controls.pause();
                }
            }
            catch (NullReferenceException) //no song selected
            {
                if (currentlyPlaying != null && playing) //if song is currently playing it will pause
                    player.controls.pause();
                else if (currentlyPlaying != null && !playing) //if song currently paused it will play
                    player.controls.play();
                return;
            }
        }

        void Player_ChangedState(int state) //Actions when media player changes state
        {
            if(state == (int)WMPLib.WMPPlayState.wmppsMediaEnded) //when media player ends song
            {
                paused = false;
                playing = false;
                currentlyPlaying = null;
                PlayButton.Content = "Play";
            }else if(state == (int)WMPLib.WMPPlayState.wmppsPaused) //when media player is paused
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

        private double Slider_Control(Object sender, EventArgs e)
        {
            return CurrentTime;
        }

        private void seek()//seeking functionality
        {
            //get song length
            TagLib.File l = TagLib.File.Create(SongView.SelectedItem.ToString());//create taglib file
            int SongLength = (int)l.Properties.Duration.TotalSeconds;//get the song length in seconds
            string time = CurrentTime.ToString();

        }


        private void Play_Prev(object sender, RoutedEventArgs e)//play the previous song in the current playlist
        {
            
            if (SongView.SelectedIndex != 0 )
            {
                SongView.SelectedItem = SongView.Items[SongView.SelectedIndex - 1];
                Play_Click(sender, e);
            }
            //SongView.SelectedItem = SongView.Items[SongView.SelectedIndex - 1];//adjust offset
            else
            {
                SongView.SelectedItem = SongView.Items[SongView.Items.Count - 1];
                Play_Click(sender, e);
            }
            
        }

        private void Play_Next(object sender, RoutedEventArgs e)//play the next song in the current playlist
        {
            //adjust offset
            //if(//not last in playlist)
            //{
            
            //}
            if (SongView.SelectedItem != SongView.Items[SongView.Items.Count - 1])
            {
                SongView.SelectedItem = SongView.Items[SongView.SelectedIndex + 1];
                Play_Click(sender, e);
            }
            //SongView.SelectedItem = SongView.Items[SongView.SelectedIndex - 1];//adjust offset
            else
            {
                SongView.SelectedItem = SongView.Items[0];
                Play_Click(sender, e);
            }

        }
    }

    

}
