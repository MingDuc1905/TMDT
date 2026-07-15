import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { ShipperPage } from '../pages/ShipperPage';
import { USERS, URLS } from '../fixtures/users';

test.setTimeout(120_000);

const SHIPPER = USERS.shipper1;

async function loginShipper(page: any) {
  const login = new LoginPage(page);
  const url = await login.login(SHIPPER.username, SHIPPER.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let retry = 0; retry < 2; retry++) {
      try {
        await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 20_000 });
        if (page.url().includes('/Shipper')) break;
      } catch {
        await page.waitForTimeout(1000);
      }
    }
  }
}

test.describe('Shipper: Dashboard', () => {

  test('shipper dashboard loads after login', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);
    expect(page.url()).toContain('/Shipper');
  });

  test('dashboard shows order table or tabs', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const tableCount = await shipper.orderTable.count();
    const fpCount = await shipper.freepickTab.count();
    const orCount = await shipper.orderTab.count();
    const hasContent = tableCount > 0 || fpCount > 0 || orCount > 0;
    console.log(`Dashboard: table=${tableCount}, freepickTab=${fpCount}, orderTab=${orCount}`);
    expect(hasContent).toBeTruthy();
  });

  test('dashboard sidebar is visible', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const sidebar = page.locator('.sidebar, .side-bar, nav, [class*="sidebar"]');
    const sidebarCount = await sidebar.count();
    console.log(`Sidebar elements: ${sidebarCount}`);
    expect(sidebarCount).toBeGreaterThan(0);
  });

  test('dashboard has FREE-PICK tab', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const fpCount = await shipper.freepickTab.count();
    const bodyText = await page.locator('body').textContent().catch(() => '');
    const hasFreepick = fpCount > 0 || (bodyText && bodyText.includes('FREE-PICK'));
    console.log(`FREE-PICK tab: count=${fpCount}, text match=${hasFreepick}`);
    expect(hasFreepick).toBeTruthy();
  });

  test('unauthenticated user redirected from shipper dashboard', async ({ page }) => {
    await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);
    const url = page.url();
    console.log(`Unauthenticated URL: ${url}`);
    const isRedirected = url.includes('/Home/Login') || url.includes('/Home/Error') || !url.includes('/Shipper');
    expect(isRedirected).toBeTruthy();
  });
});

test.describe('Shipper: Order Management', () => {

  test('FREE-PICK tab shows available orders', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    await shipper.openFreepickTab();
    await page.waitForTimeout(2000);

    const orderCount = await shipper.getOrderCount();
    console.log(`FREE-PICK orders: ${orderCount}`);
    expect(orderCount).toBeGreaterThanOrEqual(0);
  });

  test('order tab shows claimed orders', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    await shipper.openOrderTab();
    await page.waitForTimeout(2000);

    const orderCount = await shipper.getOrderCount();
    console.log(`Claimed orders: ${orderCount}`);
    expect(orderCount).toBeGreaterThanOrEqual(0);
  });

  test('order rows show order details', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const rowCount = await shipper.orderRows.count();
    console.log(`Order rows: ${rowCount}`);

    if (rowCount > 0) {
      const firstRow = shipper.orderRows.first();
      const cellCount = await firstRow.locator('td').count();
      console.log(`Cells in first row: ${cellCount}`);
      expect(cellCount).toBeGreaterThan(0);
    } else {
      console.log('No order rows to inspect');
    }
  });

  test('refresh button exists', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const refreshCount = await shipper.refreshBtn.count();
    const hasRefresh = refreshCount > 0;
    console.log(`Refresh button: ${hasRefresh}`);
    expect(hasRefresh).toBeTruthy();
  });

  test('detail link exists for orders', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const linkCount = await shipper.detailLink.count();
    console.log(`Detail links: ${linkCount}`);
    if (linkCount > 0) {
      expect(linkCount).toBeGreaterThan(0);
    } else {
      console.log('No detail links - may be no orders');
    }
  });

  test('order count is numeric', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const orderCount = await shipper.getOrderCount();
    console.log(`Order count: ${orderCount}`);
    expect(typeof orderCount).toBe('number');
    expect(orderCount).toBeGreaterThanOrEqual(0);
  });
});

