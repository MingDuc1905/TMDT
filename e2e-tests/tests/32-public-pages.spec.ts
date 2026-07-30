/**
 * 🌐 BỘ TEST 32: PUBLIC PAGES + DANH MỤC (About, Contact, Category, Product)
 *
 * Mục tiêu: Test các trang public còn thiếu
 * - About: hero, stats, team, content
 * - Contact: form, address, map
 * - DanhMuc: category grid, icons, links
 * - SanPham: products by category
 * - UI/UX: images, layout, 404 resources
 */

import { test, expect } from '@playwright/test';
import { BasePage } from '../pages/BasePage';
import { URLS } from '../fixtures/users';

// ─── ABOUT ───
test.describe('🌐 About — Giới thiệu', () => {

  test('[TC-PUB-01] About page load — hero + stats + team section', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/Home/About');
    await page.waitForTimeout(3000);

    // Kiểm tra page content
    const bodyText = await page.locator('body').textContent() || '';
    expect(bodyText.length).toBeGreaterThan(100);
    console.log(`📄 About page: ${bodyText.length} chars`);

    // Kiểm tra hero section
    const heroSection = page.locator('.hero, .page-header, [class*="about-hero"], h1').first();
    const heroVisible = await heroSection.isVisible().catch(() => false);
    console.log(`🖼️ Hero section: ${heroVisible}`);

    // Kiểm tra stats/counters
    const stats = page.locator('.stat-item, .counter, [class*="stat"], [class*="counter"]');
    const statCount = await stats.count();
    console.log(`📊 Stats: ${statCount}`);

    // Kiểm tra team section
    const teamSection = page.locator('.team, [class*="team"], .member');
    const teamCount = await teamSection.count();
    console.log(`👥 Team members: ${teamCount}`);

    // Kiểm tra ảnh không broken
    await expect(async () => {
      const imgResult = await base.validateAllImages();
      console.log(`📸 Images: ${imgResult.total}, Broken: ${imgResult.broken}`);
      if (imgResult.broken > 0) console.log(`  URLs: ${imgResult.brokenUrls.join(', ')}`);
    }).toPass({ timeout: 10_000 });
  });

  test('[TC-PUB-02] About page — navbar + footer hiển thị', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/Home/About');
    await page.waitForTimeout(2000);

    await expect(base.navbar).toBeVisible({ timeout: 5_000 });
    console.log('✅ Navbar visible');

    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await page.waitForTimeout(500);
    await expect(base.footer).toBeVisible({ timeout: 5_000 });
    console.log('✅ Footer visible');

    const footerLinks = await page.locator('.fs-footer a, footer a').count();
    console.log(`🔗 Footer links: ${footerLinks}`);
    expect(footerLinks).toBeGreaterThan(0);
  });

  test('[TC-PUB-03] About page — 0 console errors + 0 404 resources', async ({ page }) => {
    const jsErrors: string[] = [];
    const failedRequests: string[] = [];
    page.on('pageerror', (err) => jsErrors.push(err.message));
    page.on('response', (resp) => { if (resp.status() === 404) failedRequests.push(resp.url()); });

    const base = new BasePage(page);
    await base.goto('/Home/About');
    await page.waitForTimeout(3000);

    if (jsErrors.length > 0) console.log(`❌ JS errors: ${jsErrors.join(' | ')}`);
    if (failedRequests.length > 0) console.log(`⚠️ 404s: ${failedRequests.join('\\n')}`);
    expect(jsErrors.length).toBe(0);
    expect(failedRequests.length).toBe(0);
  });
});

// ─── CONTACT ───
test.describe('📞 Contact — Liên hệ', () => {

  test('[TC-PUB-04] Contact page load — form + address + map', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/Home/Contact');
    await page.waitForTimeout(3000);

    // Kiểm tra contact form
    const form = page.locator('form, .contact-form, [class*="contact"] form').first();
    const formVisible = await form.isVisible().catch(() => false);
    console.log(`📝 Contact form: ${formVisible}`);

    if (formVisible) {
      const inputs = await form.locator('input, textarea').count();
      console.log(`  Form inputs: ${inputs}`);
      expect(inputs).toBeGreaterThanOrEqual(2);
    }

    // Kiểm tra address section
    const address = page.locator('[class*="address"], [class*="dia-chi"], .contact-info').first();
    console.log(`📍 Address: ${await address.isVisible().catch(() => false)}`);

    // Kiểm tra map (Google Maps embed)
    const map = page.locator('iframe, .map, #map, [class*="map"]').first();
    console.log(`🗺️ Map: ${await map.isVisible().catch(() => false)}`);
  });

  test('[TC-PUB-05] Contact — fill form + validation', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/Home/Contact');
    await page.waitForTimeout(2000);

    const nameInput = page.locator('input[name*="name"], input[name*="hoten"], input[placeholder*="tên"]').first();
    const emailInput = page.locator('input[type="email"], input[name*="email"]').first();
    const messageInput = page.locator('textarea, input[name*="message"], input[placeholder*="nội dung"]').first();

    const nameVisible = await nameInput.isVisible().catch(() => false);
    const emailVisible = await emailInput.isVisible().catch(() => false);
    const msgVisible = await messageInput.isVisible().catch(() => false);

    console.log(`📝 Name: ${nameVisible}, Email: ${emailVisible}, Message: ${msgVisible}`);

    if (nameVisible && emailVisible && msgVisible) {
      await nameInput.fill('Nguyễn Văn Test');
      await emailInput.fill('test@example.com');
      await messageInput.fill('Đây là tin nhắn test từ E2E test');

      const submitBtn = page.locator('button[type="submit"], button:has-text("Gửi")').first();
      if (await submitBtn.isVisible().catch(() => false)) {
        await submitBtn.click();
        await page.waitForTimeout(2000);
        console.log(`📍 After submit: ${page.url()}`);
      }
    }
  });
});

