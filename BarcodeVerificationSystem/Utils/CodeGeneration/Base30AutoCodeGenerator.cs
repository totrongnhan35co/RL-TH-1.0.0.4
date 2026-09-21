using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.CodeGeneration;
//using BarcodeVerificationSystem.Utils.CodeGeneration.Helper;
using GenCode.Types;
using System;
using System.Collections.Generic;
using System.IO;
using GenCode.Utils;
using BarcodeVerificationSystem.Utils.CodeGeneration.Helper;
using BarcodeVerificationSystem.Utils;
using System.Windows;
using BarcodeVerificationSystem.Model.CaoSuDongNai.CaoSuQRCode;

namespace GenCode.Utils
{

    /*

     n	Số Lines mỗi lần sinh mã | Dùng để tính bước nhảy và độ lệch theo dòng
     s	Giá trị bắt đầu (start)	| Cơ sở tính toán cho c, b, u
     m	Max random (ví dụ: 4 → giá trị r từ 0 đến 3) | Điều khiển độ ngẫu nhiên trong giá trị u
     r	Số random tại thời điểm hiện tại | Làm u khó đoán, tăng tính ngẫu nhiên
     j	Bước nhảy (jump step) = n × m | Khoảng nhảy lớn giữa các lần sinh
     c	Giá trị hiện tại, tăng đều mỗi lần sinh mã | Là thông số duy nhất cần lưu để sinh mã không trùng
     b	Bộ số bước nhảy: (c - s) / j | Giúp phân phối u cách đều các khoảng
     t	Số tăng phụ thuộc random và index dòng: t = n × r + i | Tăng nhỏ trong từng bước j
     u	Giá trị số nguyên cần chuyển sang base30: u = s + j × b + t	| Làm đầu vào cho encode
     i	Chỉ số Line, từ 0 đến n - 1	| Phân biệt các Line sinh cùng lượt

     */
    public class Base30AutoCodeGenerator
    {
        static StreamWriter csvWriter = new StreamWriter("log.csv", append: false);
        static HashSet<string> generatedCodes = new HashSet<string>(); // Kiểm tra trùng
        static Dictionary<int, int> currentPerLine = new Dictionary<int, int>(); // Quản lý current theo lineIndex ,Duy trì current riêng cho từng line

