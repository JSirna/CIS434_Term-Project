﻿using System;
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
        bool SongEnd = false;
       



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
                    pl.Update_Songs();
                }
            }
            string myMusicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic); //using Environment instead of hard-code
            try
            {
                bool hasFile = false;
                string[] MusicDir = Directory.GetDirectories(myMusicPath);
                foreach (string folders in MusicDir) 
                {
                    
                    Playlist pl = new Playlist(getFileName(folders), folders, plHolder.getMaxPosition());
                    if (!plHolder.containsPlaylist(pl))
                        Add_Folder_View(pl); //add selected folder
                    hasFile = false;
                    
                   
                }
            }
            catch (UnauthorizedAccessException) { } //if you enter a directory your not allowed to.
            catch { } // All Other exceptions

            // Threading a clock for the timer & slider
            DispatcherTimer dTimer = new DispatcherTimer(DispatcherPriority.Send);
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

                if ((currentlyPlaying != null) && ((int)player.controls.currentPosition == Song_Duration() - 1))
                {
                    player.controls.currentPosition = 0;
                    Play_Next(new object(), new RoutedEventArgs());
                }
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
                TxtSliderMaxValue.Content = convertToString(Song_Duration()); // shows song length at right of slider
                player.URL = song.getPath();                
                player.controls.play();
            }
            if (paused) //continue playing
            {
                player.controls.play();                
            }
            else
            {               
                player.controls.pause();
            }
        }

        // write in console what second you are in the song only works when playing
        

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
            folderDisplay.Items.Add(CreateTreeItem(tmpTVI, pl));
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

            if (item == null)

                Console.WriteLine("No item was selected to display files");

            else

            {

                try

                {

                    Playlist pl = null;

                    //pl = selectedHeader;

                    if (item.Tag is Playlist)

                        pl = (item.Tag as Playlist);

                    var songs = Directory.EnumerateFiles(pl.getPath()); //Enumerate files is the only one that actually returns the files into listview

                    foreach (string folder in songs)

                    {

                        Song tmp = new Song(pl, getFileName(folder), pl.getMaxPosition() + 1, folder);

                        pl.addSong(tmp);

                    }

                    pl.sortSongs();

                    selectedHeader = pl;

                    foreach (Song song in pl.getSongs())

                    {

                        try

                        {

                            if (File.Exists(song.getPath()))

                            {

                                if (!song.getName().EndsWith(".jpg") && !song.getName().EndsWith(".ini") && !song.getName().EndsWith(".db")

                                    && !song.getName().EndsWith(".wpl") && !song.getName().EndsWith(".pla") && !song.getName().EndsWith(".png"))

                                {

                                    try

                                    {

                                        TagLib.File songThing = TagLib.File.Create(song.getPath());

                                        string title = songThing.Tag.Title == "" || songThing.Tag.Title == null ? song.getName().Replace(".mp3", "") : songThing.Tag.Title;

                                        ListViewItem lvi = new ListViewItem { Content = title, Tag = song };

                                        SongView.Items.Add(lvi);

                                    }

                                    catch (TagLib.UnsupportedFormatException ex)

                                    {

                                        Console.WriteLine(ex);

                                    }

                                }

                            }

                        }

                        catch (DirectoryNotFoundException ex)

                        { //If song moved out of folder

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
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            Shuffle_Songs();
        }

        private void Shuffle_Songs()
        {
            try
            {
                //Playlist randPL = plHolder.getRandomPlaylist();
                Song randSong = selectedHeader.getRandomSong();
                //selectedHeader = randPL;
                SongView.SelectedItem = null;
                paused = false;
                currentlyPlaying = null;
                Play(selectedHeader, randSong);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
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

        private void Add_New_Folder(object sender, System.Windows.DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, false);
            var dirs = files.Where(s => Directory.Exists(s));
            foreach (string dir in dirs)
            {
                try
                {
                    Playlist pl = new Playlist(getFileName(dir), dir, plHolder.getMaxPosition() + 1);
                    plHolder.addPlaylist(pl);
                    Add_Folder_View(pl);
                }
                catch (System.IO.IOException)
                {
                    Console.WriteLine("Adding folder at path : " + dir + "failed");
                    continue;
                }
            }
        }

        //Saves current folders
        private void On_Close(object sender, EventArgs e)
        {
            File.Delete("PlHolderData.dat");
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
            { Console.WriteLine("Failed to get song Length"); }

            TagLib.File l = TagLib.File.Create(fi.ToString());//create taglib file
            SongLength = (int)l.Properties.Duration.TotalSeconds;//get the song length in seconds
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

        // 
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

        private void TreeView_Expansion(object sender, RoutedEventArgs e) //currently works for only one subfolder :(
        {
            TreeViewItem item = e.OriginalSource as TreeViewItem;
            TreeViewItem newItem;

            string newPath = "";
            string myMusicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string[] dirs = Directory.GetDirectories(myMusicPath); //can't get a path from a treeview item, so this is hard-coded...

            foreach (string dir in dirs) //each dir is my path, and I want the path that contains the item.Header
            {
                if (dir.Contains(item.Header.ToString()))
                {
                    newPath = dir;
                    Console.WriteLine("New Path: " + newPath);
                }
            }
            if ((item.Items.Count == 1) && (item.Items[0] is string))
            {
                item.Items.Clear();
                Playlist pl = new Playlist(item.HeaderStringFormat, newPath, plHolder.getMaxPosition());
                try
                {
                    #region The stuff that works
                    string[] subFolders = Directory.GetDirectories(pl.getPath());
                    foreach (var subDir in subFolders)
                    {
                        pl = new Playlist(getFileName(subDir), subDir, pl.getMaxPosition());
                        newItem = new TreeViewItem { Header = pl.getName(), Tag = pl };
                        item.Items.Add(CreateTreeItem(newItem, pl));
                    }
                    #endregion
                }
                catch { Console.WriteLine("Something caught in the Expansion method"); }
            }
        }

        private TreeViewItem CreateTreeItem(TreeViewItem o, Playlist plObject)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = o.Header;
            item.Tag = plObject;
            item.Items.Add("Loading...");
            //Console.WriteLine(item.Header);
            return item;
        }
    }

   

   
}
