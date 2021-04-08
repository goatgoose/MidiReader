using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlChangeEvent : MidiEvent {
	
	public Midi.EventType eventType { get; protected set; }

	public int delay { get; set; }

	public long executeTime { get; set; }

	public Controller controller { get; protected set; }

	public int value { get; protected set; }

	public ControlChangeEvent(int delay, int controllerId, int value) {
		this.eventType = Midi.EventType.CONTROL_CHANGE;
		this.delay = delay;
		this.value = value;

		switch (controllerId) {
		case 0x40:
			controller = Controller.DAMPER_PEDAL;
			break;
		case 0x41:
			controller = Controller.PORTAMENTO;
			break;
		case 0x42:
			controller = Controller.SUSTENUTO;
			break;
		case 0x43:
			controller = Controller.SOFT_PEDAL;
			break;

		default:
			controller = Controller.UNDEFINED;
			break;
		}
	}

	public enum Controller {
		UNDEFINED,
		DAMPER_PEDAL,
		PORTAMENTO,
		SUSTENUTO,
		SOFT_PEDAL
	}
}
