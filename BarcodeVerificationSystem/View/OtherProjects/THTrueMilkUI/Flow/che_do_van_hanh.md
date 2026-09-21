
Chế độ 1: Mỗi batch dùng 1 mã QR
- Ở chế độ này mỗi QR chỉ được in trong một ngày
- Trường hợp sản xuất trong nhiều ngày thì khi qua 12h đêm sẽ thay đổi QR.
- Thời gian delta đổi QR (chế độ 1) --> Ví dụ delta là 1 thì sẽ gửi trước 12h 1 phút và giá trị này nhỏ nhất 1 phút, lớn nhất 15 phút.
Chế độ 2: Mỗi N phút sẽ thay đổi mã QR 1 lần. (N: setting)
Chế độ 3: Mỗi sản phẩm sẽ đổi mã QR 1 lần.
lưu ý: 
- Hệ thống đảm bảo mỗi mã QR chỉ được sử dụng bởi một Line
- Tại thời điểm đổi QR sẽ có độ trễ khoảng n buffer QR từ R-Link đã gửi cho máy in. n này do người dùng nhập để khi đổi code thì máy in vẫn gửi đi 1 lượng code buffer cũ

