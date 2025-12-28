using System;
using System.Reactive.Subjects;
using System.IO.Ports;
using System.Collections.Generic;

namespace TMCAnalyzer
{
	/// <summary>
	/// Interacts with Comminication Manager
	/// </summary>
	public class Device
	{
		#region Private Fields
	  //  private CommunicationManager _communication;
		//private string _selectedCOM = "COM16";
		public string[] Settings = new string[4] { "11500", "None", "8", "One" };
		private string _selectedIP = "";
		private int selectedTelnetPort = 0;
		private string responseData = string.Empty;
		private readonly Subject<string> _dataReceived = new Subject<string>();
		private readonly Subject<string> _errorMessage = new Subject<string>();
		private System.Timers.Timer TimerDataRead;
		#endregion

		#region Public Fields
		public static Device _connectedDevice;
		private int Bytes;
		private int InBufferSize = 10000;

		#endregion

		#region Properties
		/// <summary>
		/// Creates instance for the Device class.
		/// </summary>
		public static Device Instance
		{
			get
			{
				if (_connectedDevice == null)
				{
					_connectedDevice = new Device();

				}
				return _connectedDevice;
			}
		}
		/// <summary>
		/// Stores the current com port name
		/// </summary>
		public string SelectedCOM
		{
			get
			{
				return TMCController.ControllerPort.PortName;
			}

		}

		/// Stores the current com port status
		/// </summary>
		public bool PortIsOpen
		{
			get
			{
				if (Program.ConnectedController == null)
				{
					Program.FormMain.ConnectToController(true, false);
				}
				if (TMCController.ControllerPort != null)
					return TMCController.ControllerPort.IsOpen;
				else
					return false;
			}

		}
		public SerialPort SerialPort
		{
			get { return TMCController.ControllerPort.serialPortMuxerBase; }

		}

		/// <summary>
		/// Stores the current IP address
		/// </summary>
		public string SelectedIP
		{
			get { return _selectedIP; }
			set { _selectedIP = value; }
		}
		/// <summary>
		/// Stores the current port number of the ethernet connection
		/// </summary>
		public int SelectedTelnetPort
		{
			get { return selectedTelnetPort; }
			set { selectedTelnetPort = value; }
		}
		///// <summary>
		///// Holds the Com port Connection status, whether connected or not.
		///// </summary>
		//public IObservable<bool> IsConnected { get { return _communication.IsConnected; } }

		///// <summary>
		///// Holds the telnet Connection status, whether connected or not.
		///// </summary>
		//public IObservable<int> TelnetState { get { return _communication.TelnetState; } }

		/// <summary>
		/// Holds the received packets through com/usb.
		/// </summary>
		public IObservable<string> ReceivedPackets { get { return _dataReceived; } }

		/// <summary>
		/// Holds the received packets through telnet/ethernet
		/// </summary>
		//public IObservable<string> TelnetReceivedPackets { get { return _communication.TelnetReceivedPackets; } }

		/// <summary>
		/// Holds the received error message in serial communication
		/// </summary>
		public IObservable<string> ReceivedErrorMessage { get { return _errorMessage; } }

		///// <summary>
		///// Holds the received error message in telnet communication
		///// </summary>
		//public IObservable<string> TelnetReceivedErrorMessage { get { return _communication.TelnetReceivedErrorMessage; } }

		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of Device.
		/// Creates new instance of comm. manager
		/// </summary>
		public Device()
		{
		   // SerialPort.DataReceived += SerialPortMuxerBase_DataReceived;
			TimerDataRead = new System.Timers.Timer(25);
			TimerDataRead.Elapsed += OnElapsed;

			//_communication = new CommunicationManager();
		}

		private void SerialPortMuxerBase_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
		{
			try
			{
				while (PortIsOpen && SerialPort.BytesToRead > 0)
				{
					string data = SerialPort.ReadExisting();
					_dataReceived.OnNext(data);
				}
			}
			catch (Exception ex)
			{
			   // _errorMessage.OnNext(e.Message);
			}
		}


		#endregion

		#region Private Implmentation
		/// <summary>
		/// Checking for the received data
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="e"></param>
		private void OnElapsed(Object Sender, EventArgs e)
		{
			CheckDataReceived();
			//if(!Program.formTerminal.IsMenuActive) StopTimerToReadData();
		}

		private void CheckDataReceived()
		{
			//if (Program.ValidateScopeCheck)
			//{
			//	try
			//	{
			//		//Program.FormScope.datReceived.Reset();
			//		Program.FormScope.datReceived.WaitOne(300);
			//		string TermBuf = TMCController.ControllerPort.GetEntireBuffer;
			//		_dataReceived.OnNext(TermBuf);
			//		if (TermBuf.Contains(">~")) StopTimerToReadData();
			//	}
			//	catch (Exception e)
			//	{
			//		_errorMessage.OnNext(e.Message);
			//	}
			//}
			//else
			{
				Bytes = TMCController.ControllerPort.BytesToRead(TMCController.ControllerPort);
				if (Bytes > InBufferSize) Bytes = InBufferSize;   // Only read as many charactes as is our buffer size.
				try
				{
					if (Bytes > 0)
					{
						string TermBuf = TMCController.ControllerPort.GetEntireBuffer;
						_dataReceived.OnNext(TermBuf);
						if (TermBuf.Contains(">~")) StopTimerToReadData();
					}
				}
				catch (Exception e)
				{
					_errorMessage.OnNext(e.Message);
				}
			}

			//int byte_cnt = 0;
			//byte[] byteArrayData = new byte[4096];
			//List<byte> receivedData = new List<byte>();
			//try
			//{
			//    while (BytesToRead)
			//    {
			//        byte_cnt += networkStream.Read(byteArrayData, byte_cnt, 4096);
			//        for (int i = 0; i < byteArrayData.Length; i++)
			//        {
			//            if (byteArrayData[i] != 0)
			//                receivedData.Add(byteArrayData[i]);
			//            else
			//                break;
			//        }
			//        _dataReceived.OnNext(System.Text.Encoding.UTF8.GetString(receivedData.ToArray()));
			//    }
			//}
			//catch (Exception e)
			//{
			//    _errorMessage.OnNext(e.Message);
			//}
		}
		#endregion

