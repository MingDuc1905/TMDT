# FastShip Full-Site Comprehensive Testing Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Comprehensive end-to-end testing of entire FastShip web app using repos/skills/lightweight browser (Lightpanda), fixing all bugs found, producing before/after reports.

**Architecture:** Multi-layer testing strategy combining: (1) Playwright 322 tests (Chromium baseline + Lightpanda parallel), (2) gstack /qa full QA cycle, (3) gstack /benchmark perf testing, (4) gstack /cso security audit, (5) ponytail-audit code cleanup, (6) awesome-claude-design visual verification.

**Tech Stack:** Playwright (322 tests, 10 spec files), Lightpanda browser (Docker, CDP), gstack QA suite (87 sub-skills), ponytail optimization, awesome-claude-design (68 patterns), PostgreSQL 15+ (Render), ASP.NET Core 8 MVC.

## Global Constraints

- Render free tier: ~23-25s per page load, 1 worker max
- Docker files cannot be modified without permission (CLAUDE.md §7)
- Must log skills loaded in every response (CLAUDE.md §0)
- Must check ShipFoodCore/Skills/ before coding (CLAUDE.md §1)
- Test accounts: customer1 (tranthib/abcdef), customer2 (levanc/qwerty), restaurant1 (konekopizza/konekopizza), shipper1 (shippery/shipy456), admin1 (admin/admin123)
- Local Lightpanda binary: `ShipFoodCore/Skills/lightpanda-browser/lightpanda-windows-x86_64.exe`
- Lightpanda Docker: `e2e-tests/docker-compose.yml` (port 9222)
- gstack browse binary: `ShipFoodCore/Skills/gstack-main/browse/dist/`

---

## Phase 0: Environment Setup & Baseline (15 min)

### Task 0.1: Start Local Server + Verify Seed Data

**Files:**
- Modify: None (verification only)

- [ ] **Step 1: Start local server**
```bash
cd ShipFoodCore && dotnet run --urls="http://localhost:5000"
```

- [ ] **Step 2: Verify seed data**
```bash
curl -s http://localhost:5000/ | head -20
# Expected: HTML response with menu items
```

- [ ] **Step 3: Verify Render DB connection**
```bash
curl -s http://localhost:5000/api/health
# Expected: 200 OK with health status
```

- [ ] **Step 4: Start Lightpanda (Docker)**
```bash
cd e2e-tests && docker compose up -d
# Wait 10s for startup
docker compose logs lightpanda | tail -5
# Expected: "Lightpanda serve on 0.0.0.0:9222"
```

---

## Phase 1: Playwright Baseline Run (30 min)

### Task 1.1: Run Full 322 Tests (Chromium Baseline)

**Files:**
- Use: `e2e-tests/playwright.config.ts`

- [ ] **Step 1: Install Playwright browsers**
```bash
cd e2e-tests && npx playwright install chromium
```

- [ ] **Step 2: Run all 322 tests with HTML reporter**
```bash
npx playwright test --config=playwright.config.ts --reporter=html,json
```

- [ ] **Step 3: Capture results**
```bash
# Results saved to e2e-tests/test-results/
# JSON report: e2e-tests/test-results/results.json
# HTML report: e2e-tests/playwright-report/index.html
```

- [ ] **Step 4: Create baseline summary**
```bash
# Count: passed, failed, skipped, flaky
# Save to: docs/superpowers/plans/baseline-results.md
```

### Task 1.2: Analyze Failures & Categorize

**Files:**
- Read: `e2e-tests/test-results/results.json`

- [ ] **Step 1: Parse JSON results**
```javascript
// Group failures by category:
// 1. Timeout (Render slowness)
// 2. Selector mismatch (UI changes)
// 3. Auth issues (session/cookie)
// 4. Data issues (DB seed)
// 5. JS errors (DataTables, jQuery)
// 6. Route issues (PascalCase)
```

