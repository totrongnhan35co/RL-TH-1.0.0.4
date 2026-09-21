using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Interfaces
{
    public interface IPositionHandler
    {
        string Name { get; }
        bool TryDequeueData(out DetectModel detectModel, out string positionData);
        void ProcessPositionData(DetectModel detectModel, string positionData);
        bool ShouldThrowCancellation(CancellationToken token);
        bool ShouldAllowOutput(DetectModel detectModel, bool isPositionCorrect);
    }

    public class NoPositionHandler : IPositionHandler
    {
        private readonly ConcurrentQueue<DetectModel> _queueBufferDataObtained;
        public string Name => "NoPositionHandler";
        public NoPositionHandler(ConcurrentQueue<DetectModel> queueBufferDataObtained)
        {
            _queueBufferDataObtained = queueBufferDataObtained;
        }

        public bool TryDequeueData(out DetectModel detectModel, out string positionData)
        {
            positionData = null;
            if (!_queueBufferDataObtained.TryDequeue(out detectModel))
            {
                return false;
            }
            return true;
        }

        public void ProcessPositionData(DetectModel detectModel, string positionData)
        {
            // No position processing needed
            detectModel.isBarcodeWithinThreshold = "False";

            if(Shared.Settings.CameraList.FirstOrDefault().CameraType == CameraType.CV_X)
            {
                string[] numbers = detectModel.Text.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                if (numbers?.Length > 0)
                {
                    detectModel.Text = numbers[0];
                }
                else
                {
                    detectModel.Text = "";
                }
            }
        }

        public bool ShouldThrowCancellation(CancellationToken token)
        {
            return token.IsCancellationRequested && _queueBufferDataObtained.Count == 0;
        }

        public bool ShouldAllowOutput(DetectModel detectModel, bool isPositionCorrect)
        {
            // Always allow output for NoPositionHandler
            return detectModel.CompareResult != ComparisonResult.Valid;
        }
    }

    public class BarcodePositionHandler : IPositionHandler
    {
        private readonly ConcurrentQueue<string> _queuePositionDataObtained;
        public string Name => "BarcodePositionHandler";

        public BarcodePositionHandler(ConcurrentQueue<string> queuePositionDataObtained)
        {
            _queuePositionDataObtained = queuePositionDataObtained;
        }

        public bool TryDequeueData(out DetectModel detectModel, out string positionData)
        {
            detectModel = new DetectModel { Text = "", Image = Properties.Resources.icon_NoImage };
            if (!_queuePositionDataObtained.TryDequeue(out positionData))
            {
                return false;
            }
            return true;
        }

        public void ProcessPositionData(DetectModel detectModel, string positionData)
        {
            try
            {
                string[] numbers = positionData.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                double xPosition = numbers.Length > 0 ? double.Parse(numbers[0]) : 0;
                double yPosition = numbers.Length > 1 ? double.Parse(numbers[1]) : 0;
                string isAligned = numbers.Length > 2 ? numbers[2] : "False";
                string data = numbers.Length > 3 ? numbers[3] : "";
                detectModel.Text = data;
                detectModel.isBarcodeWithinThreshold = (isAligned.ToLower() == "true" || isAligned.ToLower() == "1")
                    ? $"True ({xPosition}, {yPosition})"
                    : $"False ({xPosition}, {yPosition})";
            }
            catch
            {
                detectModel.isBarcodeWithinThreshold = "False";
            }
        }

        public bool ShouldThrowCancellation(CancellationToken token)
        {
            return token.IsCancellationRequested && _queuePositionDataObtained.Count == 0;
        }
        public bool ShouldAllowOutput(DetectModel detectModel, bool isPositionCorrect)
        {
            // Always allow output for BarcodePositionHandler
            return detectModel.CompareResult != ComparisonResult.Valid && detectModel.Text != "" && isPositionCorrect;
        }
    }

    public class LogoPositionHandler : IPositionHandler
    {
        private readonly ConcurrentQueue<DetectModel> _queueBufferDataObtained;
        private readonly ConcurrentQueue<string> _queuePositionDataObtained;
        public string Name => "LogoPositionHandler";

        public LogoPositionHandler(ConcurrentQueue<DetectModel> queueBufferDataObtained, ConcurrentQueue<string> queuePositionDataObtained)
        {
            _queueBufferDataObtained = queueBufferDataObtained;
            _queuePositionDataObtained = queuePositionDataObtained;
        }

        public bool TryDequeueData(out DetectModel detectModel, out string positionData)
        {
            positionData = null;
            if (!_queueBufferDataObtained.TryDequeue(out detectModel))
            {
                return false;
            }
            _queuePositionDataObtained.TryDequeue(out positionData);
            return true;
        }

        public void ProcessPositionData(DetectModel detectModel, string positionData)
        {
            if (positionData == null)
            {
                detectModel.isBarcodeWithinThreshold = "False";
                return;
            }

            try
            {
                string[] numbers = positionData.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                string isAligned = numbers.Length > 0 ? numbers[0] : "False";
                detectModel.isBarcodeWithinThreshold = (isAligned.ToLower() == "true" || isAligned.ToLower() == "1")
                    ? "True"
                    : "False";
            }
            catch
            {
                detectModel.isBarcodeWithinThreshold = "False";
            }
        }

        public bool ShouldThrowCancellation(CancellationToken token)
        {
            return token.IsCancellationRequested && _queueBufferDataObtained.Count == 0;
        }
        public bool ShouldAllowOutput(DetectModel detectModel, bool isPositionCorrect)
        {
            // Always allow output for NoPositionHandler
            return detectModel.CompareResult != ComparisonResult.Valid;
        }
    }
}
