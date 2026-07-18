# FastShip Development Rules

> **Tự động áp dụng khi làm việc với dự án FastShip (ShipFood)**
> *Bao gồm Self-Enforcement Rules (SER) — AI tự tuân theo, không có ngoại lệ*

---

## 📜 PHẦN 0: SELF-ENFORCEMENT RULES (SER) — LUẬT TỰ TUÂN THEO

> **🔴 Các luật này do AI tự tạo ra và tự tuân theo. KHÔNG NGOẠI LỆ. KHÔNG SKIP.**
> Mỗi lần vi phạm = DỪNG response hiện tại, thông báo cho user, làm lại.

---

### 🔴 SER-1️⃣: TIẾNG VIỆT & GIAO TIẾP

```yaml
vietnamese_rule:
  rule: "Luôn giao tiếp bằng tiếng Việt — TRỪ KHI user hỏi bằng tiếng Anh"
  reason: "User chính là người Việt, dự án Việt Nam"
  exceptions:
    - "Code comments trong file có thể để tiếng Anh (nếu convention hiện tại dùng tiếng Anh)"
    - "Commit messages để tiếng Anh (theo conventional commits)"
  penalty: "Dùng tiếng Anh không cần thiết → VI PHẠM → xin lỗi và viết lại"

confirmation_rule:
  rule: "TRƯỚC KHI LÀM BẤT CỨ ĐIỀU GÌ — phải xác nhận đã hiểu yêu cầu"
  steps:
    1: "Tóm tắt yêu cầu của user bằng 1-2 câu"
    2: "Hỏi: 'Có đúng ý bạn không?'"
    3: "CHỈ KHI user xác nhận → mới bắt đầu làm"
  exception: "User nói rõ 'cứ làm đi' hoặc 'không cần hỏi'"
  penalty: "Làm mà không confirm → VI PHẠM → dừng, hỏi lại"
```

---

### 🔴 SER-2️⃣: ROOT CAUSE TRƯỚC KHI FIX

```yaml
root_cause_rule:
  rule: "KHÔNG BAO GIỜ đề xuất fix khi chưa tìm ra ROOT CAUSE"
  workflow:
    1: "Đọc error message — ghi rõ line, file, error code"
    2: "Reproduce bug — ghi rõ steps trigger"
    3: "Check recent changes — git diff, git log -5"
    4: "Trace data flow — từ input đến output"
    5: "CHỈ SAU ĐÓ mới propose fix"
  red_flags:
    - "Quick fix for now" → VI PHẠM
    - "I think it's X, let me fix" → VI PHẠM
    - "Just try changing X" → VI PHẠM
  penalty: "Fix mà không có root cause → VI PHẠM → load systematic-debugging, làm lại"
```

---

### 🔴 SER-3️⃣: PLAN TRƯỚC KHI CODE

```yaml
plan_before_code:
  rule: "KHÔNG CODE feature mới nếu chưa có PLAN"
  workflow:
    1: "Đọc tài liệu liên quan (Project.md, UI-UX.md, file hiện tại)"
    2: "Spawn file-picker/code-searcher — tìm file liên quan"
    3: "Viết plan ngắn: file nào cần sửa, thay đổi gì, ảnh hưởng gì"
    4: "Dùng write_todos — đánh dấu các bước"
    5: "CHỈ SAU ĐÓ mới code"
  exception: "Fix bug đơn giản (1-2 dòng) hoặc user yêu cầu gấp"
  penalty: "Code feature mà không có plan → VI PHẠM → dừng, viết plan"
```

---

### 🔴 SER-4️⃣: VERIFY TRƯỚC KHI CLAIM

```yaml
verify_before_claim:
  rule: "KHÔNG BAO GIỜ claim hoàn thành nếu chưa VERIFY"
  workflow:
    1: "Xác định lệnh verify — dotnet build, test, v.v."
    2: "CHẠY LỆNH — fresh output, ko dùng kết quả cũ"
    3: "ĐỌC OUTPUT — exit code, error count, warning count"
    4: "CHỈ SAU ĐÓ mới claim hoàn thành"
  forbidden_phrases:
    - "Should work now" → VI PHẠM
    - "Looks correct" → VI PHẠM
    - "I'm confident" → VI PHẠM
    - "Tests pass" (nếu chưa chạy) → VI PHẠM
  penalty: "Claim mà không verify → VI PHẠM → xoá claim, chạy verify"
```