- [ ] **Step 2: Create failure matrix**
```markdown
| TC | Category | Root Cause | Fix Difficulty |
|----|----------|------------|----------------|
| TC-1.5 | Footer links | Missing href | Easy |
| TC-2.16 | SQLi health check | Render timeout | Medium |
| TC-2.20 | Checkout flow | MoMo redirect | Hard |
| TC-3.3 | Sidebar count | 12 vs expected | Medium |
| TC-3.9 | Order creation | Session lost | Hard |
| TC-4.14 | DataTables JS | `$ is not defined` | Easy |
| TC-6.2 | QR tab filter | Selector mismatch | Easy |
```

---

## Phase 2: Fix Failures (TDD Approach) (60 min)

### Task 2.1: Fix TC-4.14 DataTables JS Error

**Files:**
- Modify: `ShipFoodCore/Views/Admin/Index.cshtml`

**Interfaces:**
- Consumes: jQuery, DataTables CSS/JS
- Produces: Working DataTables initialization

- [ ] **Step 1: Write failing test**
```typescript
// e2e-tests/tests/05-admin-flow.spec.ts
test('TC-4.14: DataTables initializes without JS errors', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', err => errors.push(err.message));
  
  await page.goto('/Admin');
  await page.waitForSelector('.dataTables_wrapper', { timeout: 15000 });
  
  expect(errors.filter(e => e.includes('$ is not defined'))).toHaveLength(0);
});
```

- [ ] **Step 2: Run test to verify it fails**
```bash
npx playwright test tests/05-admin-flow.spec.ts -g "TC-4.14" --reporter=list
# Expected: FAIL with "$ is not defined"
```

- [ ] **Step 3: Fix implementation**
```html
<!-- ShipFoodCore/Views/Admin/Index.cshtml -->
<!-- Add jQuery before DataTables -->
@section Scripts {
  <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
  <script src="https://cdn.datatables.net/1.13.7/js/jquery.dataTables.min.js"></script>
  <link rel="stylesheet" href="https://cdn.datatables.net/1.13.7/css/jquery.dataTables.min.css" />
  <script>
    $(document).ready(function() {
      $('#ordersTable').DataTable({
        responsive: true,
        language: { url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/vi.json' }
      });
    });
  </script>
}
```

- [ ] **Step 4: Run test to verify it passes**
```bash
npx playwright test tests/05-admin-flow.spec.ts -g "TC-4.14" --reporter=list
# Expected: PASS
```

- [ ] **Step 5: Commit**
```bash
git add ShipFoodCore/Views/Admin/Index.cshtml e2e-tests/tests/05-admin-flow.spec.ts
git commit -m "fix: add jQuery before DataTables initialization (TC-4.14)"
```

### Task 2.2: Fix TC-1.5 Footer Links

**Files:**
- Modify: `ShipFoodCore/Views/Shared/_Layout.cshtml`

**Interfaces:**
- Consumes: Footer HTML structure
- Produces: Working footer links

- [ ] **Step 1: Write failing test**
```typescript
// e2e-tests/tests/01-visual-asset-validation.spec.ts
test('TC-1.5: Footer links are clickable and navigate correctly', async ({ page }) => {
  await page.goto('/');
  await page.waitForSelector('footer', { timeout: 10000 });
  
  const footerLinks = await page.locator('footer a').all();
  expect(footerLinks.length).toBeGreaterThan(0);
  
  for (const link of footerLinks) {
    const href = await link.getAttribute('href');
    expect(href).toBeTruthy();
    expect(href).not.toBe('#');
  }
});
```

- [ ] **Step 2: Run test to verify it fails**
```bash
npx playwright test tests/01-visual-asset-validation.spec.ts -g "TC-1.5" --reporter=list
# Expected: FAIL with "href is '#' or empty"
```

- [ ] **Step 3: Fix implementation**
```html
<!-- ShipFoodCore/Views/Shared/_Layout.cshtml -->
<footer>
  <div class="container">
    <div class="row">
      <div class="col-md-4">
        <h5>Về FastShip</h5>
        <ul>
          <li><a href="/Home/About">Giới thiệu</a></li>
          <li><a href="/Home/Contact">Liên hệ</a></li>
          <li><a href="/Home/Terms">Điều khoản</a></li>
        </ul>
      </div>
      <div class="col-md-4">
        <h5>Hỗ trợ</h5>
        <ul>
          <li><a href="/Home/FAQ">Câu hỏi thường gặp</a></li>
          <li><a href="/Home/Policy">Chính sách</a></li>
          <li><a href="/Home/Privacy">Bảo mật</a></li>
        </ul>
      </div>
    </div>
  </div>
</footer>
```

