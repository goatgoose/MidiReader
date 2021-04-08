using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTempoEvent : MidiEvent {

	public Midi.EventType eventType { get; protected set; }

	public int delay { get; set; }

	public long executeTime { get; set; }

	public int tempo { get; protected set; }

	public ChangeTempoEvent(int delay, int tempo) {
		this.eventType = Midi.EventType.CHANGE_TEMPO;
		this.delay = delay;
		this.tempo = tempo;
	}

}