---

### 🔴 SER-5️⃣: TỰ AUDIT MỖI RESPONSE

```yaml
self_audit:
  rule: "CUỐI MỖI RESPONSE — tự audit compliance"
  mode_full:  # Khi response có code changes
    trigger: "Có file modification, addition, deletion"
    checkboxes:
      - "✅ IRON LAW 0: Session init marker còn hạn? (<30 phút)"
      - "✅ IRON LAW 0.5: SKILL/REPO INVENTORY đủ format?"
      - "✅ IRON LAW 1: Tool call đầu = skill?"
      - "✅ IRON LAW 2: Compliance check đã pass?"
      - "✅ IRON LAW 3: Hard gate đã pass?"
      - "✅ IRON LAW 4: Skills/repos đã scan?"
      - "✅ SER-1: Giao tiếp tiếng Việt?"
      - "✅ SER-2: Root cause trước fix? (nếu bug)"
      - "✅ SER-3: Plan trước code? (nếu feature)"
      - "✅ SER-4: Verify trước claim?"
      - "✅ SER-10: Code review spawned?"
  mode_lite:  # Khi chỉ hỏi đáp, discussion
    trigger: "Chỉ trả lời câu hỏi, không code"
    checkboxes:
      - "✅ IRON LAW 0: Session init còn hạn?"
      - "✅ IRON LAW 1: Tool call đầu = skill?"
      - "✅ IRON LAW 2: Compliance pass?"
      - "✅ SER-1: Tiếng Việt?"
  penalty: "Thiếu audit checkbox → VI PHẠM → thêm audit trước khi kết thúc"
```

---

### 🔴 SER-6️⃣: HỌC TỪ SAI LẦM (Không lặp lại)

```yaml
learn_from_mistakes:
  rule: "Khi bị user chỉ ra lỗi — phải GHI NHỚ và KHÔNG LẶP LẠI"
  workflow:
    1: "Thừa nhận lỗi — không biện minh"
    2: "Ghi lại lỗi vào bộ nhớ session: mistake_log = [...]"
    3: "Giải thích tại sao lỗi xảy ra"
    4: "Hứa không lặp lại — và giữ lời"
    5: "Check mistake_log trước mỗi quyết định tương tự"
  mistake_log: []  # Danh sách lỗi đã mắc trong session này
  note: "Không dùng penalty Reset Session vì sẽ xoá luôn mistake_log — gây phản tác dụng"
  penalty: "Lặp lại lỗi cũ → VI PHẠM → thừa nhận, ghi log, user quyết định hướng giải quyết"
```

---

### 🔴 SER-7️⃣: MINIMAL CHANGE — Chỉ thay đổi tối thiểu

```yaml
minimal_change:
  rule: "LUÔN thay đổi ÍT NHẤT có thể — không thêm code vô ích"
  ladder:
    1: "Có thực sự cần thay đổi không? (YAGNI)"
    2: "Đã có trong codebase? Reuse, đừng viết lại"
    3: "Có thể dùng stdlib? Dùng stdlib"
    4: "Có thể 1 dòng? Viết 1 dòng"
    5: "Chỉ code tối thiểu hoạt động được"
  rules:
    - "Không thêm abstraction chưa cần (interface 1 impl, factory 1 product)"
    - "Không thêm dependency mới nếu đã có sẵn"
    - "Không boilerplate 'cho sau này'"
    - "Xoá nhiều hơn thêm"
  penalty: "Thêm code không cần thiết → VI PHẠM → xoá, chỉ giữ tối thiểu"
```

---

### 🔴 SER-8️⃣: ASK BEFORE ASSUME — Khi không chắc thì hỏi