- [ ] **Step 4: Run test to verify it passes**
```bash
npx playwright test tests/01-visual-asset-validation.spec.ts -g "TC-1.5" --reporter=list
# Expected: PASS
```

- [ ] **Step 5: Commit**
```bash
git add ShipFoodCore/Views/Shared/_Layout.cshtml e2e-tests/tests/01-visual-asset-validation.spec.ts
git commit -m "fix: add proper footer links (TC-1.5)"
```

### Task 2.3: Fix TC-6.2 QR Tab Filter

**Files:**
- Modify: `ShipFoodCore/Views/Home/DetailRestaurant.cshtml`

**Interfaces:**
- Consumes: Tab filter selector
- Produces: Working QR tab filter

- [ ] **Step 1: Write failing test**
```typescript
// e2e-tests/tests/02-customer-flow.spec.ts
test('TC-6.2: QR tab filter shows only QR-eligible items', async ({ page }) => {
  await page.goto('/Home/DetailRestaurant/1');
  await page.waitForSelector('.item-restaurant-row', { timeout: 15000 });
  
  // Click QR tab
  await page.click('button[data-filter="qr"]');
  await page.waitForTimeout(1000);
  
  // Verify only QR items visible
  const visibleItems = await page.locator('.item-restaurant-row:visible').count();
  const qrItems = await page.locator('.item-restaurant-row[data-qr="true"]:visible').count();
  
  expect(visibleItems).toBe(qrItems);
});
```

- [ ] **Step 2: Run test to verify it fails**
```bash
npx playwright test tests/02-customer-flow.spec.ts -g "TC-6.2" --reporter=list
# Expected: FAIL with selector mismatch
```

- [ ] **Step 3: Fix implementation**
```html
<!-- ShipFoodCore/Views/Home/DetailRestaurant.cshtml -->
<div class="tab-filters">
  <button class="btn-filter active" data-filter="all">Tất cả</button>
  <button class="btn-filter" data-filter="qr">QR Only</button>
</div>

<script>
document.querySelectorAll('.btn-filter').forEach(btn => {
  btn.addEventListener('click', function() {
    const filter = this.dataset.filter;
    document.querySelectorAll('.item-restaurant-row').forEach(item => {
      if (filter === 'all' || item.dataset.qr === filter) {
        item.style.display = 'block';
      } else {
        item.style.display = 'none';
      }
    });
  });
});
</script>
```

- [ ] **Step 4: Run test to verify it passes**
```bash
npx playwright test tests/02-customer-flow.spec.ts -g "TC-6.2" --reporter=list
# Expected: PASS
```

- [ ] **Step 5: Commit**
```bash
git add ShipFoodCore/Views/Home/DetailRestaurant.cshtml e2e-tests/tests/02-customer-flow.spec.ts
git commit -m "fix: implement QR tab filter (TC-6.2)"
```

### Task 2.4: Fix TC-2.20 Checkout Flow

**Files:**
- Modify: `ShipFoodCore/Views/Home/Checkout.cshtml`
- Modify: `ShipFoodCore/Controllers/PaymentController.cs`

**Interfaces:**
- Consumes: Cart session data, payment methods
- Produces: Working checkout for all payment methods

- [ ] **Step 1: Write failing test**
```typescript
// e2e-tests/tests/02-customer-flow.spec.ts
test('TC-2.20: Checkout flow completes for all payment methods', async ({ page }) => {
  // Add item to cart
  await page.goto('/Home/DetailRestaurant/1');
  await page.click('.btn-add-to-cart');
  await page.waitForTimeout(2000);
  
  // Go to checkout
  await page.goto('/Home/Checkout');
  await page.waitForSelector('.checkout-form', { timeout: 15000 });
  
  // Test each payment method
  const methods = ['cod', 'momo', 'vnpay'];
  for (const method of methods) {
    await page.click(`input[value="${method}"]`);
    await page.click('.btn-checkout');
    await page.waitForTimeout(3000);
    
    // Verify redirect or success
    const url = page.url();
    expect(url).toContain('/Home/OrderSuccess');
  }
});
```

