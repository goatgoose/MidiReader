using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EndOfTrackEvent : MidiEvent {

	public Midi.EventType eventType { get; protected set; }

	public int delay { get; set; }

	public long executeTime { get; set; }

	public EndOfTrackEvent(int delay) {
		this.eventType = Midi.EventType.END_OF_TRACK;
		this.delay = delay;
	}
}