```yaml
ask_before_assume:
  rule: "Khi không chắc về yêu cầu, kiến trúc, hay cách làm — PHẢI HỎI user"
  must_ask_when:
    - "Không hiểu rõ yêu cầu"
    - "Có nhiều cách implement khác nhau"
    - "Thay đổi ảnh hưởng đến nhiều file/module"
    - "Xoá file hoặc component"
    - "Thêm dependency mới"
    - "Thay đổi thiết kế CSDL"
  dont_ask_when:
    - "Việc hiển nhiên (fix typo, đổi màu theo design token)"
    - "User đã nói rõ trước đó"
  penalty: "Tự quyết định khi không chắc → VI PHẠM → hỏi user trước khi tiếp tục"
```

---

### 🔴 SER-9️⃣: TÔN TRỌNG CODEBASE HIỆN TẠI

```yaml
respect_codebase:
  rule: "KHÔNG phá vỡ conventions, kiến trúc, design system hiện tại"
  checks:
    - "Kiểm tra coding convention hiện tại trước khi viết code mới"
    - "Dùng design tokens có sẵn (--fs-*), không hardcode"
    - "Dùng font Inter — không thêm font mới"
    - "Dùng FA5 icons — không thêm icon library mới"
    - "Tuân thủ kiến trúc MVC hiện tại"
    - "Dùng helper/function có sẵn — không viết lại"
  penalty: "Phá vỡ convention → VI PHẠM → sửa lại cho đúng"
```

---

### 🔴 SER-🔟: SPAWN AGENT REVIEW CODE BẮT BUỘC

```yaml
mandatory_code_review:
  rule: "SAU MỌI CODE CHANGE — spawn code-reviewer-deepseek-flash"
  scope:
    - "Feature mới: BẮT BUỘC"
    - "Bug fix: BẮT BUỘC (trừ fix 1-2 dòng quá đơn giản)"
    - "UI change: BẮT BUỘC"
    - "Refactor: BẮT BUỘC"
  penalty: "Không review → VI PHẠM → spawn review ngay, chỉ tiếp tục sau khi review OK"
```

---

### 🔴 SER-1️⃣1️⃣: NGHIÊN CỨU TRƯỚC KHI ĐỀ XUẤT

```yaml
research_before_recommend:
  rule: "KHI ĐỀ XUẤT công nghệ, thư viện, API — phải nghiên cứu trước"
  workflow:
    1: "Kiểm tra xem project đã dùng gì chưa? (Project.md, package.json, .csproj)"
    2: "Nếu chưa có → dùng gravity_index để tìm options"
    3: "Đọc docs của option tốt nhất (researcher-docs)"
    4: "So sánh với codebase convention hiện tại"
    5: "CHỈ SAU ĐÓ mới recommend"
  dont_recommend_from_memory:
    - "Không recommend library chỉ vì biết tên — phải check docs"
    - "Không recommend service chưa verify pricing/tier"
    - "Không recommend nếu đã có sẵn trong project"
  penalty: "Đề xuất từ memory không verify → VI PHẠM → research lại"
```

---

### 🔴 SER-1️⃣2️⃣: DÙNG TOOL ĐÚNG CÁCH

```yaml
tool_usage_guide:
  rule: "CHỌN TOOL PHÙ HỢP với nhu cầu — không lạm dụng 1 tool"
  tool_selection:
    file_picker: "Fuzzy search — tìm file liên quan đến concept/feature (không biết chính xác tên file)"
    code_searcher: "Exact pattern search — tìm function, class, variable, error cụ thể"
    glob: "File name pattern — tìm *.cs, *.css, *test* theo tên"
    read_files: "Đọc nội dung file cụ thể — PHẢI dùng sau khi biết file cần đọc"
    read_subtree: "Xem cấu trúc thư mục — PHẢI dùng trước khi read_files nếu chưa rõ cấu trúc"
    basher: "Chạy terminal command — verify build, test, compliance"
    browser_use: "Test UI thực tế trên browser — verify render, click, form"
    researcher_web: "Research online — tìm thông tin, docs"
    gravity_index: "So sánh services — tìm options, so sánh pricing"
  default_workflow:
    1: "list_directory hoặc glob — explore cấu trúc"
    2: "file_picker — tìm file liên quan"
    3: "read_files — đọc file cần sửa"
    4: "code_searcher — kiểm tra usage pattern"
    5: "str_replace/write_file — thực hiện thay đổi"
    6: "basher — verify build"
    7: "spawn code-reviewer-deepseek-flash — review code"
  penalty: "Dùng sai tool → VI PHẠM → dùng tool đúng"
```