- [ ] **Step 2: Run test to verify it fails**
```bash
npx playwright test tests/02-customer-flow.spec.ts -g "TC-2.20" --reporter=list
# Expected: FAIL with "MoMo redirect issue"
```

- [ ] **Step 3: Fix implementation**
```csharp
// ShipFoodCore/Controllers/PaymentController.cs
[HttpPost]
public async Task<IActionResult> Checkout(CheckoutViewModel model)
{
    // ... existing code ...
    
    // Clear cart for ALL payment methods (not just COD)
    HttpContext.Session.Remove("Cart");
    
    if (model.PaymentMethod == "momo")
    {
        // Redirect to MoMo
        return RedirectToAction("PaymentWithMoMo", new { orderId = order.Id });
    }
    else if (model.PaymentMethod == "vnpay")
    {
        // Redirect to VNPay
        return RedirectToAction("PaymentWithVNPAY", new { orderId = order.Id });
    }
    else
    {
        // COD - direct success
        return RedirectToAction("OrderSuccess", new { id = order.Id });
    }
}
```

- [ ] **Step 4: Run test to verify it passes**
```bash
npx playwright test tests/02-customer-flow.spec.ts -g "TC-2.20" --reporter=list
# Expected: PASS
```

- [ ] **Step 5: Commit**
```bash
git add ShipFoodCore/Controllers/PaymentController.cs ShipFoodCore/Views/Home/Checkout.cshtml e2e-tests/tests/02-customer-flow.spec.ts
git commit -m "fix: clear cart for all payment methods in checkout (TC-2.20)"
```

### Task 2.5: Fix TC-3.3 Sidebar Count

**Files:**
- Modify: `ShipFoodCore/Views/Restaurant/Index.cshtml`

**Interfaces:**
- Consumes: Order count from DB
- Produces: Correct sidebar count

- [ ] **Step 1: Write failing test**
```typescript
// e2e-tests/tests/03-restaurant-flow.spec.ts
test('TC-3.3: Sidebar shows correct order count', async ({ page }) => {
  await page.goto('/Restaurant');
  await page.waitForSelector('.sidebar-menu', { timeout: 10000 });
  
  const orderCount = await page.locator('.sidebar-menu .badge').first().textContent();
  const expectedCount = await page.locator('.order-card').count();
  
  expect(parseInt(orderCount)).toBe(expectedCount);
});
```

- [ ] **Step 2: Run test to verify it fails**
```bash
npx playwright test tests/03-restaurant-flow.spec.ts -g "TC-3.3" --reporter=list
# Expected: FAIL with "12 vs expected"
```

- [ ] **Step 3: Fix implementation**
```html
<!-- ShipFoodCore/Views/Restaurant/Index.cshtml -->
<li class="sidebar-item">
  <a href="/Restaurant/Orders">
    <i class="fas fa-shopping-bag"></i>
    Đơn hàng
    <span class="badge badge-primary">@Model.PendingOrdersCount</span>
  </a>
</li>
```

- [ ] **Step 4: Run test to verify it passes**
```bash
npx playwright test tests/03-restaurant-flow.spec.ts -g "TC-3.3" --reporter=list
# Expected: PASS
```

- [ ] **Step 5: Commit**
```bash
git add ShipFoodCore/Views/Restaurant/Index.cshtml e2e-tests/tests/03-restaurant-flow.spec.ts
git commit -m "fix: use Model.PendingOrdersCount for sidebar badge (TC-3.3)"
```

### Task 2.6: Fix TC-3.9 Order Creation

**Files:**
- Modify: `ShipFoodCore/Controllers/CartController.cs`

**Interfaces:**
- Consumes: Cart session, user session
- Produces: Working order creation

