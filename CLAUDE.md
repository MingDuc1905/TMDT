# ShipFood AI Rules

> ⚠️ **BẮT BUỘC**: Luôn sử dụng các skill đã cài đặt trong `.agents/skills/` **VÀ** các repo trong `ShipFoodCore/Skills/` trước khi thực hiện bất kỳ tác vụ nào.

---

## 0. 📝 LOG SKILL KHI LÀM VIỆC

**BẮT BUỘC TUYỆT ĐỐI — KHÔNG EXCEPTION**: Mỗi khi bắt đầu hoặc trong quá trình thực hiện task, Buffy PHẢI ghi log các skill đã load.

```yaml
quy_tắc_log_skill:
  trigger: "MỖI RESPONSE — luôn luôn, không bao giờ bỏ qua"
  format: |
    **Skill đã load**: <danh sách skill đã load trong session>
    **Skills Repo used**: <tên repo từ ShipFoodCore/Skills/> | <mục đích>
    **Agent spawned**: <tên agent> | <mục đích> (nếu có)
  rules:
    - "KHÔNG BAO GIỜ response mà KHÔNG có log skill ở đầu"
    - "Khi load skill mới → thêm vào danh sách, ghi **Skill mới load**"
    - "Khi dùng repo → ghi rõ repo nào, dùng làm gì"
    - "Khi spawn agent → ghi agent nào, mục đích gì"
    - "Log skill TRƯỚC KHI làm việc, KHÔNG phải sau khi xong"
```

**Ví dụ đúng**:
```
**Skill đã load**: brainstorming, ui-ux-pro-max, ponytail, gstack
**Skills Repo used**: developer-icons-main | SVG icons for navbar
```

**Ví dụ SAI** (bị trừ điểm):
```
// Không có log skill → SAI
// Log skill sau khi xong việc → SAI
// Chỉ ghi tên skill không ghi repo → SAI
```

---

## 1. 🎯 Nguyên tắc sử dụng Skills

**TRƯỚC KHI LÀM VIỆC**, phải kiểm tra và sử dụng skill phù hợp từ `.agents/skills/` **VÀ** `ShipFoodCore/Skills/`:

```yaml
quy_tắc_bắt_buộc:
  - Luôn load skill phù hợp với task trước khi bắt đầu
  - Dùng skill thay vì tự suy luận nếu skill đã có sẵn
  - Nếu có nhiều skill liên quan, dùng kết hợp tất cả
  - Ghi log tất cả skill đã load vào mỗi response
  - MỞ RỘNG: Luôn kiểm tra ShipFoodCore/Skills/ trước khi code tính năng mới
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
  step_5: "Kiểm tra ShipFoodCore/Skills/ có repo phù hợp không?"
  step_6: "Bắt đầu làm việc với sự hỗ trợ của skill"
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
| **🚀 Phát triển** | `subagent-driven-development`, `dispatching-parallel-agents`, `verification-before-completion`, `finishing-a-development-branch` | Chia nhỏ task, chạy parallel agents |
| **🛡 Bảo mật** | `gstack` (security router) | Audit bảo mật |
| **📊 Git/Workflow** | `using-git-worktrees`, `using-superpowers` | Quản lý branch, workflow |
| **🎨 Icon/Logo** | `developer-icons-main` (ShipFoodCore/Skills/) | Tạo icon, SVG, logo UI |
| **📝 Viết skills mới** | `writing-skills` | Khi cần tạo skill tùy chỉnh |

### 🔧 Cách dùng repo trong ShipFoodCore/Skills/

```yaml
cách_dùng_repo_skills:
  developer-icons-main:
    - "Dùng SVG icons từ ShipFoodCore/Skills/developer-icons-main/icons/"
    - "Copy file SVG cần dùng vào wwwroot/Source/icons/"
    - "Ưu tiên hơn Font Awesome vì không bị AdBlock chặn"
  
  ui-ux-pro-max-skill-main:
    - "Dùng CLI: npx uipro-cli generate --type design-system"
    - "Áp dụng 67 UI styles, 161 color palettes từ skill"
  
  ponytail-main:
    - "Chạy audit: skill ponytail && skill ponytail-audit"
    - "Tối ưu code, giảm dependencies thừa"
  
  gstack-main:
    - "Chạy security audit: skill gstack"
    - "QA review cho code changes"
  
  agent-reach-main:
    - "Tìm kiếm web, đọc tài liệu, check API docs"
  
  superpowers-main:
    - "Quản lý git worktree, workflow tự động"
  
  codegraph-main:
    - "Phân tích dependency graph, code structure"
  
  whisper-flow-main:
    - "Thiết kế prompt, tối ưu AI interactions"
