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
            foreach(Playlist tmp in playlists)
            {
                if(tmp.position > pl.position)
                {
                    pl.position--;
                }
            }
            size -= 1;
        }

        //returns whether playlist already exists
        public bool containsPlaylist(Playlist pl)
        {
            foreach(Playlist tmp in playlists)
            {
                if(pl.getPath().Equals(tmp.getPath()))
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

        public List<Playlist> getPlaylists()
        {
            return playlists;
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