		#region Public Implementation

		/// <summary>
		/// Calls _communication Init method.
		/// </summary>
		/// <param name="portname"></param>
		public void Connect(string portname, string[] settings)
		{
			//SelectedCOM = portname;
			Settings = settings;
		   // _communication.Init(portname, settings);
		}

		/// <summary>
		/// Calls _communication InitIP method.
		/// </summary>
		/// <param name="portname"></param>
		public void IPInit()
		{
			//_communication.IPInit();
		}

		/// <summary>
		/// Calls _communication OpenCOM method.
		/// </summary>
		/// <param name="portname"></param>
		public void OpenCOM()
		{
			//_communication.OpenCOM();
		}

		/// <summary>
		/// Calls _communications OpenIP method.
		/// </summary>
		/// <param name="portname"></param>
		public void OpenIP(string ipAdrs, int telnetPort)
		{
			SelectedIP = ipAdrs;
			SelectedTelnetPort = telnetPort;
		   // _communication.OpenIP( ipAdrs, telnetPort);
		}

		/// <summary>
		/// Calls _communications CloseIP method.
		/// </summary>
		/// <param name="portname"></param>
		public void CloseIP()
		{
		   // _communication.CloseIP();
		}

		/// <summary>
		///  Calls _communications Send method.
		/// </summary>
		/// <param name="bb">command which sends to connected port/controller through serial connection</param>
		/// <returns></returns>
		public bool Send(string bb)
		 {
			try
			{
				//if (!_serialPort.IsOpen)
				//    Connect();
				if (PortIsOpen)
				{
					//if (Program.ValidateScopeCheck) {
					//	Program.FormScope.datReceived.Reset();
					//	Program.FormScope.mainFormQueue.Clear();
					//}
					TMCController.ControllerPort.Write(bb);
					TimerDataRead.Enabled = true;
					//if (bb.Contains("echo>enab")) Program.FormMain.IsEchoEnable = true;
					//if (bb.Contains("echo>dis")) Program.FormMain.IsEchoEnable = false;
					//_dataSent.OnNext(cmd);
					return true;
				}
				return false;
			}
			catch (Exception e)
			{
				_errorMessage.OnNext(e.Message);
			  //  Disconnect();
				return false;
			}
		   // TMCController.ControllerPort.serialPortMuxerBase.Write(bb);
		   // Program.FormMain.SendInternal(bb, CommandTypes.ResponseExpected, out responseData, CommandAckTypes.AckExpected);
		   // return true;
		   // return _communication.Send(bb);
		}
		/// <summary>
		///
		/// </summary>
		/// <param name="bb">command which sends to connected port/controller through telnet connection</param>
		/// <returns>true, if it sends the command successfully</returns>
		public bool TelnetSend(string bb)
		{
			return false;
		   // return _communication.TelnetSend(bb);
		}

		/// <summary>
		/// Calls _communications Dispose method.
		/// Terminate the connection
		/// </summary>
		public void Dispose()
		{
		   // TimerDataRead.Enabled = false;
			//try
			//{
			//    if (_serialPort.IsOpen)
			//        _serialPort.Dispose();
			//}
			//catch (Exception e)
			//{
			//    _errorMessage.OnNext(e.Message);
			//}

			// _communication.Dispose();
		}
		public void StartTimerToReadData()
		{
			TimerDataRead.Enabled = true; ;
		}
		public void StopTimerToReadData()
		{
			TimerDataRead.Enabled = false;
		}
		/**********************************************************************************************************************/
		public static string ReadLineWithTimeout(TimeSpan timeout) {
			var line = string.Empty;
			var startTime = DateTime.Now;
			var endTime = startTime + timeout;
			int Line_Dur;
			TimeSpan Line_wait;
			do {
				try {
					line = TMCController.ControllerPort.ReadLine(out Line_Dur); //while loop inside until not empty or timeout 100 ms
					if (line.Length > 0) {
						Line_wait = DateTime.Now.Subtract(startTime);
						line = line.Trim('\r', '\n'); //sometimes it starts from \n; like "\n2250.0,   -0.92,  -29.45,   -9.93,  -30.37,   18.37, \t#"
						break;
					} else {
						//                        Line_wait = DateTime.Now.Subtract(startTime);
						//                        System.Diagnostics.Debug.WriteLine("got '' ReadLine dur=" + Line_Dur.ToString() + " ms; elaps t= " + Line_wait.TotalSeconds);
					}
				} catch (System.TimeoutException) {
					//expected (buffer is empty); ignore
					System.Diagnostics.Debug.WriteLine("Device_GetLoopTransferFunction.ReadLineWithTimeout");
				}
			} while (DateTime.Now < endTime);
			return line;
		}
		#endregion


	}
}