```

---

## 2. 📋 Quy trình làm việc bắt buộc

```mermaid
flowchart TD
    A[Nhận task] --> B[Load skill phù hợp + GHI LOG]
    B --> C[Đọc file liên quan]
    C --> D[Kiểm tra ShipFoodCore/Skills/ repo]
    D --> E[Spawn agent/subagent nếu cần]
    E --> F[Thực hiện thay đổi]
    F --> G[Review bằng skill phù hợp]
    G --> H[Kiểm tra lỗi / chạy test]
    H --> I[Build Docker locally nếu có thay đổi csproj/Dockerfile]
```

1. **Load skill trước**: Dùng `skill tool` để load skill phù hợp
2. **Kiểm tra Skills repo**: Đã có repo nào trong `ShipFoodCore/Skills/` phù hợp chưa?
3. **Ghi log**: Ghi rõ skill nào đã load, repo nào đã dùng
4. **Đọc tài liệu**: `Project.md`, `UI-UX.md`
5. **Tìm file**: Dùng file-picker, code-searcher
6. **Thực hiện**: Với sự hỗ trợ của skill đã load
7. **Review**: Dùng code-review skill
8. **Build Docker** (nếu thay đổi phụ thuộc): Kiểm tra build trước khi commit

---

## 3. 🔄 Docker Build Rules

### ⚠️ BẮT BUỘC khi thêm package NuGet mới:

```yaml
quy_tắc_nuget:
  - "Luôn kiểm tra version package có tồn tại trên NuGet.org không"
  - "Sau khi thêm package = dotnet add package, kiểm tra .csproj version CHÍNH XÁC"
  - "Chạy dotnet build LOCAL trước"
  - "Kiểm tra Dockerfile có restore được package đó không"
```

### ⚙️ Tối ưu Docker build:

```yaml
docker_build_optimization:
  - ".dockerignore phải exclude ShipFoodCore/Skills/ (86MB+ exe + hàng ngàn files)"
  - ".dockerignore phải exclude .git/, bin/, obj/, node_modules/"
  - "Giữ --no-restore trong publish step — restore ở layer riêng"
  - "Nếu thay đổi .csproj → restore layer bị invalidate → chạy lại restore"
```

### 🔧 Troubleshooting:

```yaml
docker_build_errors:
  error_CS0246_PackageNotFound:
    cause: "Package version không tồn tại trên NuGet"
    fix: "Kiểm tra version trong .csproj, cập nhật đúng version"
    example:
      - "QRCoder 1.6.1 không tồn tại → fix thành 1.7.0"
  
  error_BuildChậm:
    cause: "COPY . . copy cả Skills/ (86MB exe)"
    fix: "Thêm .dockerignore exclude ShipFoodCore/Skills/"
