## Giải thích thuật toán và hướng dẫn sử dụng

**📢Tạo code Line xuất hàng**
Cú pháp mã duy nhất bao gồm: [1 Year Code] [1 Month Code] [7 số tự tăng] [1 Line Code]
Tổng: 10 ký tự

**Giải thích:**
Sẽ có bảng quy đổi cho Năm Tháng và số Line theo Year Code, MonthCode, Line Code.

Ví dụ năm 25 => quy đổi ra ký tự 0, tháng 7 quy đổi ra chữ J, 7 số autoid reset theo tháng, Line index được quy đổi theo số line hiện tại , Line 1 => quy đổi số 0 . (Tham khảo thêm về tài liệu thuật toán).

Nguyên lý hoạt động của thuật toán: tự tăng số tối đa 7 con số từ (0000000-9,999,999)  <=> 10^7
Key reset sẽ tạo ra dạng **{yearCode}-{monthCode}-{lineCode}** nếu có sự thay đổi tháng sẽ reset giá trị tự tăng

**Cú pháp sử dụng** 

    AutoIDCodeGenerator.GenerateCodesWithAutoID(<số line hiện tại>, <số lượng mong muốn>)

- Số line hiện tại: hỗ trợ tối đa 1-30 line. Lưu ý bắt đầu từ 1
- Số lượng mong muốn: số lượng code muốn sinh ra trong một lần tạo (mỗi tháng tối đa 10 triệu code)

📒Location: BarcodeVerificationSystem/Utils/CodeGeneration/AutoIDCodeGenerator.cs
***
**📢Tạo code Line sản xuất - in Loyaltly**
Cú pháp mã duy nhất :
 [1 Year Code][1 Month Code] [6 số Base30 khó đoán][1 Line Code][2 ký tự random][1 checksum]
Tổng: 12 

**Giải thích**
Year Code : năm quy đổi
Month Code: tháng quy đổi
6 số Base30 khó đoán: có dùng thuật toán bước nhảy để gia tăng sự khó đoán
2 ký tự random: tăng sự khó đoán
1 checksum: tính đúng đắn thuật toán

    Base30AutoCodeGenerator.GenerateLineCodesForLoyalty(<số lượng>)

📒Location : BarcodeVerificationSystem/Utils/CodeGeneration/Base30AutoCodeGenerator.cs
