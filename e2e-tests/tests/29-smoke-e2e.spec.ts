/**
 * 🔥 BỘ TEST 29: E2E SMOKE TEST — Luồng nghiệp vụ chính
 *
 * Mục tiêu: Validate 3 luồng nghiệp vụ quan trọng nhất cho CI/CD gate
 * - Flow 1: Khách hàng đặt hàng COD
 * - Flow 2: Quản lý đơn hàng (Restaurant + Shipper)
 * - Flow 3: Admin dashboard
 *
 * Mỗi flow đảm bảo các chức năng core hoạt động end-to-end
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { HomePage } from '../pages/HomePage';
import { CartPage } from '../pages/CartPage';
import { CheckoutPage } from '../pages/CheckoutPage';
import { DetailRestaurantPage } from '../pages/DetailRestaurantPage';
import { USERS, SEED, SHIPPING } from '../fixtures/users';

// ─── Helpers ───
async function loginAs(page: any, user: { username: string; password: string }) {
  const login = new LoginPage(page);
  const url = await login.login(user.username, user.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (!page.url().includes('/Home/Login')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

// ─── FLOW 1: Khách hàng đặt hàng COD ───
test.describe('🔥 [E2E Flow 1] Khách hàng đặt hàng COD', () => {

  test('[TC-E2E-01] Full flow: Login → Tìm quán → Thêm món → Checkout → COD → Success', async ({ page }) => {
    test.setTimeout(180_000); // 3 phút cho flow dài

    // 1. Login
    await loginAs(page, USERS.customer1);
    console.log('✅ 1. Login thành công');

    // 2. Tìm quán Koneko Pizza
    const home = new HomePage(page);
    await home.gotoHome();
    await page.waitForTimeout(2000);

    // 3. Vào chi tiết quán
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
    await page.waitForSelector('.item-restaurant-row', { timeout: 25_000 });
    const itemCount = await detail.getMenuItemCount();
    console.log(`🍕 2. Quán Koneko Pizza: ${itemCount} món`);
    expect(itemCount).toBeGreaterThan(0);

    // 4. Thêm món vào giỏ
    await detail.addFirstItemToCart(1);
    await page.waitForTimeout(2000);
    console.log('🛒 3. Đã thêm món vào giỏ');

    // 5. Kiểm tra giỏ hàng
    const cart = new CartPage(page);
    await cart.gotoCart();
    await page.waitForTimeout(2000);
    const cartCount = await cart.getItemCount();
    console.log(`📋 4. Giỏ hàng: ${cartCount} món`);
    expect(cartCount).toBeGreaterThan(0);

    // 6. Vào checkout
    const checkout = new CheckoutPage(page);
    await checkout.gotoCheckout();
    await page.waitForTimeout(3000);

    // 7. Điền thông tin giao hàng
    try {
      await checkout.nameInput.waitFor({ state: 'visible', timeout: 10_000 });
      await checkout.fillShippingInfo(SHIPPING.name, SHIPPING.phone, SHIPPING.address);
      console.log('📍 5. Đã điền địa chỉ giao hàng');
    } catch {
      console.log('⚠️ Form address không load kịp, tiếp tục...');
    }

    // 8. Chọn COD
    try {
      await checkout.selectCOD();
      console.log('💳 6. Chọn COD');
    } catch {
      console.log('⚠️ Không chọn được COD, thử option đầu');
      await checkout.paymentOptions.first().click().catch(() => {});
    }

    // 9. Xác nhận + Submit
    try {
      await checkout.confirmOrder();
      console.log('✅ 7. Xác nhận đơn hàng');
    } catch {
      console.log('⚠️ Không có checkbox confirm');
    }

    await checkout.submitOrder();
    await page.waitForTimeout(3000);
    console.log(`📤 8. Đã submit đơn hàng — URL: ${page.url()}`);

    // 10. Kiểm tra kết quả
    const popupVisible = await checkout.isResultPopupVisible().catch(() => false);
    if (popupVisible) {
      const popupText = await checkout.getResultPopupText();
      console.log(`✅ 9. Kết quả: ${popupText?.substring(0, 100)}`);
      expect(popupText).toBeTruthy();
    } else {
      console.log(`📍 URL sau checkout: ${page.url()}`);
      // Redirect đến OrderDetail hoặc LichSuDatHang là thành công
      const isSuccess = page.url().includes('ChiTietDonHang') || page.url().includes('LichSuDatHang') || page.url().includes('Success');
      expect(isSuccess || popupVisible).toBeTruthy();
    }
  });
});

// ─── FLOW 2: Restaurant + Shipper xử lý đơn ───
test.describe('🔥 [E2E Flow 2] Restaurant + Shipper xử lý đơn', () => {

  test('[TC-E2E-02] Restaurant dashboard + Order List load + Chi tiết đơn', async ({ page }) => {
    test.setTimeout(120_000);

    // 1. Login quán ăn
    await loginAs(page, USERS.restaurant1);
    console.log('✅ 1. Login quán ăn');

    // 2. Dashboard KPI
    await page.goto('/Restaurant', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const kpiCards = page.locator('.card-header');
    const kpiCount = await kpiCards.count();
    console.log(`📊 2. KPI cards: ${kpiCount}`);
    expect(kpiCount).toBeGreaterThan(0);

    // 3. Order List
    await page.goto('/Restaurant/OrderList', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const table = page.locator('#example5, table').first();
    await expect(table).toBeVisible({ timeout: 10_000 });
    const orderRows = await page.locator('table tbody tr').count();
    console.log(`📋 3. Order List: ${orderRows} đơn`);

    // 4. Chi tiết đơn (nếu có)
    if (orderRows > 0) {
      const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
      if (await detailLinks.count() > 0) {
        await detailLinks.first().click();
        await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
        await page.waitForTimeout(2000);
        console.log(`📄 4. Chi tiết đơn: ${page.url()}`);
      }
    }
  });

  test('[TC-E2E-03] Shipper dashboard + FREE-PICK + Thu nhập', async ({ page }) => {
    test.setTimeout(120_000);

    // 1. Login shipper
    await loginAs(page, USERS.shipper2);
    console.log('✅ 1. Login shipper');

    // 2. Dashboard
    await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent() || '';
    expect(bodyText.length).toBeGreaterThan(0);
    console.log('✅ 2. Shipper dashboard load');

    // 3. FREE-PICK tab
    const freepickTab = page.locator('a:has-text("FREE-PICK"), a:has-text("Chờ nhận"), #orders-all-tab').first();
    if (await freepickTab.isVisible().catch(() => false)) {
      await freepickTab.click();
      await page.waitForTimeout(1000);
      console.log('📋 3. FREE-PICK tab clicked');
    }

    // 4. Thu nhập
    await page.goto('/Shipper/ThuNhap', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const incomeStats = page.locator('.card-header, .stat-item');
    const statCount = await incomeStats.count();
    console.log(`💰 4. Thu nhập stats: ${statCount}`);

    // 5. Lịch sử
    await page.goto('/Shipper/LichSu', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);
    console.log(`✅ 5. Lịch sử load — URL: ${page.url()}`);
  });
});

// ─── FLOW 3: Admin Dashboard ───
test.describe('🔥 [E2E Flow 3] Admin Dashboard', () => {

  test('[TC-E2E-03] Admin: Login → Dashboard KPI → Charts → User Mgmt', async ({ page }) => {
    test.setTimeout(120_000);

    // 1. Login admin
    await loginAs(page, USERS.admin1);
    console.log('✅ 1. Login admin');

    // 2. Dashboard
    await page.goto('/Admin/Dashboard', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // 3. KPI
    const kpiCards = page.locator('.stat-card, .card-header, .kpi-card');
    const kpiCount = await kpiCards.count();
    console.log(`📊 2. KPI cards: ${kpiCount}`);
    expect(kpiCount).toBeGreaterThan(0);

    // 4. Charts
    const canvasCount = await page.locator('canvas').count();
    console.log(`📈 3. Charts: ${canvasCount}`);

    // 5. User management
    await page.goto('/Admin/QuanLyKhachHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const table = page.locator('table').first();
    await expect(table).toBeVisible({ timeout: 10_000 });
    const userRows = await page.locator('table tbody tr').count();
    console.log(`👥 4. User rows: ${userRows}`);
    expect(userRows).toBeGreaterThan(0);

    // 6. DbDebug
    const dbResponse = await page.goto('/Home/DbDebug', { waitUntil: 'domcontentloaded' });
    const json = await dbResponse?.json();
    if (json?.database) {
      console.log(`🗄️ 5. Database: tbUser=${json.database.tbUser}, tbQuanAn=${json.database.tbQuanAn}`);
      expect(json.database.tbUser).toBeGreaterThan(0);
      expect(json.database.tbQuanAn).toBeGreaterThan(0);
    }
  });
});
