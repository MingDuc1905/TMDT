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
