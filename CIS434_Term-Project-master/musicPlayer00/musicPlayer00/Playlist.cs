using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace musicPlayer00
{
    [Serializable()]
    class Playlist : ISerializable
    {
        private String name;
        private String path;
        public int position;
        private List<Song> songs = new List<Song>();
        public int size = 0;
        public Playlist(String name, String path, int position)
        {
            this.name = name;
            this.path = path;
            this.position = position;
            this.size = songs.Count();
        }

        //add song to playlist
        public void addSong(Song song)
        {
            this.songs.Add(song);
            size++;
        }

        //gets random song from playlist
        public Song getRandomSong()
        {
            int n = songs.Count();
            Random rnd = new Random();
            int k = rnd.Next(0, n);
            return songs[k];
        }
        
        //input name of song, returns song
        public Song getSongByName(String name)
        {
            foreach(Song song in songs)
            {
                if(song.getName() == name)
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
            foreach(Song tmp in songs)
            {
                if(tmp.position > song.position)
                {
                    tmp.position--;
                }
            }
            size--;
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
            foreach(Song song in songs)
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
        }
        public Playlist(SerializationInfo info, StreamingContext context)
        {
            name = (string)info.GetValue("name", typeof(string));
            songs = (List<Song>)info.GetValue("songs", typeof(List<Song>));
            path = (string)info.GetValue("path", typeof(string));
            size = (int)info.GetValue("size", typeof(int));
            position = (int)info.GetValue("position", typeof(int));

        }
    }
}
