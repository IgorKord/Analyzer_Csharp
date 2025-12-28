using System;
using System.Threading;

namespace TMCAnalyzer {
    /// <summary/>
    public class SerialPortDataReader {

        /// <summary/>
        public delegate void OnDataCapturedDelegate(byte[] data, int dataLength);

        /// <summary/>
        public event OnDataCapturedDelegate OnDataCaptured;

        /// <summary/>
        /// <param name='readerPortBase'/>
        /// <param name='processSize'/>
        public SerialPortDataReader(SerPort readerPort, int processSize) {
            readerPortBase = readerPort;
            readSize = processSize;
        }

        private SerPort readerPortBase = /*set in _ctor*/ null;

        private int readSize = /*set in _ctor*/ 0;

        /// <summary>Starts the acquisition of data.</summary>
        public void Start() {
            lock (workerLock) {
                if (/*already running?*/ workerThread != null) {
                    throw new System.InvalidOperationException("worker already started");
                }
                workerIsActive = true;
                workerThread = new System.Threading.Thread(Worker);
                workerThread.Name = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + " " + Program.ConnectedController.PortName + ":" + Program.ConnectedController.BaudRate.ToString();
                workerThread.Start();
            }
        }

        /// <summary>Stop acquisition of data.</summary>
        public void Stop() {
//            lock (workerLock) {
                if (/*running?*/ workerThread != null) {
                    workerIsActive = false;
                    workerThread.Join(/*1s*/ 1000 /*ms*/);
                }
                workerThread = null;
            //}
        }

        private void Worker() {
            try {
                var /*buffer*/ bfr = new byte[2 * readSize];
                var /*bytes in buffer (population)*/ pop = 0;
                while (workerIsActive) {
                    try {
                        pop += readerPortBase.Read(bfr, pop, bfr.Length - pop);
                        if (/*enough data?*/ pop > readSize) {
                            if (/*not shutting down?*/ workerIsActive && (/*subscribers exist?*/ OnDataCaptured != null)) { OnDataCaptured(bfr, pop); }
                            pop = 0;
                        }
                    } catch (System.Exception ex) { System.Diagnostics.Debug.Assert(ex != null); }
                }
            } catch { throw; } 
        }

        private bool workerIsActive = true;
        private object workerLock = new object();
        private Thread workerThread = null;
    }
}