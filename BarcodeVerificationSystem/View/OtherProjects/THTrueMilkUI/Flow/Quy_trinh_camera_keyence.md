Quy trình dữ liệu của camera Keyence VS_C500msx
Giao thức: TCP Socket (Port 8500) — Framed Mode (STX 0x02 / ETX 0x03)
Dữ liệu thô từ camera
[STX]TRGhttps://ndatrace.vn/01/8935217402816/21/hp9v3mzoio,true,NSX:28/05/2026,11:48,HSD:28/06/2027,A16,true,0000000001[ETX]

8 fields sau khi split bằng comma
Index			Field				Ví dụ							Ý nghĩa
0				QR					https://ndatrace.vn/...			Nội dung QR code
1				QR_Result			true							Kết quả đọc QR: true/false
2				NSX					NSX:28/05/2026					Ngày sản xuất (từ camera, có prefix NSX:)
3				Time				11:48							Thời gian sản xuất
4				HSD					HSD:28/06/2027					Hạn sử dụng (từ camera, có prefix HSD:)
5				Batch				A16								Lô 
6				OCR_Result			true							Kết quả OCR: true/false
7				Image_ID			0000000001						ID ảnh để download từ FTP


1. Việc cần làm:
- lưu chuỗi này bảng vào tb_CameraLogs
- Hiển thị dữ liệu lên bảng dgvCameraLogs với các cột: QR (đã có), trạng thái (đã có), NSX, HSD, Batch(lấy NSX hiển thị), THời gian nhận (đã có)
- Chỉ so sánh kết quả khi QR_Result và OCR_Result đều là true, nếu có một trong hai là false thì sẽ phân loại lỗi luôn (vì đã biết lỗi ở đâu rồi)
- 

2. So sánh dữ liệu giữa QR code và OCR PHÂN LOẠI LỖI

qr_quantity				ocr_quantity		So sánh dữ liệu					 Kết quả 								PLC
					
FALSE						FALSE			FALSE								F									Camera
TRUE						FALSE			FALSE								F									Camera
FALSE						TRUE			FALSE								B									"Nếu lỗi liên tục n sản phẩm - thiết đặt trên Rlink Master Rlink (dừng dàn máy)"
TRUE						TRUE			Rlink so sánh QR (lưu lại kết quả)	Nếu sai là F và đúng là A			False: XUẤT TÍN HIỆU Phân loại



3. Đếm lỗi liên tục tối đa là nếu sai liên tục là bắt đầu đếm và reset về 0 nếu đúng, nếu đạt đến số lượng lỗi liên tục đã thiết đặt thì sẽ xuất tín hiệu dừng dàn máy

qr_quantity			sảN phẩn lỗi liên tục	
FALSE (B)			1	
TRUE (F)			2	
FALSE (B)			3	
TRUE (A)			0	
FALSE (B)			1	
FALSE (B)			2	
FALSE (B)			3	
TRUE (F)			4	
TRUE (F)			5	Xuất PLC dừng dàn máy

4. Lưu dữ liệu vào bảng tb_PrintingLogs:
- Thêm cột: NSX (dd mm yy), HSD (dd mm yy)
- Thêm Cột: Thời gian in mã - dòng cuối cùng có đủ giá trị
- Thêm cột: NSX trên sản phảm cuối cùng (dd mm yy HH:mm) - Lấy dòng cuối cùng có giá trị (DỮ liệu Camera)

manufactured_date
expiry_date
last_printed_at
printer_last_product_manufactured_date

5. Lưu dữ liệu vào bảng tb_CameraLogs:
- Thêm cột: NSX (dd mm yy), HSD (dd mm yy)
- Thêm Cột: Thời gian nhận gói tin (details)  lấy ngày nhận log từ Rlink
- Thêm cột: NSX trên sản phảm cuối cùng (dd mm yy HH:mm)
camera_manufactured_date
camera_expiry_date
camera_last_packet_received_at
camera_last_product_manufactured_date

6. Lưu dữ liệu vào bảng tb_ProductDefectLogs
- Thêm cột: QR (đã có), NSX (dd mm yy), HSD (dd mm yy) (gói tin trả về có dữ liệu gì lưu dữu liệu đó)
- Thêm cột: Chuỗi dữ liệu Camera (details)
- Thêm cột: Thời gian nhận (details) Lấy ngày nhận log từ Rlink"
- Đường dẫn hình ảnh lỗi (đã có)
- Thêm cột: Phân loại (B,F) (details)
- casch đặt tên tương tự bảng log camera như thêm từ error vào

7. Cách xử lý lưu ảnh lỗi:
RLink sẽ lấy index trigger gửi về từ Camera để xác định lưu hình ảnh lỗi.
Trong trường hợp tín hiệu lỗi đến trước kết quả lưu hình ảnh, thì rlink lưu đường dẫn hình ảnh vào database, hình ảnh sẽ kiểm tra sau.
Khi Người dùng nhấn Stop, thì RLink kiểm tra lại, nếu hình nào chưa được lưu thì sẽ dò và tìm ảnh lỗi, lưu vào thư mục Job.
