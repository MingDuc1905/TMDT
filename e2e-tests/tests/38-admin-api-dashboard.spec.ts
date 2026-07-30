/**
 * 📊 BỘ TEST 38: ADMIN DASHBOARD JSON API ENDPOINTS
 *
 * Mục tiêu: Test 8+ JSON API endpoints của Admin Dashboard
 * - GetDashboardStats, GetRevenueChart, GetTopRestaurants
 * - GetOrderStatusPie, GetTopItems, GetSystemStats
 * - GetCategoryStats, GetHourlyOrderStats
 * - GetActiveCoupons, MockPaymentWebhook
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS } from '../fixtures/users';

const ADMIN = USERS.admin1;

async function loginAsAdmin(page: any) {
  const login = new LoginPage(page);
  const url = await login.login(ADMIN.username, ADMIN.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/Admin', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (page.url().includes('/Admin')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

test.describe('📊 Admin Dashboard JSON APIs', () => {

  test('[TC-API-01] GetDashboardStats — trả về stats + date filter', async ({ page }) => {
    await loginAsAdmin(page);

    const resp = await page.request.get('/Admin/GetDashboardStats', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 GetDashboardStats: ${JSON.stringify(json).substring(0, 200)}`);
    expect(json).toBeDefined();

    // Test với date filter
    const respFiltered = await page.request.get('/Admin/GetDashboardStats?fromDate=2026-07-01&toDate=2026-07-29', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const jsonFiltered = await respFiltered.json();
    console.log(`  With date filter: ${JSON.stringify(jsonFiltered).substring(0, 150)}`);
  });

  test('[TC-API-02] GetRevenueChart — dữ liệu biểu đồ', async ({ page }) => {
    await loginAsAdmin(page);

    const resp = await page.request.get('/Admin/GetRevenueChart?fromDate=2026-07-01&toDate=2026-07-29', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 GetRevenueChart: ${JSON.stringify(json).substring(0, 200)}`);
    expect(json).toBeDefined();
  });

  test('[TC-API-03] GetTopRestaurants — top quán ăn', async ({ page }) => {
    await loginAsAdmin(page);

    const resp = await page.request.get('/Admin/GetTopRestaurants?fromDate=2026-07-01&toDate=2026-07-29', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 GetTopRestaurants: ${JSON.stringify(json).substring(0, 200)}`);
    expect(json).toBeDefined();
  });

  test('[TC-API-04] GetOrderStatusPie — pie chart data', async ({ page }) => {
    await loginAsAdmin(page);

    const resp = await page.request.get('/Admin/GetOrderStatusPie?fromDate=2026-07-01&toDate=2026-07-29', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 GetOrderStatusPie: ${JSON.stringify(json).substring(0, 200)}`);
    expect(json).toBeDefined();
  });

  test('[TC-API-05] GetTopItems — top món bán chạy', async ({ page }) => {
    await loginAsAdmin(page);

    const resp = await page.request.get('/Admin/GetTopItems?fromDate=2026-07-01&toDate=2026-07-29', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 GetTopItems: ${JSON.stringify(json).substring(0, 200)}`);
    expect(json).toBeDefined();
  });

  test('[TC-API-06] GetSystemStats — thống kê hệ thống', async ({ page }) => {
    await loginAsAdmin(page);

    const resp = await page.request.get('/Admin/GetSystemStats', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 GetSystemStats: ${JSON.stringify(json).substring(0, 200)}`);
    expect(json).toBeDefined();
  });

  test('[TC-API-07] GetCategoryStats + GetHourlyOrderStats', async ({ page }) => {
    await loginAsAdmin(page);

    const resp1 = await page.request.get('/Admin/GetCategoryStats?fromDate=2026-07-01&toDate=2026-07-29', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json1 = await resp1.json();
    console.log(`📡 GetCategoryStats: ${JSON.stringify(json1).substring(0, 150)}`);
    expect(json1).toBeDefined();

    const resp2 = await page.request.get('/Admin/GetHourlyOrderStats', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json2 = await resp2.json();
    console.log(`📡 GetHourlyOrderStats: ${JSON.stringify(json2).substring(0, 150)}`);
    expect(json2).toBeDefined();
  });

  test('[TC-API-08] GetActiveCoupons + MockPaymentWebhook', async ({ page }) => {
    await loginAsAdmin(page);

    const resp1 = await page.request.get('/Admin/GetActiveCoupons', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json1 = await resp1.json();
    console.log(`📡 ActiveCoupons: ${JSON.stringify(json1).substring(0, 150)}`);
    expect(json1).toBeDefined();

    const resp2 = await page.request.post('/Admin/MockPaymentWebhook?madh=99999', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json2 = await resp2.json();
    console.log(`📡 MockPayment: ${JSON.stringify(json2).substring(0, 150)}`);
    expect(json2).toBeDefined();
  });
});
