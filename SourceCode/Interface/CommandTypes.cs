namespace TMCAnalyzer {

	/// <summary>Send command responses.</summary>
	public enum AckTypes
	{
		/// <summary>Command accepted and being performed but not complete yet</summary>
		DoingCommand = 0,
		/// <summary>Command handled</summary>
		Ok = 1,
		/// <summary>No ack was expected or handled</summary>
		NoAck = 2,
		/// <summary>Command status is unknown when in scope mode</summary>
		StatusUnknownInScopeMode = 3,
		/// <summary>Port Error</summary>
		PortError = 4,
		/// <summary>No response arrived in alotted time</summary>
		TimeOut = 5,
		/// <summary>Unknown ack type</summary>
		Unknown = 6
	}
	/// <summary/>
	public enum CommandTypes {
		NoResponseExpected,
		ResponseExpected
	}
	/// <summary/>
	public enum CommandAckTypes
	{
		AckExpected,
		NoAckExpected
	}

	/// <summary/>
	public enum EchoStates
	{
		Enabled,
		Disabled,
		Unknown
	}

}