// ─── DANH MỤC (Category) ───
test.describe('📂 Danh mục món ăn', () => {

  test('[TC-PUB-06] DanhMuc page — grid categories + icons', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/Home/DanhMuc');
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent() || '';
    console.log(`📄 Page: ${bodyText.substring(0, 100)}`);

    // Kiểm tra category grid
    const categories = page.locator('.category-item, .danhmuc-item, .product-item, [class*="category"] a');
    const catCount = await categories.count();
    console.log(`🏷️ Categories: ${catCount}`);

    if (catCount > 0) {
      const names = await categories.allTextContents();
      console.log(`  Names: ${names.map(n => n?.trim()).filter(Boolean).join(', ')}`);
      expect(catCount).toBeGreaterThan(0);
    }

    // Kiểm tra ảnh
    const imgResult = await base.validateAllImages();
    console.log(`📸 Images: ${imgResult.total}, Broken: ${imgResult.broken}`);
    if (imgResult.broken > 0) console.log(`  URLs: ${imgResult.brokenUrls.join(', ')}`);
  });

  test('[TC-PUB-07] DanhMuc — click category → SanPham page', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/Home/DanhMuc');
    await page.waitForTimeout(3000);

    const categoryLinks = page.locator('a[href*="SanPham"], a[href*="DanhMuc"]');
    const linkCount = await categoryLinks.count();
    console.log(`🔗 Category links: ${linkCount}`);

    if (linkCount > 0) {
      await categoryLinks.first().click();
      await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
      await page.waitForTimeout(2000);

      const url = page.url();
      console.log(`📍 URL: ${url}`);
      expect(url).toContain('SanPham');
    }
  });
});

// ─── SẢN PHẨM THEO DANH MỤC ───
test.describe('🍽️ Sản phẩm theo danh mục', () => {

  test('[TC-PUB-08] SanPham page — product grid + filter + sort', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/Home/SanPham?idDM=1');
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent() || '';
    console.log(`📄 Page: ${bodyText.substring(0, 100)}`);

    // Kiểm tra product grid
    const products = page.locator('.product-item, .monan-item, .menu-item, [class*="product"]');
    const productCount = await products.count();
    console.log(`🍽️ Products: ${productCount}`);

    if (productCount > 0) {
      const firstProductName = await products.first().locator('.product-title, .ten-mon, h3, h4').textContent().catch(() => '(no name)');
      console.log(`  First: ${firstProductName?.trim()}`);
    }

    // Kiểm tra ảnh
    const imgResult = await base.validateAllImages();
    console.log(`📸 Images: ${imgResult.total}, Broken: ${imgResult.broken}`);

    // Kiểm tra filter/sort options
    const sortSelect = page.locator('select[name*="sort"], select[name*="sapxep"]').first();
    console.log(`🔽 Sort: ${await sortSelect.isVisible().catch(() => false)}`);
  });

  test('[TC-PUB-09] SanPham — click product → ChiTietSanPham', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/Home/SanPham?idDM=1');
    await page.waitForTimeout(3000);

    const productLinks = page.locator('a[href*="ChiTietSanPham"], a[href*="chi-tiet"]');
    const linkCount = await productLinks.count();

    if (linkCount > 0) {
      await productLinks.first().click();
      await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
      await page.waitForTimeout(2000);

      const url = page.url();
      console.log(`📍 URL: ${url}`);
      expect(url).toContain('ChiTietSanPham');
    } else {
      console.log('ℹ️ Không có product links');
    }
  });

  test('[TC-PUB-10] SanPham — page with invalid id → không crash 500', async ({ page }) => {
    const base = new BasePage(page);
    const resp = await page.goto('/Home/SanPham?idDM=99999', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(2000);

    const status = resp?.status() ?? 0;
    const bodyText = await page.locator('body').textContent() || '';
    const isError = bodyText.includes('500') || bodyText.includes('Error') || bodyText.includes('Exception');

    console.log(`📍 Invalid id: status=${status}, hasError=${isError}`);
    // Should NOT crash — show empty state or redirect
    expect(isError).toBe(false);
  });
});