```

---

## 4. 🔄 Tham chiếu

- File rules chi tiết: `.agents/skills/fastship-rules.md`
- Danh sách đầy đủ skills: `skills-lock.json`
- Thư mục skills chính: `.agents/skills/`
- Kho repo skills: `ShipFoodCore/Skills/`
- Dockerfile: `Dockerfile`
- Docker ignore: `.dockerignore`

## 5. 📦 Kho Repo trong thư mục `ShipFoodCore/Skills/`

### Các repo skills chính:

| Repo | Mô tả | Vị trí | Khi nào dùng |
|------|-------|--------|-------------|
| **ponytail-main** | Ponytail optimization suite | `ShipFoodCore/Skills/ponytail-main/` | Tối ưu code, giảm thiểu, refactor |
| **gstack-main** | Security router suite (YC CEO) | `ShipFoodCore/Skills/gstack-main/` | Audit bảo mật, QA, code review |
| **agent-reach-main** | Agent truy cập 13+ nền tảng | `ShipFoodCore/Skills/agent-reach-main/` | Cần tìm kiếm/tương tác web |
| **developer-icons-main** | 700+ SVG tech icons | `ShipFoodCore/Skills/developer-icons-main/` | Tạo icon, logo UI |
| **codegraph-main** | CodeGraph retrieval & indexing | `ShipFoodCore/Skills/Graph/codegraph-main/` | Phân tích codebase |
| **superpowers-main** | Superpowers workflow tools | `ShipFoodCore/Skills/FLow/superpowers-main/` | Quản lý workflow |
| **whisper-flow-main** | Prompt engineering | `ShipFoodCore/Skills/prompt/whisper-flow-main/` | Thiết kế prompt |
| **ui-ux-pro-max** | UI/UX design (161 rules, 67 styles, 161 color palettes, 57 font pairings) | `ShipFoodCore/Skills/UI UX/ui-ux-pro-max-skill-main/` | Thiết kế giao diện, chọn màu, font, style |
| **awesome-claude-design** | 68 DESIGN.md patterns (design tokens, layout, components, responsive, accessibility) | `ShipFoodCore/Skills/awesome-claude-design/` | Thiết kế UI mới — đảm bảo design system nhất quán |
| **UI UX data** | Color palettes (161 CSV), typography specs, brand guidelines, logo colors | `ShipFoodCore/Skills/UI UX/` | Tra cứu màu sắc, font pairings, style guide |
| **public-apis-master** | Public APIs collection | `ShipFoodCore/Skills/public-apis-master/` | Tìm kiếm APIs, tham khảo tích hợp dịch vụ |
| **scientific-agent-skills-main** | 400+ scientific skill packages | `ShipFoodCore/Skills/scientific-agent-skills-main/` | Data analysis, ML |

### Cách load repo skill:

```bash
# Ponytail (đã có trong available skills)
skill ponytail

# UI UX Pro Max (đã có trong available skills)
skill ui-ux-pro-max

# gstack (đã có trong available skills)
skill gstack

# Developer Icons — dùng trực tiếp từ thư mục
# Copy icon SVG cần dùng từ ShipFoodCore/Skills/developer-icons-main/icons/ vào wwwroot/Source/icons/
# Tra cứu icon technology theo tên: c-sharp.svg, bootstrap5.svg, react.svg, v.v.
# Dùng `ls ShipFoodCore/Skills/developer-icons-main/icons/ | grep <từ khóa>` để tìm icon nhanh
```

## 6. 🖼️ Nguồn Ảnh & Tài Nguyên Media

### 📸 Nguồn ảnh được phép sử dụng:

| Nguồn | URL | Loại |
|-------|-----|------|
| **Pexels Videos** | `https://www.pexels.com/videos/` | Video stock miễn phí |
| Unsplash | `https://unsplash.com` | Ảnh stock (đã biết 403 — ưu tiên local) |

**Quy tắc sử dụng ảnh:**
- ❌ KHÔNG dùng link Unsplash trực tiếp trong code (dễ bị 403)
- ✅ Tải ảnh về local: `/Source/images/MonAn/` (món ăn), `/Source/Home/img/` (rest, icons)
- ✅ Fallback khi ảnh lỗi: `onerror="this.src='/Source/Home/img/pizza.jpg'"`
- ✅ Đặt ảnh trong wwwroot để ASP.NET Core serve trực tiếp

