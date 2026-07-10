# ShipFood AI Rules

> ⚠️ **BẮT BUỘC**: Luôn sử dụng các skill đã cài đặt trong `.agents/skills/` trước khi thực hiện bất kỳ tác vụ nào.

---

## 1. 🎯 Nguyên tắc sử dụng Skills

**TRƯỚC KHI LÀM VIỆC**, phải kiểm tra và sử dụng skill phù hợp từ `.agents/skills/`:

```yaml
quy_tắc_bắt_buộc:
  - Luôn load skill phù hợp với task trước khi bắt đầu
  - Dùng skill thay vì tự suy luận nếu skill đã có sẵn
  - Nếu có nhiều skill liên quan, dùng kết hợp tất cả
```

### 🕐 Tần suất Load Skill

```yaml
tần_suất_load_skill:
  # Mỗi phiên (session) chỉ cần load 1 lần
  - Đầu phiên: load tất cả skill liên quan đến dự án hiện tại
  - Khi chuyển task mới: load skill phù hợp với task đó (nếu chưa load trong phiên)
  - KHÔNG cần reload skill đã load trong cùng phiên
  
  # Khi nào load lại:
  - Bắt đầu session mới (mở lại terminal/IDE)
  - Chuyển sang dự án khác
  - Cần skill mới chưa load trong phiên hiện tại
```

### 📚 Quy trình Load Skill Chuẩn

```yaml
quy_trình_load_skill:
  step_1: "Xác định task cần làm → skill nào phù hợp?"
  step_2: "Kiểm tra skill đã load trong phiên chưa?"
  step_3: |
    Nếu CHƯA load → dùng skill tool để load
    Nếu ĐÃ load → dùng luôn, không load lại
  step_4: "Tham khảo codebase (Project.md, UI-UX.md, file-picker)"
  step_5: "Bắt đầu làm việc với sự hỗ trợ của skill"
```

### Các nhóm skills chính:

| Nhóm | Skill | Khi nào dùng |
|------|-------|-------------|
| **🧠 Lên kế hoạch** | `brainstorming`, `writing-plans`, `executing-plans` | Trước khi code tính năng mới |
| **🔍 Nghiên cứu** | `agent-reach`, `exa-search`, `parallel-web`, `research-lookup` | Cần tìm kiếm thông tin, research API, đọc docs |
| **📐 UI/UX Design** | `ui-ux-pro-max`, `design`, `design-system`, `banner-design`, `brand`, `ui-styling`, `slides` | Thiết kế giao diện, components, styling |
| **🧪 Kiểm thử** | `test-driven-development` | Viết unit test trước khi code |
| **📝 Code Review** | `requesting-code-review`, `receiving-code-review`, `code-reviewer-deepseek-flash` | Review code sau khi thay đổi |
| **🐛 Debug** | `systematic-debugging` | Khi gặp bug, test failure |
| **✂️ Tối ưu code** | `ponytail`, `ponytail-review`, `ponytail-audit` | Giảm thiểu code thừa, tối ưu |
| **🧬 Khoa học/Data** | `scikit-learn`, `matplotlib`, `seaborn`, `statsmodels`, `pandas`... | Phân tích dữ liệu, ML, thống kê |
| **🔬 Bioinformatics** | `scanpy`, `biopython`, `rdkit`, `pydeseq2`... | Nếu dự án có liên quan sinh học/hóa học |
| **🚀 Phát triển** | `subagent-driven-development`, `dispatching-parallel-agents`, `verification-before-completion`, `finishing-a-development-branch` | Chia nhỏ task, chạy parallel agents |
| **🛡 Bảo mật** | `gstack` (security router) | Audit bảo mật |
| **📊 Git/Workflow** | `using-git-worktrees`, `using-superpowers` | Quản lý branch, workflow |
| **📝 Viết skills mới** | `writing-skills` | Khi cần tạo skill tùy chỉnh |

---

## 2. 📋 Quy trình làm việc bắt buộc

```mermaid
flowchart TD
    A[Nhận task] --> B[Load skill phù hợp]
    B --> C[Đọc file liên quan]
    C --> D[Spawn agent/subagent nếu cần]
    D --> E[Thực hiện thay đổi]
    E --> F[Review bằng skill phù hợp]
    F --> G[Kiểm tra lỗi / chạy test]
```

1. **Load skill trước**: Dùng `skill tool` để load skill phù hợp
2. **Đọc tài liệu**: `Project.md`, `UI-UX.md`
3. **Tìm file**: Dùng file-picker, code-searcher
4. **Thực hiện**: Với sự hỗ trợ của skill đã load
5. **Review**: Dùng code-review skill

---

## 3. 🔄 Tham chiếu

- File rules chi tiết: `.agents/skills/fastship-rules.md`
- Danh sách đầy đủ skills: `skills-lock.json`
- Thư mục skills chính: `.agents/skills/`

## 4. 📦 Kho Repo trong thư mục `ShipFoodCore/Skills/`

### Các repo skills chính:

| Repo | Mô tả | Khi nào dùng |
|------|-------|-------------|
| **agent-reach-main** | Agent truy cập 13+ nền tảng (Twitter, Reddit, YouTube, GitHub...) | Cần tìm kiếm/tương tác web |
| **ponytail-main** | Ponytail optimization suite | Tối ưu code, giảm thiểu, refactor |
| **codegraph-main** (trong `Graph/`) | CodeGraph retrieval & indexing | Phân tích codebase, tìm kiếm nâng cao |
| **gstack-main** | Security router suite | Audit bảo mật |
| **superpowers-main** (trong `FLow/`) | Superpowers workflow tools | Quản lý workflow, phiên làm việc |
| **whisper-flow-main** (trong `prompt/`) | Prompt engineering | Thiết kế prompt tối ưu |
| **ui-ux-pro-max** (trong `UI UX/` + `Skill/`) | UI/UX design: 50+ styles, 161 palettes, 57 font pairings | Thiết kế giao diện (2 repo này cũng nội dung, dùng 1 cái) |
| **scientific-agent-skills-main** | Khoa học: bioinformatics, ML, statistics... | Phân tích dữ liệu khoa học |
| **developer-icons-main** | Bộ icon SVG cho developer | Tạo icon, logo, UI elements |

### Repo tham khảo:

| Repo | Mô tả |
|------|-------|
| **public-apis-master** | 📚 Danh sách public APIs miễn phí — tham khảo khi cần tích hợp API bên thứ 3 (không phải skill AI, chỉ là danh sách tham khảo) |

### 🎯 Khi nào dùng repo trong Skills/

```yaml
dùng_repo_khi:
  - Cần code mẫu từ skill chưa được load sẵn
  - Muốn tham khảo implementation của 1 skill cụ thể
  - Cần documentation chi tiết của skill
  - Muốn xem ví dụ/test cases từ skill repo
  - Cần API reference từ public-apis-master
```

---

*File này được AI đọc tự động mỗi khi làm việc với dự án. Tuân thủ nghiêm ngặt.*
