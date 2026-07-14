#!/usr/bin/env node
// ====================================================================
// FastShip + Lightpanda Agent — Demo Script
// ====================================================================
// Lightpanda Agent dùng AI để tự động điều khiển trình duyệt.
// Chạy: lightpanda agent session.js
//
// PandaScript API:
//   page.goto(url, options?)
//   page.click(selector)
//   page.fill(selector, text)
//   page.text(selector)
//   page.html(selector?)
//   page.screenshot(path?)
//   page.wait(ms)
//   page.waitSelector(selector, timeout?)
//   page.evaluate(fn, ...args)
//
// Yêu cầu:
//   1. Lightpanda binary (hoặc Docker CLI)
//   2. API key AI (ANTHROPIC_API_KEY hoặc OPENAI_API_KEY)
//
// Cài đặt Lightpanda trên Linux/Mac:
//   curl -L -o lightpanda https://github.com/lightpanda-io/browser/releases/download/nightly/lightpanda-x86_64-linux
//   chmod a+x ./lightpanda
//
// Chạy với LLM:
//   ANTHROPIC_API_KEY=sk-xxx ./lightpanda agent --task "..." 
//
// Chạy không LLM (REPL mode):
//   ./lightpanda agent --no-llm
//
// Xem thêm: https://github.com/lightpanda-io/browser
// ====================================================================

// ─── FastShip Test: Kiểm tra trang chủ ──────────────────────────────
// Lưu ý: PandaScript chạy trong Lightpanda context, không phải Node.js
// Các API dưới đây là giả lập cho mục đích minh họa.

const TASKS = [
  {
    name: 'Kiểm tra trang chủ FastShip',
    steps: [
      { action: 'goto', url: 'https://fastship-web.onrender.com' },
      { action: 'wait', ms: 2000 },
      { action: 'screenshot', path: '/tmp/fastship-homepage.png' },
      { action: 'log', message: '✅ Homepage loaded' },
    ],
  },
  {
    name: 'Tìm kiếm quán ăn',
    steps: [
      { action: 'goto', url: 'https://fastship-web.onrender.com' },
      { action: 'wait', ms: 1500 },
      { action: 'fill', selector: 'input[type="search"]', value: 'Pizza' },
      { action: 'wait', ms: 1000 },
      { action: 'screenshot', path: '/tmp/fastship-search.png' },
      { action: 'log', message: '✅ Search completed' },
    ],
  },
  {
    name: 'Kiểm tra đăng nhập',
    steps: [
      { action: 'goto', url: 'https://fastship-web.onrender.com/Home/Login' },
      { action: 'wait', ms: 1500 },
      { action: 'screenshot', path: '/tmp/fastship-login.png' },
      { action: 'log', message: '✅ Login page loaded' },
    ],
  },
];

// ─── CLI entry point ────────────────────────────────────────────────
// PandaScript thực tế sẽ chạy trong Lightpanda agent runtime.
// File này là mẫu tham khảo, export dưới dạng module để replay.
if (require.main === module) {
  console.log('🚀 FastShip + Lightpanda Agent Demo');
  console.log(`📋 ${TASKS.length} tasks ready`);
  console.log('');
  console.log('▶ Chạy với Lightpanda:');
  console.log('  ANTHROPIC_API_KEY=sk-xxx lightpanda agent --task "Check FastShip homepage"');
  console.log('');
  console.log('▶ Replay từ file:');
  console.log('  lightpanda agent lightpanda-agent-demo.js');
  console.log('');
  console.log('▶ Hoặc dùng Docker + Playwright CDP:');
  console.log('  docker compose up -d && npx playwright test --config=lightpanda.config.ts');
}

module.exports = { TASKS };
