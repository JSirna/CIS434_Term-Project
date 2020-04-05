using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace musicPlayer00
{
    class Song
    {
        private Playlist pl;
        private String name;
        private int position;
        public Song(Playlist pl, String name, int position)
        {
            this.pl = pl;
            this.name = name;
            this.position = position;
        }

        public String getName()
        {
            return this.name;
        }
        public void setName(String name)
        {
            this.name = name;
        }
        public Playlist getPlaylist()
        {
            return this.pl;
        }
        public int getPosition()
        {
            return this.position;
        }
        public void setPosition(int position)
        {
            this.position = position;
        }
    }
}
