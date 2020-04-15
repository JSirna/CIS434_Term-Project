using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace musicPlayer00
{
    [Serializable()]
    class PlaylistHolder : ISerializable
    {
        private List<Playlist> playlists = new List<Playlist>();
        public int size;

        public PlaylistHolder()
        {
            size = 0;
        }
        public void addPlaylist(Playlist pl)
        {
            playlists.Add(pl);
            size += 1;
        }

        //removes playlist
        public void removePlaylist(Playlist pl)
        {
            playlists.Remove(pl);
            foreach (Playlist tmp in playlists)
            {
                if (tmp.position > pl.position)
                {
                    pl.position--;
                }
            }
            size -= 1;
        }

        //returns whether playlist already exists
        public bool containsPlaylist(Playlist pl)
        {
            foreach (Playlist tmp in playlists)
            {
                if (pl.getPath().Equals(tmp.getPath()))
                {
                    return true;
                }
            }
            return false;
        }

        //Checks for playlist of same name
        public bool containsName(String name)
        {
            foreach (Playlist playlist in playlists)
            {
                if (playlist.getName() == name)
                    return true;
            }
            return false;
        }

        //gets last position via position property
        public int getMaxPosition()
        {
            return size > 0 ? playlists.Max<Playlist>(e => e.position) : 0;
        }

        //for subfolder
        public int getMaxPosition(Playlist parent)
        {
            Playlist[] pls = playlists.Where<Playlist>(e => e.parent == parent).ToArray();
            return size > 0 ? pls.Max<Playlist>(e => e.position) : 0;
        }

        public List<Playlist> getPlaylists()
        {
            return playlists;
        }
        public Playlist getRandomPlaylist()
        {
            Playlist[] tmp = playlists.Where(e => e.size > 0).ToArray();
            Random rnd = new Random();
            int n = tmp.Count();
            if (tmp.Count() < 1)
                throw new NullReferenceException();
            int k = rnd.Next(0, n);
            Console.WriteLine(n);
            return tmp[k];
        }

        //serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("playlists", playlists);
            info.AddValue("size", size);
        }

        public PlaylistHolder(SerializationInfo info, StreamingContext context)
        {
            playlists = (List<Playlist>)info.GetValue("playlists", typeof(List<Playlist>));
            size = (int)info.GetValue("size", typeof(int));
        }

    }
}
