//using BarcodeVerificationSystem.Controller;
//using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
//using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
//using BarcodeVerificationSystem.View.NutrifoodUI.Manufacturing;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;

//namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Core
//{
//    public class VerificationQueueProcessor
//    {
//        private readonly BlockingCollection<VerificationDataEntry> _queue;
//        private readonly IStorageService<VerificationDataEntry> _storage;
//        private readonly ISenderService<VerificationDataEntry> _sender;

//        public VerificationQueueProcessor(BlockingCollection<VerificationDataEntry> queue, IStorageService<VerificationDataEntry> storage, ISenderService<VerificationDataEntry> sender)
//        {
//            _storage = storage;
//            _sender = sender;
//            _queue = queue;
//        }

//        public void Start()
//        {
//            _sender.Start();
//            foreach (var entry in _storage.LoadUnsentEntries())
//            {
//                _queue.Add(entry);
//            }

//        }

//        public void Enqueue(int id, string[] code)
//        {
//            // Lấy product_code: ưu tiên CaoSuProduct, fallback LastCheckedProductCode
//            string productCode = string.Empty;
//            if (!string.IsNullOrWhiteSpace(Shared.CurrentJob?.CaoSuProduct?.product_code))
//                productCode = Shared.CurrentJob.CaoSuProduct.product_code;
//            else if (!string.IsNullOrWhiteSpace(Shared.CurrentJob?.LastCheckedProductCode))
//                productCode = Shared.CurrentJob.LastCheckedProductCode;

//            var entry = new VerificationDataEntry
//            {
//                Id = id,
//                Code = code[FrmMainNutri.Index_ResultData],
//                VerifiedStatus = code[FrmMainNutri.Index_Result],
//                VerifiedDate = code[FrmMainNutri.Index_DateTime],
//                LotNumber = Shared.CurrentJob?.LOTNumber ?? string.Empty,
//                ProductCode = productCode,
//                Weight = Shared.CurrentJob?.productWeight.ToString() ?? string.Empty,
//            };
//            _storage.AppendEntry(entry);
//            _queue.Add(entry);
//        }

//        public void Stop()
//        {
//            _sender.Stop();
//        }
//    }

//}
using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.View.NutrifoodUI.Manufacturing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Core
{
    public class VerificationQueueProcessor
    {
        private readonly BlockingCollection<VerificationDataEntry> _queue;
        private readonly IStorageService<VerificationDataEntry> _storage;
        private readonly ISenderService<VerificationDataEntry> _sender;

        public VerificationQueueProcessor(BlockingCollection<VerificationDataEntry> queue, IStorageService<VerificationDataEntry> storage, ISenderService<VerificationDataEntry> sender)
        {
            _storage = storage;
            _sender = sender;
            _queue = queue;
        }

        public void Start()
        {
            _sender.Start();
            foreach (var entry in _storage.LoadUnsentEntries())
            {
                _queue.Add(entry);
            }
        }

        /// <summary>
        /// Enqueue mã check thông thường (dùng cho _checkedDataProcess)
        /// </summary>
        public void Enqueue(int id, string[] code)
        {
            string productCode = string.Empty;
            if (!string.IsNullOrWhiteSpace(Shared.CurrentJob?.CaoSuProduct?.product_code))
                productCode = Shared.CurrentJob.CaoSuProduct.product_code;
            else if (!string.IsNullOrWhiteSpace(Shared.CurrentJob?.LastCheckedProductCode))
                productCode = Shared.CurrentJob.LastCheckedProductCode;

            var entry = new VerificationDataEntry
            {
                Id = id,
                Code = code[FrmMainNutri.Index_ResultData],
                VerifiedStatus = code[FrmMainNutri.Index_Result],
                VerifiedDate = code[FrmMainNutri.Index_DateTime],
                LotNumber = Shared.CurrentJob?.LOTNumber ?? string.Empty,
                ProductCode = productCode,
                Weight = Shared.CurrentJob?.productWeight.ToString() ?? string.Empty,
            };
            _storage.AppendEntry(entry);
            _queue.Add(entry);
        }

        /// <summary>
        /// Enqueue mã vào pallet processor — ghi vào file pallet để load lại khi mở job
        /// </summary>
        public void EnqueuePallet(int id, string[] code)
        {
            string productCode = string.Empty;
            if (!string.IsNullOrWhiteSpace(Shared.CurrentJob?.CaoSuProduct?.product_code))
                productCode = Shared.CurrentJob.CaoSuProduct.product_code;
            else if (!string.IsNullOrWhiteSpace(Shared.CurrentJob?.LastCheckedProductCode))
                productCode = Shared.CurrentJob.LastCheckedProductCode;

            var entry = new VerificationDataEntry
            {
                Id = id,
                Code = code[FrmMainNutri.Index_ResultData],
                VerifiedStatus = code[FrmMainNutri.Index_Result],
                VerifiedDate = code[FrmMainNutri.Index_DateTime],
                LotNumber = Shared.CurrentJob?.LOTNumber ?? string.Empty,
                ProductCode = productCode,
                Weight = Shared.CurrentJob?.productWeight.ToString() ?? string.Empty,
            };

            // Ghi vào file pallet để load lại khi mở job
            _storage.AppendEntry(entry);
            _queue.Add(entry);
        }

        public void Stop()
        {
            _sender.Stop();
        }
    }
}