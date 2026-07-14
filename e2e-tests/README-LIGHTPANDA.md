# 🚀 Lightpanda Browser — FastShip Integration

> **Lightpanda**: Trình duyệt headless viết bằng Zig, nhanh hơn 9x và ít RAM hơn 16x so với Chromium headless.  
> Tương thích CDP (Chrome DevTools Protocol) → dùng được với Playwright!  
> https://github.com/lightpanda-io/browser

---

## 📋 Mục Lục

1. [Tổng Quan](#1-tổng-quan)
2. [Cài Đặt & Chạy](#2-cài-đặt--chạy)
3. [Phương Thức 1: Playwright qua CDP](#3-phương-thức-1-playwright-qua-cdp)
4. [Phương Thức 2: Lightpanda Agent Mode](#4-phương-thức-2-lightpanda-agent-mode)
5. [Kiến Trúc](#5-kiến-trúc)
6. [So Sánh Hiệu Năng](#6-so-sánh-hiệu-năng)
7. [Troubleshooting](#7-troubleshooting)

---

## 1. Tổng Quan

FastShip tích hợp **Lightpanda Browser** theo 2 cách:

| Phương thức | Mô tả | Dùng khi nào |
|-------------|-------|-------------|
| **Playwright via CDP** | Kết nối Playwright tới Lightpanda server qua Chrome DevTools Protocol | Chạy E2E tests nhanh hơn, tiết kiệm tài nguyên |
| **Lightpanda Agent** | Dùng AI agent (Anthropic/OpenAI/Gemini) để tự động duyệt web | Automation AI-native, tạo PandaScripts |

### Lợi ích

| Metric | Chromium (Playwright) | Lightpanda | Improvement |
|--------|----------------------|------------|-------------|
| Memory (100 pages) | 2GB | 123MB | **~16x less** |
| Execution time | 46s | 5s | **~9x faster** |
| Binary size | ~300MB+ | ~20MB | **~15x smaller** |
| Language | C++ | Zig | Explicit memory control |

---

## 2. Cài Đặt & Chạy

### Yêu cầu

- **Docker Desktop** (Windows/Mac/Linux) — chạy Lightpanda container
- Hoặc **Lightpanda binary** (Linux/Mac native)

### Quick Start

```bash
# 1. Start Lightpanda CDP server
cd e2e-tests
docker compose up -d

# 2. Kiểm tra Lightpanda đã hoạt động
curl http://127.0.0.1:9222/json/version

# 3. Chạy smoke test với Playwright
npx playwright test --config=lightpanda.config.ts examples/lightpanda-smoke.spec.ts

# 4. Xem report
npx playwright show-report playwright-report-lightpanda

# 5. Tắt Lightpanda
docker compose down
```

### Docker Compose

```yaml
# e2e-tests/docker-compose.yml
services:
  lightpanda:
    image: lightpanda/browser:nightly
    ports:
      - '127.0.0.1:9222:9222'
    command: lightpanda serve --host 0.0.0.0 --port 9222 --obey-robots
```

---

## 3. Phương Thức 1: Playwright qua CDP

### Kiến Trúc

```
Playwright Test ──▶ chromium.connectOverCDP("http://127.0.0.1:9222")
                           │
                    ┌──────┴──────┐
                    │  Lightpanda  │
                    │  CDP Server  │
                    │  (Docker)    │
                    └─────────────┘
```

### Custom Fixture

File: `e2e-tests/fixtures/lightpanda-fixture.ts`

```typescript
import { test as base } from '@playwright/test';
import { chromium } from 'playwright';

// Worker-scoped: 1 browser instance cho cả worker
const test = base.extend({
  lightpandaBrowser: [async ({ }, use) => {
    const browser = await chromium.connectOverCDP('http://127.0.0.1:9222');
    await use(browser);
    await browser.close();
  }, { scope: 'worker' }],

  // Test-scoped: page mới mỗi test
  page: async ({ lightpandaBrowser }, use) => {
    const context = lightpandaBrowser.contexts()[0]
      || await lightpandaBrowser.newContext();
    const page = context.pages()[0] || await context.newPage();
    await use(page);
    await page.close();
  },
});
```

### Cấu Hình Playwright

File: `e2e-tests/lightpanda.config.ts`

```typescript
export default defineConfig({
  timeout: 30_000,              // Lightpanda nhanh hơn → timeout ngắn hơn
  fullyParallel: true,          // Lightpanda nhẹ → chạy parallel
  workers: 4,                   // 4 worker cùng lúc
  projects: [
    { name: 'Lightpanda Desktop' }
  ],
});
```

### Chạy Tests

```bash
# Chạy tất cả tests với Lightpanda
npm run test:lightpanda

# Chạy smoke test
npx playwright test --config=lightpanda.config.ts examples/lightpanda-smoke.spec.ts

# Chạy với headed mode (nếu Lightpanda hỗ trợ)
# Chú ý: Lightpanda là headless-only
```

---

## 4. Phương Thức 2: Lightpanda Agent Mode

### Giới Thiệu

Lightpanda Agent dùng AI để điều khiển trình duyệt tự động:
- Mô tả task bằng tiếng Anh → AI tự thực hiện
- Tạo **PandaScript**: deterministic, token-free scripts
- Hỗ trợ: Anthropic, OpenAI, Gemini, Hugging Face, Ollama

### Yêu Cầu

1. **Lightpanda binary** (Linux/Mac) hoặc **Docker với agent mode**
2. **API key** cho AI provider (ANTHROPIC_API_KEY, OPENAI_API_KEY, GEMINI_API_KEY)

### Cài Đặt Lightpanda Binary

```bash
# Linux (x86_64)
curl -L -o lightpanda https://github.com/lightpanda-io/browser/releases/download/nightly/lightpanda-x86_64-linux
chmod a+x ./lightpanda

# macOS (ARM)
curl -L -o lightpanda https://github.com/lightpanda-io/browser/releases/download/nightly/lightpanda-aarch64-macos
chmod a+x ./lightpanda
```

### Sử Dụng Agent

```bash
# Chạy task với Claude (Anthropic)
ANTHROPIC_API_KEY=sk-ant-xxx ./lightpanda agent \
  --task "Go to fastship-web.onrender.com, check if the homepage loads, and tell me what restaurants are available"

# Chạy với Gemini
GEMINI_API_KEY=xxx ./lightpanda agent \
  --provider gemini \
  --task "Search for Pizza on FastShip and count the results"

# Chạy với OpenAI
OPENAI_API_KEY=sk-xxx ./lightpanda agent \
  --provider openai \
  --task "Take a screenshot of the FastShip login page"

# Chạy REPL (không LLM)
./lightpanda agent --no-llm
```

### PandaScript

PandaScript là JavaScript với API browser primitives:

```javascript
// File: lightpanda-agent-demo.js
// Chạy: lightpanda agent lightpanda-agent-demo.js

// PandaScript API
await page.goto('https://fastship-web.onrender.com');
await page.wait(2000);
await page.screenshot('/tmp/fastship-home.png');

// Tương tác với trang
await page.fill('input[type="search"]', 'Pizza');
await page.wait(1000);
const results = await page.text('.search-results');
console.log('Results:', results);
```

---

## 5. Kiến Trúc

```
┌────────────────────────────────────────────────────────┐
│                   e2e-tests/                            │
│                                                         │
│  docker-compose.yml          Lightpanda CDP Server     │
│  ┌───────────────────┐     ┌──────────────────────┐    │
│  │ lightpanda        │────▶│ ws://localhost:9222   │    │
│  │ browser:nightly   │     │ CDP WebSocket        │    │
│  └───────────────────┘     └──────────────────────┘    │
│                                                         │
│  ┌────────────────────────────────────────────────┐    │
│  │ Playwright via CDP                              │    │
│  │                                                 │    │
│  │  lightpanda.config.ts                           │    │
│  │       └── lightpanda-fixture.ts                 │    │
│  │              └── connectOverCDP()               │    │
│  │                     └── E2E Tests               │    │
│  └────────────────────────────────────────────────┘    │
│                                                         │
│  ┌────────────────────────────────────────────────┐    │
│  │ Lightpanda Agent Mode                           │    │
│  │                                                 │    │
│  │  lightpanda agent --task "..."                  │    │
│  │       └── PandaScripts (.js files)              │    │
│  │              └── AI-driven automation            │    │
│  └────────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────────┘
```

---

## 6. So Sánh Hiệu Năng

### Benchmark (933 real web pages, AWS EC2 m5.large)

| Metric | Headless Chrome | Lightpanda | Savings |
|--------|----------------|------------|---------|
| Peak Memory | 2,048 MB | 123 MB | **94% less** |
| Total Time | 46 sec | 5 sec | **89% faster** |
| Per-page Time | ~49ms | ~5ms | **~10x faster** |

### Tác động lên FastShip E2E Tests

| Config | Browser | Timeout | Workers | Expected time (10 tests) |
|--------|---------|---------|---------|-------------------------|
| `playwright.config.ts` | Chromium | 60s | 1 | ~5-8 phút |
| `lightpanda.config.ts` | Lightpanda CDP | 30s | 4 | ~1-2 phút |

---

## 7. Troubleshooting

### Lightpanda container không start

```bash
# Kiểm tra logs
docker compose logs lightpanda

# Kiểm tra image
docker pull lightpanda/browser:nightly

# Kiểm tra port
netstat -an | findstr 9222
```

### CDP connection refused

```bash
# Kiểm tra container đã chạy chưa
docker ps | grep lightpanda

# Kiểm tra CDP endpoint
curl http://127.0.0.1:9222/json/version

# Restart container
docker compose restart lightpanda
```

### Playwright test timeout

```bash
# Lightpanda beta - một số Web APIs chưa hỗ trợ
# Thử tăng timeout trong lightpanda.config.ts
use: {
  navigationTimeout: 30_000,
  actionTimeout: 20_000,
}
```

### Windows + WSL2

Lightpanda không hỗ trợ Windows native. Dùng Docker Desktop:
- Docker Desktop tự động forward port từ WSL2
- `localhost:9222` hoạt động cả từ Windows host

### Lightpanda Beta Limitations

- ⚠️ Đang ở giai đoạn Beta, chưa hỗ trợ 100% Web APIs
- ⚠️ Một số advanced Playwright features có thể không hoạt động
- ⚠️ Không hỗ trợ headed mode (chỉ headless)
- ⚠️ Không có Windows native binary (cần Docker/WSL2)

---

## 📚 Tham Khảo

- [Lightpanda GitHub](https://github.com/lightpanda-io/browser)
- [Lightpanda Documentation](https://lightpanda.io/docs/)
- [Chrome DevTools Protocol](https://chromedevtools.github.io/devtools-protocol/)
- [Playwright CDP API](https://playwright.dev/docs/api/class-browsertype#browser-type-connect-over-cdp)
- [Playwright Custom Fixtures](https://playwright.dev/docs/test-fixtures)
