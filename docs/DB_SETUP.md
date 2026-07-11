# 🗄 Hướng dẫn kết nối Database Local

> Dùng chung DB PostgreSQL trên Render cho local development.
> **Chú ý**: DB này là production — tránh thao tác xoá dữ liệu quan trọng.

---

## ⚡ Cách 1: Dùng biến môi trường (Khuyên dùng)

### Terminal (CMD/PowerShell):
```powershell
cd ShipFoodCore
$env:DATABASE_URL="postgresql://dbfoody_user:aeS5wafY0L6R3jzYj9JGfRfddUUTnzvl@dpg-d97scdpkh4rs73bik3u0-a.singapore-postgres.render.com/dbfoody"
dotnet run
```

### Hoặc tạo file `.env` ở thư mục gốc dự án:
```env
DATABASE_URL=postgresql://dbfoody_user:aeS5wafY0L6R3jzYj9JGfRfddUUTnzvl@dpg-d97scdpkh4rs73bik3u0-a.singapore-postgres.render.com/dbfoody
```

> File `.env` đã có trong `.gitignore` — an toàn, không bị push lên GitHub.

---

## ⚡ Cách 2: Sửa `appsettings.json`

Mở `ShipFoodCore/appsettings.json` và cập nhật:

```json
{
  "ConnectionStrings": {
    "dbFoodyEntities": "postgresql://dbfoody_user:aeS5wafY0L6R3jzYj9JGfRfddUUTnzvl@dpg-d97scdpkh4rs73bik3u0-a.singapore-postgres.render.com/dbfoody"
  }
}
```

> ⚠️ **Cảnh báo**: `appsettings.json` nằm trong `.gitignore` nên sẽ không bị push lên GitHub.
> Nhưng nếu bạn bỏ nó khỏi `.gitignore`, mật khẩu sẽ bị lộ!

---

## 🔧 Cách hoạt động

`Program.cs` tự động xử lý kết nối theo thứ tự ưu tiên:

```
1. appsettings.json → ConnectionStrings:dbFoodyEntities
2. Environment Variable → DATABASE_URL
3. Environment Variables → PGHOST, PGPORT, PGUSER, PGPASSWORD, PGDATABASE
```

`ParsePgConnectionString()` tự động chuyển đổi:
- `postgresql://user:pass@host:5432/db` 
- → `Host=host;Port=5432;Database=db;Username=user;Password=pass;SSL Mode=Require;Trust Server Certificate=true`

---

## 🐳 Kết nối bằng Database Client (DBeaver, pgAdmin, VS Code)

### Thông số kết nối:

| Field | Giá trị |
|-------|---------|
| **Host** | `dpg-d97scdpkh4rs73bik3u0-a.singapore-postgres.render.com` |
| **Port** | `5432` |
| **Database** | `dbfoody` |
| **Username** | `dbfoody_user` |
| **Password** | `aeS5wafY0L6R3jzYj9JGfRfddUUTnzvl` |
| **SSL** | Required |

### VS Code — PostgreSQL Extension:
1. Cài extension "PostgreSQL" (ms-ossdata.vscode-postgresql)
2. Thêm connection mới với thông số trên
3. Tab "SSL" → set `SSL Mode = Require`