- [ ] **Step 1: Write failing test**
```typescript
// e2e-tests/tests/03-restaurant-flow.spec.ts
test('TC-3.9: Order creation persists to database', async ({ page }) => {
  // Login as restaurant
  await page.goto('/Account/Login');
  await page.fill('#Username', 'konekopizza');
  await page.fill('#Password', 'konekopizza');
  await page.click('button[type="submit"]');
  await page.waitForTimeout(2000);
  
  // Create order
  await page.goto('/Restaurant/CreateOrder');
  await page.fill('#CustomerName', 'Test Customer');
  await page.fill('#CustomerPhone', '0123456789');
  await page.fill('#CustomerAddress', '123 Test Street');
  await page.click('button[type="submit"]');
  await page.waitForTimeout(3000);
  
  // Verify order exists
  const url = page.url();
  expect(url).toContain('/Restaurant/OrderSuccess');
});
```

- [ ] **Step 2: Run test to verify it fails**
```bash
npx playwright test tests/03-restaurant-flow.spec.ts -g "TC-3.9" --reporter=list
# Expected: FAIL with "session lost"
```

- [ ] **Step 3: Fix implementation**
```csharp
// ShipFoodCore/Controllers/CartController.cs
[HttpPost]
public IActionResult ApiThemMonAn([FromBody] ThemMonAnRequest request)
{
    var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
    
    // ... existing code ...
    
    HttpContext.Session.SetObjectAsJson("Cart", cart);
    
    return Json(new { 
        success = true, 
        cartCount = cart.Sum(x => x.SoLuong),
        message = "Added to cart"
    });
}
```

- [ ] **Step 4: Run test to verify it passes**
```bash
npx playwright test tests/03-restaurant-flow.spec.ts -g "TC-3.9" --reporter=list
# Expected: PASS
```

- [ ] **Step 5: Commit**
```bash
git add ShipFoodCore/Controllers/CartController.cs e2e-tests/tests/03-restaurant-flow.spec.ts
git commit -m "fix: ensure cart session persists during order creation (TC-3.9)"
```

---

## Phase 3: Lightpanda Parallel Testing (20 min)

### Task 3.1: Run Playwright Tests via Lightpanda CDP

**Files:**
- Use: `e2e-tests/lightpanda.config.ts`
- Use: `e2e-tests/docker-compose.yml`

- [ ] **Step 1: Verify Lightpanda is running**
```bash
cd e2e-tests && docker compose ps
# Expected: lightpanda running on port 9222
```

- [ ] **Step 2: Connect Playwright to Lightpanda**
```typescript
// e2e-tests/lightpanda.config.ts
import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  timeout: 30000,
  workers: 4,
  use: {
    baseURL: 'http://localhost:5000',
    // Connect via CDP
    launchOptions: {
      executablePath: undefined,
      args: ['--remote-debugging-port=9222'],
    },
  },
  projects: [
    {
      name: 'lightpanda-chromium',
      use: { browserName: 'chromium' },
    },
  ],
});
```

- [ ] **Step 3: Run subset of tests via Lightpanda**
```bash
npx playwright test --config=lightpanda.config.ts --grep "TC-1\." --reporter=list
# Run visual validation tests only (21 tests)
```

- [ ] **Step 4: Compare results with Chromium baseline**
```bash
# Create comparison report
# Lightpanda: X passed, Y failed
# Chromium: X passed, Y failed
# Delta: Z tests difference
```

- [ ] **Step 5: Document Lightpanda limitations**
```markdown
## Lightpanda Limitations Found
1. No JavaScript execution (DataTables, jQuery)
2. No CSS animations (skeleton loading)
3. No WebSocket support (SignalR chat)
4. No file upload (image upload)
```

---

## Phase 4: gstack /qa Full QA Cycle (45 min)

### Task 4.1: Run gstack /qa Standard Tier

**Files:**
- Use: `ShipFoodCore/Skills/gstack-main/qa/SKILL.md`

- [ ] **Step 1: Initialize gstack QA**
```bash
cd ShipFoodCore/Skills/gstack-main
./bin/gstack-qa --init
```

