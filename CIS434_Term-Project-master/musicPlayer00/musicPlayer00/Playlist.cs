using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace musicPlayer00
{
    class Playlist
    {
        private String name;
        private String path;
        private int position;
        private List<Song> songs = new List<Song>();
        public Playlist(String name, String path, int position)
        {
            this.name = name;
            this.path = path;
            this.position = position;
        }

        public void addSong(Song[] songs)
        {
            foreach (Song song in songs)
            {
                this.songs.Add(song);
            }
        }

        //shuffles songs
        public void shuffleSongs() // source: stackoverflow.com/questions/5383498/shuffle-rearrange-randomly-a-liststring
        {
            int n = this.songs.Count;
            Random rnd = new Random();
            while (n > 1)
            {
                int k = (rnd.Next(0, n) % n);
                n--;
                Song value = this.songs[k];
                this.songs[k] = this.songs[n];
                this.songs[n] = value;
            }
        }

        public void removeSong(Song song)
        {
            this.songs.Remove(song);
        }

        public List<Song> getSongs()
        {
            return this.songs;
        }

        public string getPath()
        {
            return this.path;
        }

        public void swapPosition(Playlist swap)
        {
            int tmp = swap.position;
            swap.position = this.position;
            this.position = tmp;
        }
    }
}
