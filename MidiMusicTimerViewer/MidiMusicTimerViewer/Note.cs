using Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiMusicTimerViewer
{
    public class Note
    {
        public Channel Channel { get; set; }
        public float StartTime { get; set; }
        public float EndTime { get; set; }
        public Pitch Pitch { get; set; }
    }
}
