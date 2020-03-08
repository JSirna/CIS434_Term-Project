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
        String selectedHeader;
        bool playing = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        public void OpenFolder() //refactor as needed
        {
        }

        public void LoadMusic() //refactor as needed
        {

        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!playing) //plays song
                {
                    String name = SongView.SelectedItem.ToString();
                    String key = name+selectedHeader;
                    String song = songPath[key];
                    player.URL = song;
                    player.controls.play();
                    playing = true;
                    PlayButton.Content = "Pause";
                }
                else //pause doesn't work properly currently
                {
                    player.controls.stop();
                    playing = false;
                    PlayButton.Content = "Play";
                }
            }
            catch (NullReferenceException) //no song selected
            {
                return;
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                player.controls.pause();
            }
            catch (Exception)
            {
                Console.Write("not currently playing a song");
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
                    if (songPath.ContainsKey(key))
                        songPath.Remove(key);
                    songPath.Add(key, song);
                    SongView.Items.Add(tmp); //add each song to songview
                }
            }
            catch (DirectoryNotFoundException) //if directory deleted remove folder from treeview
            {
                namePath.Remove(selectedHeader); //remove deleted file from dictionary
                Remove_Folder_View(selectedHeader); //remove file from view
                return;
            }
            catch (NullReferenceException)
            {
                return;
            }
        }


        /*
         *PERIPHERY METHODS 
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
    }
}
