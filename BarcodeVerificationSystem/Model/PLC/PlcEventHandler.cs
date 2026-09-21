using BarcodeVerificationSystem.Controller;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.PLC
{
    internal class PlcEventHandler
    {
        private readonly ConcurrentQueue<(int compareIndex, int delayMilliseconds)> _eventQueue = new ConcurrentQueue<(int compareIndex, int delayMilliseconds)>();
        private readonly object _lock = new object();
        private volatile bool _isProcessing;
        private volatile bool _shouldStop;

        public PlcEventHandler()
        {
            // Start the processing thread
            new Thread(() => ProcessQueue()).Start();
        }

        private void RaiseDelayedCameraOutputSignalChangeEvent(int compareIndex, int delayMilliseconds)
        {
            if (!_shouldStop)
            {
                Thread.Sleep(delayMilliseconds); // Synchronous delay
                Shared.SendErrorOutputToSensorController(compareIndex);
            }

        }

        public void EnqueueEvent(int compareIndex, int delayMilliseconds)
        {
            if (!_shouldStop)
            {
                _eventQueue.Enqueue((compareIndex, delayMilliseconds));
                lock (_lock)
                {
                    if (!_isProcessing)
                    {
                        _isProcessing = true;
                        // Start a new thread to process if not already running
                        new Thread(() => ProcessQueue()).Start();
                    }
                }
            }
        }

        private void ProcessQueue()
        {
            while (!_shouldStop)
            {
                if (_eventQueue.TryDequeue(out var eventData))
                {
                    RaiseDelayedCameraOutputSignalChangeEvent(eventData.compareIndex, eventData.delayMilliseconds);
                }
                else
                {
                    lock (_lock)
                    {
                        if (_eventQueue.IsEmpty && !_shouldStop)
                        {
                            _isProcessing = false;
                            break;
                        }
                    }
                }
            }
        }


        public void StopQueue()
        {
            _shouldStop = true;
            lock (_lock)
            {
                // Drain the queue by dequeuing all items
                while (_eventQueue.TryDequeue(out _)) { }
                _isProcessing = false; // Allow the processing thread to exit
                _shouldStop = false;
            }
        }

    }

}