        public static event Action<string> OnDuplicateCode;
        public static event Action<int, double> OnGenerationCompleted;
        public static event Action<string> OnCodeGenerated;
        private static int totalCodeCount = 0;
        public static event Action<int> OnCodeCountChanged;

   
        public static List<string> GenerateLineCodesForLoyalty(int quantity)
        {
         
            // added surplus percentage
            int? surplusPercentage =  0;
            quantity = (int)((quantity * surplusPercentage) / 100) + quantity;

            string valueMode = "ManufacturingCurrentValue";
            int lineIndex = Shared.Settings.LineIndex;
            int totalLines = Shared.Settings.TotalLines;
            int startValue = Shared.FirstGeneratedCodeIndex = RegistryHelper.ReadValue(valueMode) == null ? 0 : int.Parse(RegistryHelper.ReadValue(valueMode));

            try
            {

                if (!currentPerLine.ContainsKey(lineIndex))
                    currentPerLine[lineIndex] = startValue;

                List<string> randomCodes = new List<string>();

                int s = startValue;
                int c = currentPerLine[lineIndex];
                int i = lineIndex;
                int n = totalLines;

                Random rand = new Random(Guid.NewGuid().GetHashCode());

                int r = rand.Next(1, 4);  // từ 1 đến 14 theo mô tả (tránh 0 vì j=0 không hợp lệ)
                int j = n * r;             // bước nhảy cố định cho line này trong đợt sinh
                int t = n * r + i;         // bước tăng theo line index (i)

                Console.WriteLine($"[INIT] Line={i}, n={n}, r={r}, j={j}, t={t}");

                for (int k = 0; k < quantity; k++)
                {
                    int b = (c - s) / j;       // số bước nhảy đã đi
                    int u = s + (j * b) + t;   // giá trị u cuối cùng để mã hóa base30

                    string lineIDStr = LineConvertHelper.GetLineCode(i).ToString();
                    string base30code = Base30Helper.EncodeToBase30_Loyaltly(u);
                    string rawCode = "";
                    string focusCode = "";
                   
                 
                        int factoryCodeInt = (int)FactoryCode.GiaLai;
                        string factoryCode = factoryCodeInt.ToString();
                        string currentYear = DateCodeHelper.GetCurrentYearTwoDigits();
                        string currentMonth = DateCodeHelper.GetCurrentMonthTwoDigits();
                        char monthVal = MonthCodeHelper.GetMonthCode(currentMonth);
                        char yearVal = YearCodeHelper.GetYearCode(currentYear);
                        string[] twoChar = RandomCharHelper.GetTwoRandomChars();

                        rawCode = factoryCode +
                                currentYear +
                                currentMonth +
                                monthVal +
                                yearVal +
                                base30code +
                                lineIDStr +
                                twoChar[0] +
                                twoChar[1];
                        string checksumSource = monthVal.ToString() +
                                     yearVal.ToString() +
                                     base30code +
                                     lineIDStr +
                                     twoChar[0] +
                                     twoChar[1];

                        int total;
                        char checksum = CodeConverter.GetChecksumChar(checksumSource, out total);
                        
                        focusCode = monthVal.ToString() +
                                         yearVal.ToString() +
                                         base30code +
                                         lineIDStr +
                                         twoChar[0] +
                                         twoChar[1]
                                          + checksum;
                    


                    string _focusCode = Manufacturing.GenerateCode(focusCode);

                    string allPODCode = _focusCode + "," + Manufacturing.GetUniqueCode(focusCode); //+ (!isManufacturingMode ? "," + Dispatching.getShipmentCode() + "," + Dispatching.getShiptoCode() : "");

                    randomCodes.Add(allPODCode);

                    if (!generatedCodes.Add(_focusCode))
                    {
                        Console.WriteLine($"⚠️ Trùng mã: {_focusCode}");
                        OnDuplicateCode?.Invoke(_focusCode); // báo trùng
                    }
                    else
                    {
                        OnCodeGenerated?.Invoke(_focusCode); // báo mã mới               
                        totalCodeCount++;
                        OnCodeCountChanged?.Invoke(totalCodeCount);
                    }
                    c = u;
                }
                currentPerLine[lineIndex] = Shared.LastGeneratedCodeIndex = c; // cập nhật lại current
                RegistryHelper.WriteValue(valueMode, (c+1).ToString());

                return randomCodes;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public static List<string> GenerateCodesForCaoSuDongNai(int quantity)
        {
            string valueMode = "CaoSuCurrentValue";
            string factoryCode = Shared.Settings.FactoryCode;
            int lineIndex = Shared.Settings.LineIndex;
            int totalLines = Shared.Settings.TotalLines;
            int startValue = Shared.FirstGeneratedCodeIndex = RegistryHelper.ReadValue(valueMode) == null ? 0 : int.Parse(RegistryHelper.ReadValue(valueMode));

            try
            {
                if (!currentPerLine.ContainsKey(lineIndex))
                    currentPerLine[lineIndex] = startValue;

                List<string> randomCodes = new List<string>();

                int s = startValue;
                int c = currentPerLine[lineIndex];
                int i = lineIndex;
                int n = totalLines;

                Random rand = new Random(Guid.NewGuid().GetHashCode());

                int r = rand.Next(1, 4);  // từ 1 đến 14 theo mô tả (tránh 0 vì j=0 không hợp lệ)
                int j = n * r;             // bước nhảy cố định cho line này trong đợt sinh
                int t = n * r + i;         // bước tăng theo line index (i)

                Console.WriteLine($"[INIT] Line={i}, n={n}, r={r}, j={j}, t={t}");

                for (int k = 0; k < quantity; k++)
                {

                    // 13 Characters : 
                    // Ma nha may: A (Nhà máy Xuân Lộc ) , B (Nhà máy An Đông)
                    // Line 1: 1, Line 2: 2 
                    // Thang: 1 character - monthVal
                    // Nam: 1 character - yearVal
                    // 5 character base36
                    // 3 random characters -  randomChars[0], randomChars[1], randomChars[2]
                    // 1 Check digit - checksumSource

                    int b = (c - s) / j;  
                    int u = s + (j * b) + t;

                    string focusCode = "";
                    string base30code = Base30Helper.EncodeToBase30_5Chars(u);
                    string currentYear = DateCodeHelper.GetCurrentYearTwoDigits();
                    string currentMonth = DateCodeHelper.GetCurrentMonthTwoDigits();
                    char monthVal = MonthCodeHelper.GetMonthCode(currentMonth);
                    char yearVal = YearCodeHelper.GetYearCode(currentYear);
                    string[] randomChars = RandomCharHelper.GetThreeRandomChars();

                    string checksumSource = monthVal.ToString() +
                                 yearVal.ToString() +
                                 base30code +
                                 randomChars[0] +
                                 randomChars[1] +
                                 randomChars[2];

                    int total;
                    char checksum = CodeConverter.GetChecksumChar(checksumSource, out total);

                    focusCode = factoryCode + lineIndex + monthVal.ToString() +
                                     yearVal.ToString() +
                                     base30code +
                                     randomChars[0] +
                                     randomChars[1] +
                                     randomChars[2]
                                      + checksum;

                    string _focusCode = CaoSuQRCode.GenerateCode(focusCode); // Total: 48 Characters

                    string allPODCode = _focusCode + "," + CaoSuQRCode.GetUniqueCode(focusCode);

                    randomCodes.Add(allPODCode);


                    if (!generatedCodes.Add(_focusCode))
                    {
                        Console.WriteLine($"⚠️ Trùng mã: {_focusCode}");
                        OnDuplicateCode?.Invoke(_focusCode); 
                    }
                    else
                    {
                        OnCodeGenerated?.Invoke(_focusCode);     
                        totalCodeCount++;
                        OnCodeCountChanged?.Invoke(totalCodeCount);
                    }
                    c = u;
                }
                currentPerLine[lineIndex] = Shared.LastGeneratedCodeIndex = c;
                RegistryHelper.WriteValue(valueMode, (c + 1).ToString());

                return randomCodes;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }


        public static void RunBulkGenerationTest(int totalCodes = 729000, 
            int codesPerBatch = 1000,
            int totalLines = 14, 
            int startValue = 100,
            int current = 100)
        {
            int codeGenerated = 0;
            int batch = 0;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (codeGenerated < totalCodes)
            {
                int remaining = totalCodes - codeGenerated;
                int quantity = Math.Min(codesPerBatch, remaining);

                Console.WriteLine($"[Batch {batch++}] Generating {quantity} codes...");

                int quantitySoFar = 0;

                for (int lineIndex = 0; lineIndex < totalLines; lineIndex++)
                {
                    int quantityPerLine = quantity / totalLines;

                    // ⚠ Phân bổ số dư cho các line đầu
                    if (lineIndex < quantity % totalLines)
                        quantityPerLine++;

                    // ❌ Bỏ qua line nếu không có gì để sinh
                    if (quantityPerLine <= 0)
                        continue;

                    GenerateLineCodesForLoyalty(quantityPerLine);

                    current += quantityPerLine;
                    quantitySoFar += quantityPerLine;
                }

                codeGenerated += quantitySoFar;

                // ⚠ Nếu chẳng có mã nào sinh ra (quantitySoFar == 0), dừng để tránh lặp vô hạn
                if (quantitySoFar == 0)
                {
                    Console.WriteLine("⚠ Không còn mã nào được sinh ra. Thoát vòng lặp.");
                    break;
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"✅ Done generating {codeGenerated} codes in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");

            csvWriter.Flush();
            csvWriter.Close();
            OnGenerationCompleted?.Invoke(codeGenerated, stopwatch.Elapsed.TotalSeconds);
        }

    }
}
