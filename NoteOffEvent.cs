using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NoteOffEvent : MidiEvent {

	public Midi.EventType eventType { get; protected set; }

	public int delay { get; set; }

	public long executeTime { get; set; }

	public int channel { get; protected set; }

	public int note { get; protected set; }

	public int velocity { get; protected set; }

	public NoteOffEvent(int delay, int channel, int note, int velocity) {
		this.eventType = Midi.EventType.NOTE_OFF;
		this.delay = delay;
		this.channel = channel;
		this.note = note;
		this.velocity = velocity;
	}
}
