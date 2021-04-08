using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface MidiEvent {

	Midi.EventType eventType { get; }

	int delay { get; set; } // in ticks

	long executeTime { get; set; }
}
