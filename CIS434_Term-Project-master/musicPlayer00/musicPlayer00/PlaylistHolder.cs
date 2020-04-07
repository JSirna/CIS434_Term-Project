using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace musicPlayer00
{
    class PlaylistHolder
    {
        private List<Playlist> playlists = new List<Playlist>();

        public void addPlaylist(Playlist pl)
        {
            playlists.Add(pl);
        }

        public void removePlaylist(Playlist pl)
        {
            playlists.Remove(pl);
        }

        public bool containsPath(String path)
        {
            foreach(Playlist playlist in playlists)
            {
                if (playlist.getPath() == path)
                    return true;
            }
            return false;
        }
        public bool containsName(String name)
        {
            foreach (Playlist playlist in playlists)
            {
                if (playlist.getName() == name)
                    return true;
            }
            return false;
        }
    }
}