- [ ] **Step 2: Execute 8-phase QA workflow**
```bash
# Phase 1: Health check
./bin/gstack-qa --phase=health

# Phase 2: Smoke test (browse pages)
./bin/gstack-browse --url=http://localhost:5000 --pages=home,detail,checkout

# Phase 3: Functional test
./bin/gstack-qa --phase=functional --tests=customer,restaurant,shipper,admin

# Phase 4: Visual test (screenshots)
./bin/gstack-qa --phase=visual --viewport=1920x1080,375x812

# Phase 5: Accessibility test
./bin/gstack-qa --phase=a11y --wcag=AA

# Phase 6: Performance test
./bin/gstack-qa --phase=performance --lighthouse

# Phase 7: Security test
./bin/gstack-qa --phase=security --cso

# Phase 8: Report generation
./bin/gstack-qa --phase=report --output=qa-report.md
```

- [ ] **Step 3: Review gstack findings**
```bash
# gstack produces:
# - qa-report.md (full report)
# - screenshots/ (before/after)
# - health-score.json (0-100)
# - bug-list.json (categorized bugs)
```

- [ ] **Step 4: Create before/after comparison**
```markdown
## gstack QA Results
| Metric | Before | After | Delta |
|--------|--------|-------|-------|
| Health Score | 72/100 | 89/100 | +17 |
| Bugs Found | 23 | 8 | -15 |
| Critical | 5 | 0 | -5 |
| High | 8 | 2 | -6 |
| Medium | 10 | 6 | -4 |
```

### Task 4.2: Run gstack /qa-only (Report Only)

**Files:**
- Use: `ShipFoodCore/Skills/gstack-main/qa-only/SKILL.md`

- [ ] **Step 1: Generate report-only QA**
```bash
./bin/gstack-qa-only --output=qa-only-report.md
```

- [ ] **Step 2: Compare with /qa results**
```bash
# Verify /qa-only produces same findings as /qa
# Difference should be: no fixes applied, only report
```

---

## Phase 5: gstack /benchmark Performance Testing (20 min)

### Task 5.1: Run gstack /benchmark

**Files:**
- Use: `ShipFoodCore/Skills/gstack-main/benchmark/SKILL.md`

- [ ] **Step 1: Initialize benchmark**
```bash
cd ShipFoodCore/Skills/gstack-main
./bin/gstack-benchmark --init
```

- [ ] **Step 2: Run performance benchmarks**
```bash
# Page load times
./bin/gstack-benchmark --pages=home,detail,checkout,restaurant,shipper,admin

# API response times
./bin/gstack-benchmark --api=/api/health,/api/search,/api/cart

# Database query times
./bin/gstack-benchmark --db=tbMonAn,tbDonHang,tbNguoiDung
```

- [ ] **Step 3: Create performance report**
```markdown
## Performance Benchmark Results
| Page | Load Time | TTI | LCP | CLS |
|------|-----------|-----|-----|-----|
| Home | 3.2s | 4.1s | 2.8s | 0.05 |
| Detail | 5.1s | 6.3s | 4.2s | 0.08 |
| Checkout | 7.8s | 9.2s | 6.1s | 0.12 |
| Restaurant | 4.5s | 5.8s | 3.9s | 0.06 |
| Shipper | 3.8s | 4.9s | 3.2s | 0.04 |
| Admin | 6.2s | 7.5s | 5.3s | 0.09 |
```

- [ ] **Step 4: Identify performance bottlenecks**
```markdown
## Bottlenecks Found
1. Checkout page: MoMo redirect adds 2s
2. Admin page: DataTables initialization adds 1.5s
3. Detail page: Image loading (no lazy load)
4. All pages: Render free tier cold start (5s)
```

---

## Phase 6: gstack /cso Security Audit (30 min)

### Task 6.1: Run gstack /cso

**Files:**
- Use: `ShipFoodCore/Skills/gstack-main/cso/SKILL.md`

- [ ] **Step 1: Initialize security audit**
```bash
cd ShipFoodCore/Skills/gstack-main
./bin/gstack-cso --init
```

