 1.Cấu hình & kết nối thiết bị (tài khoản Config)
- Kết nối R-Link Master (Server 130) 
- Kết nối máy in RYNAN P54
- Kết nối Camera
- Kết nối PLC
- Cấu hình Line: IP Line, Tên Line
- Cài đặt và kết nối Database Local

2. Lấy và lưu thông tin thiết đặt từ R-Link Master
- Đăng nhập R-Link (R-Link sẽ đăng nhập bằng tài khoản do R-Link Master cung cấp)
- Dữ liệu cần kéo về và lưu lại thông tin, nếu như ngắt kết nối mạng => Thì lấy thông tin đã lưu:
Setting R-Link Master
+ Thư mục lưu trữ hình ảnh lỗi
+ Thời gian làm mới dữ liệu Dashboard (R-Link Monitoring): theo đơn vị phút.
+ Thời gian lưu log in, log Camera: theo đơn vị phút.
+ Khoảng Delta bù trừ để thay đổi QR Code ở chế độ .
+ Bufer gửi qua máy in
+ Ngưỡng cấp phát QR
Tài khoản:
+ Tên tài khoản
+ Phân quyền tài khoản (các chức năng đi kèm)
Danh mục sản phẩm:
+ Mã Sản Phẩm
+ Tên Sản Phẩm
3. Monitor R-Link trên R-Link Master
Gửi thông tin R-Link lên SaaS đảm bảo theo thời gian Monitoring thiết đặt trên r-Link Master
Hệ thống cho phép theo dõi các thông tin sau:
• Trạng thái hoạt động của từng thiết bị R-Link.
• Trạng thái kết nối của các thiết bị liên quan như Printer, Camera và PLC.
• Thông tin dữ liệu sản xuất của Công Việc (Job) hiện tại hoặc gần nhất, bao gồm thông tin:
o Batch đang sản xuất.
o Sản lượng sản xuất tại từng line
o Tổng sản lượng QR đã được cấp
o Tổng QR đã sử dụng
o Tổng QRCode không đạt yêu cầu trong quá trình sản xuất
o Hình ảnh lỗi gần nhất
4.Nhận & quản lý QR Code từ R-Link Master
Nhận QR Code từ R-Link Master → lưu vào Database Local
Phản hồi trạng thái lưu trữ lại cho R-Link Master
Cập nhật trạng thái QR Code trong Database Local (ĐÃ DÙNG HOẶC CHƯA DÙNG)
5.Tạo Job in (frmjob)
Tạo Job mới cho dây chuyền
+ Các thiết đặt lấy từ r-Link Master:
   Chế độ in
   Danh sách sản phẩm
   Thời gian delta đổi QR (chế độ 1) 
   N phút đổi QR(chế độ 2)
   Buffer gửi qua máy in
   Thời gian lưu log in, log Camera
+ Input từ người dùng:
   Chọn chính xác sản phẩm cần sản xuất theo danh sách từ SaaS
   Nhập Batch
   Nhập Số lượng cần sản xuất
   Chọn Job train Camera theo danh sách Keyence trả về (xem lại) - Pending
=> Nhấn Xác nhận tạo Job:
    + Kiểm tra Kho QR xem còn đủ số lượng (3 mode vận hành)
    + Nếu đủ số lượng => Lấy QR trong database Local theo quy tắc FI-FO
Hiển thị thông tin Job: tên Job, số lượng QR, chế độ, trạng thái
6. Vận hành Job(frmMain)
Người vận hành xác nhận Job trước khi chạy
 + Hiển thị danh sách QR Code trên màn hình vận hành
 + Gửi dữ liệu QR Code đến máy in
Start in:
  + Lưu PosstgreSQL và gửi log in, log Camera cho SaaS (Log report) trạng thái Start
Trong quá trình in:
  + Nhận phản hồi từ máy in, cập nhật giao diện + Database Local
  + Nhận phản hồi từ Camera cập nhật giao diện
  + Nếu có sản phẩm lỗi: 
         - Lưu hình ảnh trên R-Link (lấy cam hiện tại để test, tích hợp Keyence sau)
         - Nếu không khớp → gửi tín hiệu PLC xử lý lỗi
         - Lưu vào PostgreSQL và gửi lưu log dữ liệu ảnh lỗi lên SaaS
  +  Lưu PostgreSQL và gửi log in, log Camera cho SaaS (Log report) trạng thái Run định thời theo thời gian thiết đặt từ SaaS
Stop in:
  + Lưu PostgreSQL và gửi log in, log Camera cho SaaS (Log report), trạng thái Stop
Lưu ý:
   + Trong quá trình sản xuất, nếu tới thời gian delta thì phải đổi code (trường hợp QR chuẩn bị trước thiếu, thì tự động lấy QR từ database Local bổ sung vào)
   + Trong quá trình vận hành vẫn đảm bảo lệnh cấp phát từ QR R-Link Master hoạt động song song.
7. Xác nhận Hoàn thành Job
Công việc xử lý:
+ Gửi API yêu cầu xác nhận hoàn thành Job lên R-Link master (Job name, Số lượng QR đã sử dụng, Số lượng sản phẩm đã sản xuất) (SaaS tự kiểm tra và trả về QR cần cấp phát)
+ Nếu có danh sách QR được cấp phát thì lưu vào Database
+ Lưu PostgreSQL và gửi log in, log Camera cho SaaS (Log report), trạng thái Completed