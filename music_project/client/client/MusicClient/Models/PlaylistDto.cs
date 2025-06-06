using System.Collections.Generic;

namespace MusicClient.Models
{
    public class PlaylistDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<TrackDto> Tracks { get; set; }
    }
} 