**Quy tắc sử dụng SVG icons từ developer-icons-main:**
- ✅ Dùng SVG icons thay Font Awesome khi có thể (tránh bị AdBlock chặn)
- ✅ Copy file `.svg` cần dùng: `cp ShipFoodCore/Skills/developer-icons-main/icons/<icon>.svg ShipFoodCore/wwwroot/Source/icons/`
- ✅ Dùng inline SVG hoặc `<img src="~/Source/icons/<icon>.svg">`
- ✅ Đổi màu SVG: dùng `fill="currentColor"` + CSS `color:` để theme-friendly
- ❌ KHÔNG dùng emoji làm icon cho navigation, buttons, system controls
- ❌ KHÔNG dùng Font Awesome cho logo thương hiệu (dùng SVG để sharp hơn)

## 7. 🌿 Git Branch Management

**Từ nay chỉ giữ 2 branch chính:**

| Branch | Mục đích |
|--------|---------|
| `master` | Production — ổn định, đã deploy |
| `feat/redesign-v2` | Feature development — đang phát triển |

**Quy tắc:**
- ❌ KHÔNG tạo branch tạm thời như `fix/xxx`, `deploy/*`
- ✅ Mọi thay đổi đều làm trên nhánh hiện tại, commit trực tiếp
- ✅ Khi cần thử nghiệm, dùng `feat/redesign-v2`
- ✅ Xóa branch remote không cần thiết ngay sau khi dùng xong
- ✅ Kiểm tra build Docker LOCAL trước khi push nếu thay đổi csproj/Dockerfile

---

## 8. 🧪 E2E Testing Rules

### ⚡ Lightpanda — Ưu tiên cho test đơn giản:

```yaml
lightpanda_rules:
  overview: "Lightpanda = headless browser nhanh hơn Chrome 9x, RAM thấp hơn 16x. Kết nối qua CDP."
  prerequisites:
    - "docker compose up -d (chạy Lightpanda CDP server)"
    - "Config: e2e-tests/lightpanda.config.ts"
    - "Fixture: e2e-tests/fixtures/lightpanda-fixture.ts"
  
  khi_dùng_lightpanda:
    - "Login, redirect, sidebar, search, form validation"
    - "Dashboard load, KPI cards, stats display"
    - "Navigation routing, URL changes"
    - "Healthcheck endpoints, API responses"
    - "Responsive layout, overflow checks"
  
  KHÔNG_dùng_lightpanda:
    - "SignalR WebSocket (TC-6.8/6.9/9.8) — chưa hỗ trợ"
    - "File upload/preview (TC-3.18)"
    - "localStorage/cookie-dependent flows"
    - "QR code rendering, canvas-based tests"
    - "Multi-tab workflows (TC-3.9)"
    - "Download handling (TC-10.9)"
  
  command: "npx playwright test --config=lightpanda.config.ts"
  
  workflow: |
    Step 1: docker compose up -d
    Step 2: Chạy subset test đơn giản bằng Lightpanda (nhanh)
    Step 3: Chạy test nâng cao bằng Chromium (playwright.config.ts)
    Step 4: Tổng hợp kết quả từ cả 2
```

### 📊 E2E Test Strategy:

| Loại test | Browser | Timeout | Khi nào dùng |
|-----------|---------|---------|--------------|
| Smoke / Simple | Lightpanda | 30s | Search, login, sidebar, redirect, KPI |
| Advanced | Chromium | 60s | SignalR, file upload, QR, multi-tab |
| Mobile | Chrome Mobile | 60s | Responsive, viewport, touch events |

### 🔧 Fixture usage:

```typescript
// Lightpanda (nhanh, đơn giản)
import { test, expect } from '../fixtures/lightpanda-fixture';

// Chromium (nâng cao, full feature)
import { test, expect } from '@playwright/test';
```

---

*File này được AI đọc tự động mỗi khi làm việc với dự án. Tuân thủ nghiêm ngặt.*
