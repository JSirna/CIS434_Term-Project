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
    class Song : ISerializable
    {
        private Playlist pl;
        private String name;
        public int position;
        private string path;
        public Song(Playlist pl, String name, int position, String path)
        {
            this.pl = pl;
            this.name = name;
            this.position = position;
            this.path = path;
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

        public String getPath()
        {
            return this.path;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", name);
            info.AddValue("playlist", pl);
            info.AddValue("path", path);
            info.AddValue("position", position);
        }
        public Song(SerializationInfo info, StreamingContext context)
        {
            name = (string)info.GetValue("name", typeof(string));
            pl = (Playlist)info.GetValue("playlist", typeof(Playlist));
            path = (string)info.GetValue("path", typeof(string));
            position = (int)info.GetValue("position", typeof(int));
        }
    }
}
