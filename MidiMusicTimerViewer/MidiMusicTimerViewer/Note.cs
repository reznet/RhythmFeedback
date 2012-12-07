using Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiMusicTimerViewer
{
    /// <summary>
    /// Represents a note event.
    /// </summary>
    public class Note
    {
        /// <summary>
        /// Gets or sets the channel of the note.
        /// </summary>
        public Channel Channel { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds after the clock started that this note started.
        /// </summary>
        public float StartTime { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds after the clock started that this note ended.
        /// </summary>
        public float EndTime { get; set; }

        /// <summary>
        /// Gets or sets the pitch of this note.
        /// </summary>
        public Pitch Pitch { get; set; }
    }
}
