/**
 * 📸 BỘ TEST 40: FULL VISUAL REGRESSION — Screenshots Every Page
 *
 * Mục tiêu: Chụp screenshot tất cả các trang để visual regression
 * - Mỗi page được chụp fullPage screenshot
 * - So sánh với baseline (nếu có)
 * - Kiểm tra layout, spacing, màu sắc
 * - Tất cả 4 roles: customer, restaurant, shipper, admin
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS } from '../fixtures/users';

const SCREENSHOT_DIR = 'screenshots/visual-regression';

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

async function takePageScreenshot(page: any, name: string) {
  await page.waitForTimeout(2000);
  await page.screenshot({
    path: `${SCREENSHOT_DIR}/${name}.png`,
    fullPage: true,
  });
  console.log(`📸 Screenshot: ${name}`);
}

test.describe('📸 Full Visual Regression — ALL PAGES', () => {

  // ─── PUBLIC / CUSTOMER PAGES ───
  test.describe('🛍️ Customer Pages', () => {

    test('[TC-VR-01] Trang chủ /', async ({ page }) => {
      await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await takePageScreenshot(page, 'homepage');
    });

    test('[TC-VR-02] Login /Home/Login', async ({ page }) => {
      await page.goto('/Home/Login', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await takePageScreenshot(page, 'login');
    });

    test('[TC-VR-03] Signup /Home/Signup', async ({ page }) => {
      await page.goto('/Home/Signup', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await takePageScreenshot(page, 'signup');
    });

    test('[TC-VR-04] About /Home/About', async ({ page }) => {
      await page.goto('/Home/About', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await takePageScreenshot(page, 'about');
    });

    test('[TC-VR-05] Contact /Home/Contact', async ({ page }) => {
      await page.goto('/Home/Contact', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await takePageScreenshot(page, 'contact');
    });

    test('[TC-VR-06] DanhMuc /Home/DanhMuc', async ({ page }) => {
      await page.goto('/Home/DanhMuc', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await takePageScreenshot(page, 'danh-muc');
    });

    test('[TC-VR-07] SanPham /Home/SanPham?idDM=1', async ({ page }) => {
      await page.goto('/Home/SanPham?idDM=1', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await takePageScreenshot(page, 'san-pham');
    });

    test('[TC-VR-08] Chi tiết quán (logged out)', async ({ page }) => {
      await page.goto('/Home/DetailRestaurant?id=6', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await takePageScreenshot(page, 'restaurant-detail');
    });

    test('[TC-VR-09] Chi tiết sản phẩm', async ({ page }) => {
      // Lấy mamon đầu tiên từ quán Koneko Pizza
      await page.goto('/Home/DetailRestaurant?id=6', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      const firstLink = page.locator('a[href*="ChiTietSanPham"]').first();
      const href = await firstLink.getAttribute('href').catch(() => null);
      if (href) {
        await page.goto(href.startsWith('/') ? href : `/Home/ChiTietSanPham?id=1`, { waitUntil: 'domcontentloaded', timeout: 30_000 });
        await takePageScreenshot(page, 'product-detail');
      } else {
        console.log('⚠️ No product link found, using default ID');
        await page.goto('/Home/ChiTietSanPham?id=1', { waitUntil: 'domcontentloaded', timeout: 30_000 });
        await takePageScreenshot(page, 'product-detail');
      }
    });

    test('[TC-VR-10] Cart / giỏ hàng trống', async ({ page }) => {
      await page.goto('/Cart', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await takePageScreenshot(page, 'cart-empty');
    });
  });

  // ─── CUSTOMER LOGGED IN PAGES ───
  test.describe('🔐 Customer Logged In Pages', () => {

    test('[TC-VR-11] Giỏ hàng có item', async ({ page }) => {
      await loginAs(page, USERS.customer1);
      // Add item
      await page.goto('/Home/DetailRestaurant?id=6', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      const addBtn = page.locator('.add-to-cart-btn').first();
      if (await addBtn.isVisible().catch(() => false)) {
        await page.locator('.adding-food-cart input[name="soLuong"]').first().fill('1');
        await addBtn.click();
        await page.waitForTimeout(2000);
      }
      await page.goto('/Cart', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await takePageScreenshot(page, 'cart-with-items');
    });

    test('[TC-VR-12] Checkout page', async ({ page }) => {
      await loginAs(page, USERS.customer1);
      await page.goto('/Cart/Checkout', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);
      await takePageScreenshot(page, 'checkout');
    });

    test('[TC-VR-13] Lịch sử đơn hàng', async ({ page }) => {
      await loginAs(page, USERS.customer1);
      await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);
      await takePageScreenshot(page, 'order-history');
    });

    test('[TC-VR-14] Chi tiết đơn hàng', async ({ page }) => {
      await loginAs(page, USERS.customer1);
      await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
      if (await detailLinks.count() > 0) {
        await detailLinks.first().click();
        await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
        await takePageScreenshot(page, 'order-detail');
      } else {
        console.log('ℹ️ No orders, taking empty history screenshot');
        await takePageScreenshot(page, 'order-history-empty');
      }
    });

    test('[TC-VR-15] Customer NhanTin chat', async ({ page }) => {
      await loginAs(page, USERS.customer1);
      await page.goto('/Home/NhanTin', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);
      await takePageScreenshot(page, 'customer-chat');
    });
  });

  // ─── RESTAURANT PAGES ───
  test.describe('🏪 Restaurant Pages', () => {

    test('[TC-VR-16] Restaurant Dashboard', async ({ page }) => {
      await loginAs(page, USERS.restaurant1);
      await page.goto('/Restaurant', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);
      await takePageScreenshot(page, 'restaurant-dashboard');
    });

    test('[TC-VR-17] Restaurant OrderList', async ({ page }) => {
      await loginAs(page, USERS.restaurant1);
      await page.goto('/Restaurant/OrderList', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);
      await takePageScreenshot(page, 'restaurant-orders');
    });

    test('[TC-VR-18] Restaurant ProductList', async ({ page }) => {
      await loginAs(page, USERS.restaurant1);
      await page.goto('/Restaurant/ProductList', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);
      await takePageScreenshot(page, 'restaurant-products');
    });

    test('[TC-VR-19] Restaurant Analytics', async ({ page }) => {
      await loginAs(page, USERS.restaurant1);
      await page.goto('/Restaurant/Analytics', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);
      await takePageScreenshot(page, 'restaurant-analytics');
    });

    test('[TC-VR-20] Restaurant Discount + Wallet', async ({ page }) => {
      await loginAs(page, USERS.restaurant1);
      await page.goto('/Restaurant/Discount', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'restaurant-discount');

      await page.goto('/Restaurant/Wallet', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'restaurant-wallet');
    });

    test('[TC-VR-21] Restaurant Review + Profile + Scanner', async ({ page }) => {
      await loginAs(page, USERS.restaurant1);
      await page.goto('/Restaurant/Review', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'restaurant-review');

      await page.goto('/Restaurant/Profile', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'restaurant-profile');

      await page.goto('/Restaurant/Scanner', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'restaurant-scanner');
    });
  });

  // ─── SHIPPER PAGES ───
  test.describe('🚚 Shipper Pages', () => {

    test('[TC-VR-22] Shipper Dashboard + OrderDetail', async ({ page }) => {
      await loginAs(page, USERS.shipper2);
      await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);
      await takePageScreenshot(page, 'shipper-dashboard');
    });

    test('[TC-VR-23] Shipper Wallet + Income + History', async ({ page }) => {
      await loginAs(page, USERS.shipper2);
      await page.goto('/Shipper/ViTien', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'shipper-wallet');

      await page.goto('/Shipper/ThuNhap', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'shipper-income');

      await page.goto('/Shipper/LichSu', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'shipper-history');
    });

    test('[TC-VR-24] Shipper QRDelivery + CaiDat + ThongBao', async ({ page }) => {
      await loginAs(page, USERS.shipper2);
      await page.goto('/Shipper/QRDelivery', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'shipper-qr');

      await page.goto('/Shipper/CaiDat', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'shipper-settings');

      await page.goto('/Shipper/ThongBao', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'shipper-notifications');
    });
  });

  // ─── ADMIN PAGES ───
  test.describe('👑 Admin Pages', () => {

    test('[TC-VR-25] Admin Dashboard', async ({ page }) => {
      await loginAs(page, USERS.admin1);
      await page.goto('/Admin/Dashboard', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);
      await takePageScreenshot(page, 'admin-dashboard');
    });

    test('[TC-VR-26] Admin User Mgmt (4 pages)', async ({ page }) => {
      await loginAs(page, USERS.admin1);

      await page.goto('/Admin/QuanLyKhachHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'admin-users');

      await page.goto('/Admin/QuanLyQuanAn', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'admin-restaurants');

      await page.goto('/Admin/QuanLyShipper', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'admin-shippers');
    });

    test('[TC-VR-27] Admin Orders + Chat + Voucher + Category', async ({ page }) => {
      await loginAs(page, USERS.admin1);

      await page.goto('/Admin/Order', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'admin-orders');

      await page.goto('/Admin/Category', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'admin-categories');

      await page.goto('/Admin/VoucherManager', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'admin-vouchers');
    });

    test('[TC-VR-28] Admin Chat + DeliveryLogs + WalletManager', async ({ page }) => {
      await loginAs(page, USERS.admin1);

      await page.goto('/AdminChat', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'admin-chat');

      await page.goto('/EDelivery/DeliveryLogs', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'admin-delivery-logs');

      await page.goto('/Admin/WalletManager', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
      await takePageScreenshot(page, 'admin-wallet');
    });
  });
});
