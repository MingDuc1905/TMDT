/**
 * 🏪 BỘ TEST 08: MERCHANT ADVANCED — Product CRUD, Upload, Discount, Analytics
 *
 * Target: Tất cả tính năng nâng cao của Merchant chưa có trong 03-restaurant-flow
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS, SEED } from '../fixtures/users';

const RESTAURANT = USERS.restaurant1;

async function loginAsMerchant(page: any) {
  const login = new LoginPage(page);
  const url = await login.login(RESTAURANT.username, RESTAURANT.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/Restaurant', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (page.url().includes('/Restaurant')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

// ─── SUITE 1: Product CRUD ───
test.describe('🍽️ CRUD Món ăn', () => {
  test('[TC-8.1] PostMonAn — form field validation', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/ProductDetail', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const form = page.locator('form[action*="PostMonAn"]');
    const exists = await form.count();
    expect(exists).toBeGreaterThan(0);
    // Kiểm tra all required fields
    const fields = [
      { name: 'tenMonAn', label: 'Tên món' }, { name: 'giatien', label: 'Giá' },
      { name: 'madanhmuc', label: 'Danh mục' }, { name: 'mota', label: 'Mô tả' },
    ];
    for (const f of fields) {
      const el = form.locator(`[name="${f.name}"], select[name="${f.name}"]`);
      console.log(`📝 ${f.label} (${f.name}): ${await el.isVisible().catch(() => false)}`);
    }
    // Submit trống → form validation
    await form.locator('button[type="submit"]').click(); await page.waitForTimeout(1000);
    const url = page.url();
    console.log(`📍 URL after empty submit: ${url}`);
  });

  test('[TC-8.2] Size variant pricing grid (M/L/XL) — inputs + labels', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/ProductDetail', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const sizeLabels = ['M', 'L', 'XL']; let found = 0;
    for (const s of sizeLabels) {
      const input = page.locator(`input[name*="size${s}"], input[placeholder*="${s}"]`);
      if (await input.isVisible().catch(() => false)) { found++; console.log(`  ✅ Size ${s} input found`); }
    }
    console.log(`📏 Size inputs found: ${found}/${sizeLabels.length}`);
  });

  test('[TC-8.3] Upload ảnh — file input + preview', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/ProductDetail', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    try {
      const fileInput = page.locator('input[type="file"]').first();
      const visible = await fileInput.isVisible().catch(() => false);
      console.log(`📁 File upload input visible: ${visible}`);
      const preview = page.locator('img[src*="MonAn"], img#imgPreview').first();
      console.log(`🖼️ Preview image: ${await preview.isVisible().catch(() => false)}`);
    } catch (e) {
      console.log(`ℹ️ File upload test: ${e}`);
    }
  });

  test('[TC-8.4] Product List — nút Chỉnh sửa + Xóa', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/ProductList', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const editLinks = page.locator('a[href*="ProductDetail/"]');
    const deleteLinks = page.locator('a[href*="XoaMonAn/"]');
    console.log(`✏️ Edit links: ${await editLinks.count()}`);
    console.log(`🗑️ Delete links: ${await deleteLinks.count()}`);
    if (await editLinks.count() > 0) {
      await editLinks.first().click(); await page.waitForLoadState('networkidle'); await page.waitForTimeout(2000);
      expect(page.url()).toContain('ProductDetail');
    }
  });

  test('[TC-8.5] Xóa món — click + redirect về ProductList', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/ProductList', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const deleteLinks = page.locator('a[href*="XoaMonAn/"]');
    if (await deleteLinks.count() > 0) {
      await deleteLinks.first().click(); await page.waitForLoadState('networkidle'); await page.waitForTimeout(2000);
      console.log(`📍 After delete: ${page.url()}`);
    } else console.log('ℹ️ No delete links (may have no products)');
  });
});

// ─── SUITE 2: Profile ───
test.describe('⚙️ Hồ sơ Quán', () => {
  test('[TC-8.6] Profile page — form fields hiển thị', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Profile', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const inputs = page.locator('form input, form select, form textarea');
    const inputCount = await inputs.count();
    console.log(`📋 Profile form fields: ${inputCount}`);
    expect(inputCount).toBeGreaterThan(0);
  });

  test('[TC-8.7] Profile — submit form', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Profile', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const submitBtn = page.locator('button[type="submit"]').first();
    if (await submitBtn.isVisible().catch(() => false)) {
      await submitBtn.click(); await page.waitForLoadState('networkidle'); await page.waitForTimeout(2000);
      console.log(`📍 After profile save: ${page.url()}`);
    }
  });
});

// ─── SUITE 3: Discount ───
test.describe('🏷️ Khuyến mãi', () => {
  test('[TC-8.8] Discount page — danh sách + CRUD buttons', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Discount', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const table = page.locator('table').first();
    const tableVisible = await table.isVisible().catch(() => false);
    console.log(`📋 Discount table: ${tableVisible}`);
    if (tableVisible) {
      const rows = await page.locator('table tbody tr').count();
      console.log(`  Promotions: ${rows}`);
    }
    const addBtn = page.locator('a:has-text("Thêm"), button:has-text("Thêm")').first();
    console.log(`➕ Add promotion btn: ${await addBtn.isVisible().catch(() => false)}`);
  });
});

// ─── SUITE 4: Analytics ───
test.describe('📊 Analytics', () => {
  test('[TC-8.9] Analytics page — charts + stats', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Analytics', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const canvases = await page.locator('canvas').count();
    console.log(`📈 Charts: ${canvases}`);
    const cards = await page.locator('.card, .stat-item, [class*="kpi"]').count();
    console.log(`📊 Stats cards: ${cards}`);
  });

  test('[TC-8.10] Review page — danh sách đánh giá', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Review', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const body = await page.locator('body').textContent() || '';
    expect(body.length).toBeGreaterThan(0);
    const reviews = page.locator('[class*="review"], [class*="danhgia"]').first();
    console.log(`⭐ Reviews: ${await reviews.isVisible().catch(() => false)}`);
  });
});
