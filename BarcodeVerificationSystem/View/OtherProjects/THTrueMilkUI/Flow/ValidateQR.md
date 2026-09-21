1. Khi nhận cấp phát qr
2. KHi tạo JOB
3. Khi start lại JOB cũ



Nội dung:

2. Khi tạo JOB
- Trong frm D:\R-Link-Branch-Nhan\R_Link\BarcodeVerificationSystem\View\OtherProjects\THTrueMilkUI\Manufacturing\frmJobTHTrueMilk.cs
ở thao tác tạo job khi nhấn saveJobTH thì nó lấy danh sách qr dựa vào gtin sản phẩm đã chọn trong sản phẩm có product_gtin và xem có đủ số lượng qr đủ điều kiện cần dùng cho job không
- 
- 
validate gồm:
- Khi nhận QR thì validate xem máy in và camera có ds template và ds program tương ứng với Gtin không
- Khi tạo job thì 




TẠO JOB (frmJobTHTrueMilk):
  1. User chọn sản phẩm → có GTIN
  2. Lấy QR config (baseUrl, numberOfUrl)
  3. GetSampleQr: query WHERE gtin=@gtin AND startsWith(baseUrl) AND length=numberOfUrl
  4. BuildCsv: query WHERE gtin=@gtin AND startsWith(baseUrl) AND length=numberOfUrl
  5. Lưu GTIN + baseUrl + numberOfUrl vào JobModel
  6. frmValidateQrCode: hiển thị ảnh sản phẩm + QR hợp lệ
  7. User xác nhận → tạo job → chuyển sang trang vận hành

START JOB (frmMainTHTrueMilk):
  1. Validate 1 lần: gtin có khớp camera/printer template không
  2. Start printer
  3. Mỗi dòng trước khi in:
     - Validate qr.StartsWith(baseUrl)
     - Validate qr.Length == numberOfUrl
     - Nếu fail → skip + log

ĐỔI QR (Mode 2 timer):
  1. Query QR mới WHERE gtin=@gtin AND startsWith(baseUrl) AND length=numberOfUrl
  2. Chỉ lấy QR pass validati