---

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
- Password: Plain-text (so sánh `user.pwd == pwd`, không hash)
- Session JSON trong HttpContext.Session
- Redis distributed cache cho SignalR connection state
- `tbChiTietDonHang.mamon` là FK → `tbBienTheMonAn.id` (không phải `tbMonAn.mamon`)

### 6. Kiến trúc

- ASP.NET Core 8 MVC (không MVC 5)
- Cookie + Session auth (không Identity Framework)
- SignalR 8 cho real-time (12 methods, 5 groups)
- Chart.js cho charts
- Leaflet.js + SignalR cho live tracking
- Gemini AI gemini-3.5-flash (free tier)

### 7. Skill Routing & Usage Rules

**BẮT BUỘC**: Khi nhận task, phải tra cứu bảng sau để biết skill/repo nào cần dùng:

```yaml
skill_routing_table:
  # ─── .agents/skills/ (installed skills) ───
  brainstorming: "Trước mọi creative work — thiết kế UI mới, tính năng mới, refactor lớn"
  ui-ux-pro-max: "Thiết kế giao diện, chọn style, color palette, typography, component styling"
  ponytail: "Tối ưu code, giảm code thừa, refactor — DÙNG TRÊN MỌI CODE TASK"
  gstack: "Audit bảo mật, QA review code changes"
  agent-reach: "Tìm kiếm thông tin web, research API, đọc tài liệu online"
  code-reviewer-deepseek-flash: "Review code SAU KHI thay đổi (bắt buộc)"
  systematic-debugging: "Khi gặp bug, test failure, unexpected behavior"
  
  # ─── ShipFoodCore/Skills/ repos ───
  awesome-claude-design: "Tra cứu 68 DESIGN.md patterns khi thiết kế UI mới — layout, component states, responsive, accessibility"
  developer-icons-main: "Dùng SVG icons thay Font Awesome — tránh AdBlock, sharp hơn, theme-friendly"
  ponytail-main: "Đọc SKILL.md files trong repo để biết ponytail audit commands"
  gstack-main: "Đọc SKILL.md files trong repo để biết gstack workflow"
  agent-reach-main: "Đọc SKILL.md trong repo để biết cách dùng 13+ platforms"
  UI UX data: "Tra cứu color palettes (161 CSV), typography specs (57), brand guidelines"
```

**Luật ưu tiên dùng repo skills:**
```
1. Nếu repo có dữ liệu (CSV, JSON, markdown patterns) → DÙNG NGAY, không tự suy luận
2. Nếu có nhiều repo phù hợp → DÙNG TẤT CẢ, kết hợp thông tin
3. Nếu repo có SKILL.md → ĐỌC skill guide trước khi dùng
4. GHI LOG mỗi lần dùng: "**Skills Repo used**: <tên> | <mục đích>"
```

### 8. Design Rules (from awesome-claude-design)

> Nguồn tham khảo: `ShipFoodCore/Skills/awesome-claude-design/` — 68 DESIGN.md patterns
> Nguồn phụ: `ShipFoodCore/Skills/UI UX/` — 161 color palettes, 57 font pairings, brand guidelines CSV

Khi thiết kế UI mới, LUÔN:
1. Load skill `awesome-claude-design` và `ui-ux-pro-max`
2. Tra cứu DESIGN.md patterns từ `ShipFoodCore/Skills/awesome-claude-design/README.md`
3. Tra cứu color palettes từ `ShipFoodCore/Skills/UI UX/` (161 CSV files)
4. Tuân thủ 9 section sau:

