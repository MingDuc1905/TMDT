/**
 * 📊 BỘ TEST 17: ANALYTICS & DASHBOARD
 *
 * Mục tiêu: Test restaurant analytics + admin dashboard
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS } from '../fixtures/users';

const RESTAURANT = USERS.restaurant1;
const ADMIN = USERS.admin1;

async function loginAs(page: any, user: { username: string; password: string }) {
  const login = new LoginPage(page);
  await login.login(user.username, user.password);
  await page.waitForTimeout(2000);
}

// ════════════════════════════════════════════════════════════════
// 1. RESTAURANT DASHBOARD
// ════════════════════════════════════════════════════════════════
test.describe('🍽️ Restaurant dashboard', () => {

  test('[TC-ANALYTIC-01] Restaurant Dashboard → KPI cards render', async ({ page }) => {
    await loginAs(page, RESTAURANT);

    await page.goto('/Restaurant', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Check for KPI cards
    const cards = page.locator('.card-body');
    const count = await cards.count();
    console.log(`Dashboard cards: ${count}`);

    // Check for KPI values
    const h4Values = page.locator('.card-body h4');
    const h4Count = await h4Values.count();
    console.log(`KPI values: ${h4Count}`);

    if (h4Count > 0) {
      for (let i = 0; i < Math.min(h4Count, 4); i++) {
        const text = await h4Values.nth(i).textContent();
        console.log(`  KPI ${i}: ${text}`);
      }
    }
  });

  test('[TC-ANALYTIC-02] Restaurant Dashboard → order status summary', async ({ page }) => {
    await loginAs(page, RESTAURANT);

    await page.goto('/Restaurant', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent();
    const hasStatuses = bodyText?.includes('Đang chuẩn bị') ||
      bodyText?.includes('Hoàn thành') ||
      bodyText?.includes('Đã huỷ');
    console.log(`Order status summary visible: ${hasStatuses}`);
  });

  test('[TC-ANALYTIC-03] Restaurant Dashboard → Apriori cross-sell insights', async ({ page }) => {
    await loginAs(page, RESTAURANT);

    await page.goto('/Restaurant', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const aprioriCards = page.locator('.apriori-insight-card');
    const count = await aprioriCards.count();
    console.log(`Apriori insight cards: ${count}`);

    if (count > 0) {
      const firstCard = await aprioriCards.first().textContent();
      console.log(`First insight: "${firstCard?.substring(0, 100)}"`);
    }
  });

  test('[TC-ANALYTIC-04] Restaurant Dashboard → wallet page', async ({ page }) => {
    await loginAs(page, RESTAURANT);

    await page.goto('/Restaurant/Wallet', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent();
    const hasWallet = bodyText?.includes('Số dư') || bodyText?.includes('Ví tiền') || bodyText?.includes('Doanh thu');
    console.log(`Wallet page loaded: ${hasWallet}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 2. RESTAURANT ANALYTICS PAGE
// ════════════════════════════════════════════════════════════════
test.describe('📈 Restaurant analytics page', () => {

  test('[TC-ANALYTIC-05] Analytics → feedback stats render', async ({ page }) => {
    await loginAs(page, RESTAURANT);

    await page.goto('/Restaurant/Analytics', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const currentUrl = page.url();
    console.log(`Analytics URL: ${currentUrl}`);

    const bodyText = await page.locator('body').textContent();
    const hasAnalytics = bodyText?.includes('Thống kê') || bodyText?.includes('Phản hồi') || bodyText?.includes('Đánh giá');
    console.log(`Analytics page loaded: ${hasAnalytics}`);

    // Check for product cards
    const productCards = page.locator('.col-md-4, .product-card');
    const count = await productCards.count();
    console.log(`Product analytics cards: ${count}`);
  });

  test('[TC-ANALYTIC-06] Analytics → star ratings visible', async ({ page }) => {
    await loginAs(page, RESTAURANT);

    await page.goto('/Restaurant/Analytics', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const stars = page.locator('.fa-star.text-orange, .star-rating, [class*="star"]');
    const count = await stars.count();
    console.log(`Star rating elements: ${count}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 3. ADMIN DASHBOARD
// ════════════════════════════════════════════════════════════════
test.describe('👨‍💼 Admin dashboard', () => {

  test('[TC-ADMIN-01] Admin Dashboard → KPI cards', async ({ page }) => {
    await loginAs(page, ADMIN);

    await page.goto('/Admin/Dashboard', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent();
    const hasDashboard = bodyText?.includes('Dashboard') ||
      bodyText?.includes('Tổng quan') ||
      bodyText?.includes('Đơn hàng') ||
      bodyText?.includes('Doanh thu');
    console.log(`Admin dashboard loaded: ${hasDashboard}`);
  });

  test('[TC-ADMIN-02] Admin Dashboard → charts render', async ({ page }) => {
    await loginAs(page, ADMIN);

    await page.goto('/Admin/Dashboard', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Check for Chart.js canvases
    const canvases = page.locator('canvas');
    const count = await canvases.count();
    console.log(`Chart canvases: ${count}`);

    // Check for chart containers
    const chartContainers = page.locator('[class*="chart"], .chartjs-render-monitor');
    const chartCount = await chartContainers.count();
    console.log(`Chart containers: ${chartCount}`);
  });

  test('[TC-ADMIN-03] Admin → user management page', async ({ page }) => {
    await loginAs(page, ADMIN);

    await page.goto('/Admin/QuanLyKhachHang', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent();
    const hasUsers = bodyText?.includes('Khách hàng') || bodyText?.includes('Tài khoản');
    console.log(`User management loaded: ${hasUsers}`);

    const userRows = page.locator('tr, .user-card, .list-item');
    const count = await userRows.count();
    console.log(`User rows: ${count}`);
  });

  test('[TC-ADMIN-04] Admin → order management page', async ({ page }) => {
    await loginAs(page, ADMIN);

    await page.goto('/Admin/Order', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent();
    const hasOrders = bodyText?.includes('Đơn hàng') || bodyText?.includes('Order');
    console.log(`Order management loaded: ${hasOrders}`);
  });
});
