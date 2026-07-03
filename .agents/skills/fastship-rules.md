# FastShip Development Rules

> **Tự động áp dụng khi làm việc với dự án FastShip (ShipFood)**

## Nguyên tắc bắt buộc

Trước khi thực hiện BẤT KỲ thay đổi nào trong dự án, LUÔN:

### 1. Đọc tài liệu dự án

```yaml
required_reading:
  - Project.md     # Tổng quan kiến trúc, stack, tính năng, cấu trúc
  - UI-UX.md       # Thiết kế UI/UX chi tiết, responsive rules
  - Architectural-Solution.md  # Giải pháp kiến trúc, kế hoạch cải thiện
```

### 2. Tuân thủ quy tắc code

- **Conventions**: Tuân thủ nghiêm ngặt conventions hiện tại (C# coding style, Razor syntax, CSS naming)
- **Không thay đổi behavior không cần thiết**: Mỗi dòng code đều có mục đích
- **Tái sử dụng code**: Luôn dùng helper/component có sẵn
- **Tối giản**: Chỉ thay đổi tối thiểu để hoàn thành yêu cầu
- **Kiểm tra tồn tại**: Verify library/framework usage trong project trước khi dùng

### 3. Responsive & Mobile Rules

- Touch targets ≥ 44×44px trên mobile
- `font-size: 16px` trên input (chống iOS zoom)
- `data-label` attributes trên tất cả dashboard tables
- Aspect ratio 4/3 cho ảnh category, 16/9 trên mobile
- Flex layout thay vì fixed width
- Test trên breakpoints: 400px, 576px, 768px, 992px

### 4. UI/UX Standards

- Font: Inter (không dùng font khác trừ Roboto cho Google Identity)
- Primary color: `#3CB815` (xanh lá)
- Secondary color: `#F65005` (cam)
- Skeleton loading: shimmer CSS (không spinner)
- Cart: Session-based JSON, AJAX quantity
- Chat: SignalR real-time + Gemini AI

### 5. Database Rules

- MySQL + Pomelo EF Core (không SQL Server)
- `EnsureCreated()` cho development, migrations cho production
- BCrypt.Net-Next cho password (workFactor 12)
- Session JSON trong HttpContext.Session

### 6. Kiến trúc

- ASP.NET Core 8 MVC (không MVC 5)
- Cookie + Session auth (không Identity Framework)
- SignalR 8 cho real-time
- Chart.js cho charts
- Leaflet.js + SignalR cho live tracking

### 7. Quy trình làm việc

1. Đọc Project.md, UI-UX.md, Architectural-Solution.md
2. Spawn file-picker/code-searcher để tìm file liên quan
3. Đọc file cần sửa
4. Thực hiện thay đổi tối thiểu
5. Spawn code-reviewer-deepseek-flash để review
6. Commit message bằng tiếng Anh, rõ ràng, prefix theo conventional commits
