# Luồng gửi Command PLC — THTrueMilk

> Phiên bản: sau sửa đổi 2024  
> PLC chỉ có 1 port (Port 2 = 0)

---

## 1. Khởi động App

Mở frmJobTHTrueMilk / frmMainTHTrueMilk
    │
    ├── Set SensorControllerPort2 = 0
    ├── Set PLCVersion = 1
    └── Gửi ARED? ❌ KHÔNG (đã bỏ PendingLoginRed)

---

## 2. Kết nối PLC

Monitor Thread — 2 giây/lần
    │
    ├── Khởi tạo: PODController(IP, Port1, Port2=0)
    ├── Connect() → Luôn gọi
    ├── Connect2() → BỎ QUA (Port2=0)
    │
    └── Khi Connected:
        ├── SendSettingToSensorController() ✅
        └── Send ARED? ❌ KHÔNG

---

## 3. Sơ đồ Command

┌─────────────────────────────────────────────────────────┐
│                    TRẠNG THÁI BẮT ĐẦU                  │
│              Chưa kết nối / Chưa chạy                  │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│              KẾT NỐI PLC THÀNH CÔNG                     │
│         Gửi: Cấu hình encoder (P...D...L...)          │
└─────────────────────────────────────────────────────────┘
                          │
        ┌─────────────────┼─────────────────┐
        ▼                 ▼                 ▼
┌───────────────┐   ┌───────────────┐   ┌───────────────┐
│   START JOB   │   │    RUNNING    │   │  MẤT KẾT NỐI  │
└───────────────┘   └───────────────┘   │   > 10 giây   │
        │                   │             └───────────────┘
        ▼                   │                       │
┌───────────────┐          │                       ▼
│  Đủ điều kiện │          │             ┌───────────────┐
│  PLC+Camera+  │          │             │   GỬI ARED    │
│  Máy in OK    │          │             │   + AutoStop  │
└───────────────┘          │             └───────────────┘
        │                  │
        ▼                  │
┌───────────────┐          │
│  GỬI AGREEN   │          │
│ (Sẵn sàng)    │          │
└───────────────┘          │
        │                  │
        ▼                  ▼
┌───────────────┐   ┌───────────────┐
│   HOÀN THÀNH  │   │  USER STOP    │
│   (Stop xong) │   │ (Bình thường) │
│   Gửi AGREEN  │   │ GỬI AREDSTOP  │
└───────────────┘   └───────────────┘

---

## 4. Bảng Command chi tiết

| Command | Khi nào gửi | Điều kiện | Gói tin TCP |
|---------|-------------|-----------|-------------|
| **Cấu hình encoder** | PLC vừa kết nối | `IsSensorControllerConnected` | `(P...D...L...H...G...E...)` |
| **AGREEN** | Bắt đầu chạy | `Processing → Running` | `<STX>AGREEN<ETX>` |
| **AGREEN** | Kết thúc stop | `OperStatus == Processing` | `<STX>AGREEN<ETX>` |
| **ARED** | Máy in mất >10s | `isRunning + PLC connected` | `<STX>ARED<ETX>` |
| **ARED** | Camera mất >10s | `isRunning + PLC connected` | `<STX>ARED<ETX>` |
| **ARED** | PLC mất >10s | `isRunning` | `<STX>ARED<ETX>` |
| **ARED** | Lỗi liên tục | Đạt ngưỡng | `<STX>ARED<ETX>` |
| **AREDSTOP** | User bấm Stop | Bình thường (không lỗi) | `<STX>AREDSTOP<ETX>` |
| **AYELLOW** | Cảnh báo | - | `<STX>AYELLOW<ETX>` |
| **REJECT** | User bấm Reject | - | `<STX>REJECT<ETX>` |