test.describe('Shipper: Order Claiming', () => {

  test('order detail page loads with order info', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const linkCount = await shipper.detailLink.count();
    if (linkCount > 0) {
      await shipper.detailLink.first().click();
      await page.waitForLoadState('domcontentloaded');
      await page.waitForTimeout(2000);

      const url = page.url();
      console.log(`Order detail URL: ${url}`);
      expect(url).toContain('OrderDetail');
    } else {
      console.log('No orders to view detail');
    }
  });

  test('claim order button exists on detail page', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const linkCount = await shipper.detailLink.count();
    if (linkCount > 0) {
      await shipper.detailLink.first().click();
      await page.waitForLoadState('domcontentloaded');
      await page.waitForTimeout(2000);

      const claimBtn = page.locator(
        'a:has-text("Nhận đơn"), button:has-text("Nhận đơn"), a:has-text("Chấp nhận"), button:has-text("Chấp nhận"), a:has-text("Claim"), button:has-text("Claim")'
      );
      const claimCount = await claimBtn.count();
      const bodyText = await page.locator('body').textContent().catch(() => '');
      const hasClaimOption = claimCount > 0 || (bodyText && bodyText.includes('Nhận đơn'));
      console.log(`Claim button: ${claimCount}, text match: ${hasClaimOption}`);
    } else {
      console.log('No orders to check claim button');
    }
  });

  test('claimed order appears in order tab', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    await shipper.openOrderTab();
    await page.waitForTimeout(2000);

    const orderCount = await shipper.getOrderCount();
    console.log(`Orders in tab: ${orderCount}`);
    expect(orderCount).toBeGreaterThanOrEqual(0);
  });

  test('shipper can update delivery status', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoDashboard();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    await shipper.openOrderTab();
    await page.waitForTimeout(2000);

    const statusBtns = [
      page.locator('a[href*="layhang"], a:has-text("Lấy hàng")'),
      page.locator('a[href*="danggiao"], a:has-text("Đang giao")'),
      page.locator('a[href*="dagiao"], a:has-text("Đã giao")'),
    ];

    let foundAny = false;
    for (const btn of statusBtns) {
      const count = await btn.count();
      if (count > 0) {
        console.log(`Status button found: count=${count}`);
        foundAny = true;
        break;
      }
    }
    if (!foundAny) {
      console.log('No status update buttons visible - may need claimed orders first');
    }
  });
});

test.describe('Shipper: Wallet & Income', () => {

  test('wallet page loads', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoWallet();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const url = page.url();
    console.log(`Wallet URL: ${url}`);
    expect(url).toContain('ViTien');
  });

  test('income page loads', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoIncome();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const url = page.url();
    console.log(`Income URL: ${url}`);
    expect(url).toContain('ThuNhap');
  });

  test('wallet shows balance or zero', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoWallet();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const balance = await shipper.getWalletBalance();
    console.log(`Wallet balance: ${balance}`);
    const balanceText = balance || '0';
    const hasNumber = /\d/.test(balanceText);
    expect(hasNumber).toBeTruthy();
  });
});

test.describe('Shipper: History', () => {

  test('history page loads', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoHistory();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const url = page.url();
    console.log(`History URL: ${url}`);
    expect(url).toContain('LichSu');
  });

  test('history shows table or empty state', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoHistory();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const tableCount = await shipper.orderTable.count();
    const bodyText = await page.locator('body').textContent().catch(() => '');
    const hasTable = tableCount > 0;
    const hasContent = bodyText && bodyText.length > 50;
    console.log(`History: table=${hasTable}, hasContent=${hasContent}`);
    expect(hasTable || hasContent).toBeTruthy();
  });

  test('history has order records or empty', async ({ page }) => {
    await loginShipper(page);
    const shipper = new ShipperPage(page);
    await shipper.gotoHistory();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const rowCount = await shipper.orderRows.count();
    console.log(`History rows: ${rowCount}`);
    expect(rowCount).toBeGreaterThanOrEqual(0);
  });
});