```yaml
design_system_sections:
  1_visual_theme: "Set tone, density, mood — Sweetgreen-inspired, modern, card-based"
  2_color_palette: "CSS variables with semantic names — Primary #3CB815, Secondary #F65005"
  3_typography: "Type scale + Google Fonts fallback — Inter, weight 400/500/600/700"
  4_component_styling: "Buttons, inputs, cards, nav with all states (hover/active/focus/disabled)"
  5_layout_principles: "Spacing scale, grid, whitespace rhythm — 4px base unit"
  6_depth_elevation: "Shadow tokens + surface hierarchy — 3 levels: flat/raised/elevated"
  7_dos_donts: "Guardrails when generating new screens"
  8_responsive: "Breakpoints, touch targets, collapse behavior"
  9_agent_prompt: "Reusable prompts for consistent design across screens"
```

**Design Token Naming Convention:**
```css
:root {
  /* Color */
  --color-primary: #3CB815;
  --color-secondary: #F65005;
  --color-surface-1: #FFFFFF;  /* flat */
  --color-surface-2: #F8F9FA;  /* raised */
  --color-surface-3: #E9ECEF;  /* elevated */
  
  /* Typography */
  --font-family: 'Inter', sans-serif;
  --text-xs: 0.75rem;   /* 12px */
  --text-sm: 0.875rem;  /* 14px */
  --text-base: 1rem;    /* 16px */
  --text-lg: 1.125rem;  /* 18px */
  --text-xl: 1.25rem;   /* 20px */
  
  /* Spacing (4px base) */
  --space-1: 0.25rem;   /* 4px */
  --space-2: 0.5rem;    /* 8px */
  --space-3: 0.75rem;   /* 12px */
  --space-4: 1rem;      /* 16px */
  --space-6: 1.5rem;    /* 24px */
  --space-8: 2rem;      /* 32px */
  
  /* Elevation */
  --shadow-flat: none;
  --shadow-raised: 0 1px 3px rgba(0,0,0,0.12);
  --shadow-elevated: 0 4px 12px rgba(0,0,0,0.15);
  
  /* Border Radius */
  --radius-sm: 6px;
  --radius-md: 12px;
  --radius-lg: 16px;
  --radius-full: 9999px;
}
```

**Component States (bắt buộc cho mọi component):**
| State | Visual | Trigger |
|-------|--------|---------|
| Default | Base style | Initial render |
| Hover | Lighten/darken 10% | `:hover` |
| Active/Pressed | Darken 15% | `:active` |
| Focus | Outline 2px primary | `:focus-visible` |
| Disabled | Opacity 50%, cursor not-allowed | `[disabled]` |
| Loading | Skeleton shimmer | Async operation |

**Layout Principles:**
- Spacing scale: 4px base (4, 8, 12, 16, 24, 32, 48, 64)
- Max content width: 1200px (dashboard), 1440px (marketing)
- Grid: 12 columns, 16px gutters
- White space: generous — breathe, don't cram

**Shadow & Depth:**
- Level 1 (flat): Cards at rest — `shadow-raised`
- Level 2 (raised): Dropdowns, popovers — `shadow-elevated`
- Level 3 (elevated): Modals, dialogs — `shadow-elevated + translateY(-2px)`

**Do's and Don'ts:**
| ✅ DO | ❌ DON'T |
|-------|---------|
| Use CSS variables from design tokens | Hardcode hex colors inline |
| Consistent border-radius per component type | Mix radius values randomly |
| Skeleton loading for async content | Spinner for page content |
| 44×44px touch targets on mobile | Tiny clickable elements |
| Semantic color names (primary, danger) | Raw color names (green, red) |
| Progressive disclosure for complex forms | Dump all fields at once |

### 8. Quy trình làm việc

1. Đọc Project.md, UI-UX.md, Architectural-Solution.md
2. Spawn file-picker/code-searcher để tìm file liên quan
3. Đọc file cần sửa
4. Thực hiện thay đổi tối thiểu
5. Spawn code-reviewer-deepseek-flash để review
6. Commit message bằng tiếng Anh, rõ ràng, prefix theo conventional commits
