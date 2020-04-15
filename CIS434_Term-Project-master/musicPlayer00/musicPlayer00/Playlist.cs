using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace musicPlayer00
{
    [Serializable()]
    class Playlist : ISerializable
    {
        static string[] acceptable = { "id3", "mp3", "mp4", "id3v1", "id3v2", "mpc", "ogg", "wav" };
        private String name;
        private String path;
        public Playlist parent;
        public int position;
        private List<Song> songs = new List<Song>();
        public int size;
        public Playlist(String name, String path, int position)
        {
            this.name = name;
            this.path = path;
            this.position = position;
            this.size = songs.Count();
            this.parent = null;
        }

        //for subfolder
        public Playlist(String name, String path, int position, Playlist parent)
        {
            this.name = name;
            this.path = path;
            this.position = position;
            this.size = songs.Count();
            this.parent = parent;
        }

        //add song to playlist
        public void addSong(Song song)
        {
            if (!alreadyHasSong(song) && acceptable.Any(x => song.getPath().ToLower().EndsWith(x)))
            {
                this.songs.Add(song);
                size++;
            }
        }

        //gets random song from playlist
        public Song getRandomSong()
        {
            Song[] tmp = songs.Where(e => e.getPath().EndsWith(".mp3") || e.getPath().EndsWith(".wav")).ToArray();
            int n = tmp.Length;
            Random rnd = new Random();
            int k = rnd.Next(0, n);
            return songs[k];
        }

        //input name of song, returns song
        public Song getSongByName(String name)
        {
            foreach (Song song in songs)
            {
                if (song.getName() == name)
                {
                    return song;
                }
            }
            throw new KeyNotFoundException();
        }

        //removes song
        public void removeSong(Song song)
        {
            this.songs.Remove(song);
            foreach (Song tmp in songs)
            {
                if (tmp.position > song.position)
                {
                    tmp.position--;
                }
            }
            size--;
        }

        public void Update_Songs()
        {
            for(int i = 0; i < songs.Count(); i++)
            {
                Console.WriteLine(File.Exists(songs[i].getPath()));
                if (!File.Exists(songs[i].getPath()))
                {
                    removeSong(songs[i]);
                }
            }
        }

        //get list of songs
        public List<Song> getSongs()
        {
            return this.songs;
        }

        //get path of playlist
        public string getPath()
        {
            return this.path;
        }

        //swaps position of this song and other song
        public void swapPosition(Playlist swap)
        {
            int tmp = swap.position;
            swap.position = this.position;
            this.position = tmp;
        }

        //gets playlist name
        public String getName()
        {
            return this.name;
        }

        //checks if playlist already has song of same name
        public bool alreadyHasSong(Song songToCompare)
        {
            foreach (Song song in songs)
            {
                if (song.getName().Equals(songToCompare.getName()))
                {
                    return true;
                }
            }
            return false;
        }

        //sorts songs by position property
        public void sortSongs()
        {
            songs = songs.OrderBy(o => o.position).ToList<Song>();
        }

        public int getMaxPosition()
        {
            return size > 0 ? songs.Max<Song>(e => e.position) : 0;
        }

        //set playlist name
        public void SetName(String newName)
        {
            name = newName;
        }

        //serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", name);
            info.AddValue("songs", songs);
            info.AddValue("path", path);
            info.AddValue("size", size);
            info.AddValue("position", position);
            info.AddValue("parent", parent);
        }
        public Playlist(SerializationInfo info, StreamingContext context)
        {
            name = (string)info.GetValue("name", typeof(string));
            songs = (List<Song>)info.GetValue("songs", typeof(List<Song>));
            path = (string)info.GetValue("path", typeof(string));
            size = (int)info.GetValue("size", typeof(int));
            position = (int)info.GetValue("position", typeof(int));
            if(parent != null)
                parent = (Playlist)info.GetValue("parent", typeof(Playlist));

        }
    }
}