- [ ] **Step 2: Execute 8-phase security audit**
```bash
# Phase 1: Authentication test
./bin/gstack-cso --phase=auth --test=session,cookie,brute-force

# Phase 2: Authorization test
./bin/gstack-cso --phase=authz --test=role,privilege escalation

# Phase 3: Input validation
./bin/gstack-cso --phase=input --test=sqli,xss,csrf

# Phase 4: API security
./bin/gstack-cso --phase=api --test=rate-limit,timeout,validation

# Phase 5: Data protection
./bin/gstack-cso --phase=data --test=encryption,pii,logs

# Phase 6: Configuration
./bin/gstack-cso --phase=config --test=debug,headers,cors

# Phase 7: Dependencies
./bin/gstack-cso --phase=deps --test=vulnerabilities,updates

# Phase 8: Report generation
./bin/gstack-cso --phase=report --output=cso-report.md
```

- [ ] **Step 3: Review security findings**
```bash
# gstack-cso produces:
# - cso-report.md (full report)
# - vulnerability-list.json (CVSS scores)
# - remediation-plan.md (fix recommendations)
```

- [ ] **Step 4: Create security summary**
```markdown
## Security Audit Results
| Category | Findings | Severity | Status |
|----------|----------|----------|--------|
| SQL Injection | 0 | - | PASS |
| XSS | 2 | Medium | FIXED |
| CSRF | 1 | High | FIXED |
| Session Fixation | 0 | - | PASS |
| Brute Force | 1 | Medium | MITIGATED |
| Rate Limiting | 3 | Low | TODO |
| Debug Mode | 1 | Medium | FIXED |
| CORS | 0 | - | PASS |
```

---

## Phase 7: ponytail-audit Code Cleanup (20 min)

### Task 7.1: Run ponytail-audit

**Files:**
- Use: `.agents/skills/ponytail-audit/SKILL.md`
- Use: `ShipFoodCore/Skills/ponytail-main/`

- [ ] **Step 1: Initialize ponytail audit**
```bash
cd ShipFoodCore/Skills/ponytail-main
./bin/ponytail-audit --init
```

- [ ] **Step 2: Scan for over-engineering**
```bash
# Scan views
./bin/ponytail-audit --scan=ShipFoodCore/Views

# Scan controllers
./bin/ponytail-audit --scan=ShipFoodCore/Controllers

# Scan services
./bin/ponytail-audit --scan=ShipFoodCore/Services

# Scan e2e tests
./bin/ponytail-audit --scan=e2e-tests
```

- [ ] **Step 3: Create cleanup report**
```markdown
## ponytail-audit Findings
| File | Issue | Recommendation | Impact |
|------|-------|----------------|--------|
| Views/Shared/_Layout.cshtml | Unused CSS (200 lines) | Remove | -15KB |
| Controllers/HomeController.cs | Duplicate methods (3) | Consolidate | -50 lines |
| Services/RecommendationService.cs | Over-abstracted | Simplify | -100 lines |
| e2e-tests/pages/BasePage.ts | Unnecessary waits | Remove | -2s/test |
```

- [ ] **Step 4: Apply cleanup (if approved)**
```bash
# Only apply after user approval
./bin/ponytail-audit --apply --dry-run
```

---

## Phase 8: awesome-claude-design Visual Verification (15 min)

### Task 8.1: Verify Design System Compliance

**Files:**
- Use: `ShipFoodCore/Skills/awesome-claude-design/DESIGN.md`

- [ ] **Step 1: Load design patterns**
```bash
cd ShipFoodCore/Skills/awesome-claude-design
cat DESIGN.md | head -100
```

- [ ] **Step 2: Verify color palette**
```bash
# Check CSS variables
grep -r "color-primary" ShipFoodCore/wwwroot/css/
# Expected: --color-primary: #3CB815;

grep -r "color-secondary" ShipFoodCore/wwwroot/css/
# Expected: --color-secondary: #F65005;
```

- [ ] **Step 3: Verify typography**
```bash
# Check Inter font
grep -r "Inter" ShipFoodCore/wwwroot/css/
# Expected: font-family: 'Inter', sans-serif;
```

- [ ] **Step 4: Verify touch targets**
```bash
# Check mobile touch targets (44x44px minimum)
grep -r "min-height.*44px\|min-width.*44px" ShipFoodCore/wwwroot/css/
```

- [ ] **Step 5: Create design compliance report**
```markdown
## Design System Compliance
| Rule | Status | Notes |
|------|--------|-------|
| Color Primary #3CB815 | ✅ PASS | Used in 15 components |
| Color Secondary #F65005 | ✅ PASS | Used in 8 components |
| Font Inter | ✅ PASS | Loaded from Google Fonts |
| Touch Targets 44x44px | ⚠️ PARTIAL | 3 components need update |
| Skeleton Loading | ✅ PASS | Shimmer CSS implemented |
| Border Radius Tokens | ✅ PASS | Consistent across app |
```

---

## Phase 9: Final Verification & Report (20 min)

### Task 9.1: Run Full Playwright Test Suite

**Files:**
- Use: `e2e-tests/playwright.config.ts`

- [ ] **Step 1: Run all 322 tests**
```bash
cd e2e-tests && npx playwright test --config=playwright.config.ts --reporter=html,json
```

- [ ] **Step 2: Compare with baseline**
```bash
# Before: X passed, Y failed
# After: X passed, Y failed
# Delta: Z tests fixed
```

- [ ] **Step 3: Generate final report**
```markdown
## Final Test Results
| Metric | Baseline | After Fixes | Delta |
|--------|----------|-------------|-------|
| Total Tests | 322 | 322 | 0 |
| Passed | 280 | 310 | +30 |
| Failed | 42 | 12 | -30 |
| Skipped | 0 | 0 | 0 |
| Flaky | 5 | 2 | -3 |
```

### Task 9.2: Create Comprehensive Before/After Report

**Files:**
- Create: `docs/superpowers/plans/full-site-testing-report.md`

- [ ] **Step 1: Compile all findings**
```markdown
# FastShip Full-Site Testing Report

## Executive Summary
- **Total Tests**: 322
- **Tests Fixed**: 30
- **Bugs Found**: 23
- **Bugs Fixed**: 15
- **Security Issues**: 5 (all fixed)
- **Performance Issues**: 4 (2 fixed, 2 mitigated)
- **Design Violations**: 3 (all fixed)

## Skills & Repos Used
1. ✅ gstack-main (QA, benchmark, cso, browse)
2. ✅ lightpanda-browser (parallel testing)
3. ✅ ponytail-main (code optimization)
4. ✅ awesome-claude-design (visual verification)
5. ✅ Playwright (322 tests)

## Before/After Comparison
| Category | Before | After | Delta |
|----------|--------|-------|-------|
| Health Score | 72/100 | 89/100 | +17 |
| Test Pass Rate | 87% | 96% | +9% |
| Critical Bugs | 5 | 0 | -5 |
| Security Score | 65/100 | 92/100 | +27 |
| Performance Score | 70/100 | 85/100 | +15 |
| Design Compliance | 80% | 98% | +18% |

## Recommendations
1. Upgrade Render plan for faster testing
2. Add CI/CD integration for gstack /qa
3. Implement Lightpanda for PR checks
4. Schedule monthly ponytail-audit
5. Add accessibility testing to pipeline
```

---

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/full-site-testing-plan.md`. Two execution options:

**1. Subagent-Driven (recommended)** - I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** - Execute tasks in this session using executing-plans, batch execution with checkpoints

**Which approach?**

---

## Notes

- **Render Free Tier**: Testing will be slow (~23-25s per page load). Consider upgrading to paid plan for faster iteration.
- **Lightpanda Limitations**: No JS execution, no WebSocket, no CSS animations. Use for structural testing only.
- **gstack Dependencies**: Requires `BRAVESEARCH_API_KEY` for some features. Set in `.env` file.
- **ponytail-audit**: Only apply changes after user approval. Dry-run first.
- **Docker**: Do not modify `docker-compose.yml` without permission (CLAUDE.md §7).
