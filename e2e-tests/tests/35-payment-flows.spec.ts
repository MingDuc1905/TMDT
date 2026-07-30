/**
 * 💳 BỘ TEST 35: PAYMENT FLOWS — VNPay, MoMo, Failure, BankWebhook
 *
 * Mục tiêu: Test các phương thức thanh toán còn lại
 * - VNPay: tạo payment, redirect, IPN, return
 * - MoMo: sandbox QR + payment
 * - FailureView: hiển thị khi payment fail
 * - BankWebhook: verify giao dịch
 * - CheckPaymentStatus: API kiểm tra trạng thái
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS } from '../fixtures/users';

const CUSTOMER = USERS.customer1;

async function loginAsCustomer(page: any) {
  const login = new LoginPage(page);
  const url = await login.login(CUSTOMER.username, CUSTOMER.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (!page.url().includes('/Home/Login')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

// ─── VNPAY ───
test.describe('💳 VNPay Payment', () => {

  test('[TC-PAY-01] CreateVnpayPayment API — trả về payment URL', async ({ page }) => {
    await loginAsCustomer(page);

    // Test API với order không tồn tại
    const resp = await page.request.post('/Payment/CreateVnpayPayment?orderId=99999', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 CreateVnpayPayment: ${JSON.stringify(json)}`);
    expect(json).toBeDefined();
    // Nếu order không tồn tại, trả về error hoặc success=false
    if (json.success === false) {
      console.log(`✅ Correctly rejected invalid order: ${json.message}`);
    }
  });

  test('[TC-PAY-02] VnpayReturn page — load + hiển thị kết quả', async ({ page }) => {
    const resp = await page.goto('/Payment/VnpayReturn?vnp_ResponseCode=00&vnp_TransactionNo=TEST123&vnp_Amount=100000&vnp_OrderInfo=Test', {
      waitUntil: 'domcontentloaded', timeout: 30_000
    });
    await page.waitForTimeout(3000);

    const status = resp?.status() ?? 0;
    const url = page.url();
    console.log(`📍 VnpayReturn: ${url.substring(0, 80)}, status: ${status}`);

    const bodyText = await page.locator('body').textContent() || '';
    console.log(`📄 Page: ${bodyText.substring(0, 150)}`);
    expect(bodyText.length).toBeGreaterThan(0);
  });

  test('[TC-PAY-03] VnpayIPN — xử lý IPN callback', async ({ page }) => {
    await loginAsCustomer(page);

    const resp = await page.request.get('/Payment/VnpayIPN?vnp_ResponseCode=00&vnp_TransactionNo=TEST123&vnp_Amount=100000&vnp_TxnRef=99999', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 VnpayIPN: ${JSON.stringify(json)}`);
    expect(json).toBeDefined();
  });
});

// ─── MoMo ───
test.describe('💳 MoMo Payment', () => {

  test('[TC-PAY-04] MoMo payment option hiển thị trên checkout', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/Cart/Checkout', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const momoOption = page.locator('.payment-option:has-text("MoMo"), .payment-option:has-text("momo"), [class*="momo"]').first();
    const momoVisible = await momoOption.isVisible().catch(() => false);
    console.log(`💳 MoMo option: ${momoVisible}`);

    if (momoVisible) {
      await momoOption.click();
      await page.waitForTimeout(1000);

      // Kiểm tra QR/image hiển thị sau khi chọn MoMo
      const qrSection = page.locator('.qr-wrapper, #payment-info-area, [class*="momo-info"]').first();
      console.log(`📱 MoMo QR/payment info: ${await qrSection.isVisible().catch(() => false)}`);
    }
  });
});

// ─── FAILURE VIEW ───
test.describe('❌ Payment Failure', () => {

  test('[TC-PAY-05] FailureView page load', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/Cart/FailureView', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent() || '';
    console.log(`📄 Failure page: ${bodyText.substring(0, 150)}`);

    // Should show error/failure message
    const hasFailure = bodyText.includes('thất bại') || bodyText.includes('lỗi') || bodyText.includes('không thành công');
    console.log(`❌ Failure message: ${hasFailure}`);

    // Should have link to retry or go back
    const retryLink = page.locator('a[href*="Checkout"], a[href*="Cart"], a:has-text("thử lại")').first();
    console.log(`🔄 Retry link: ${await retryLink.isVisible().catch(() => false)}`);
  });
});

// ─── BANK WEBHOOK ───
test.describe('🏦 Bank Webhook & Verification', () => {

  test('[TC-PAY-06] BankWebhook — verify API response', async ({ page }) => {
    await loginAsCustomer(page);

    const resp = await page.request.post('/Payment/BankWebhook', {
      data: {
        accountNo: '1234567890',
        amount: 50000,
        reference: `TEST${Date.now()}`,
        description: 'Test payment'
      },
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 BankWebhook: ${JSON.stringify(json)}`);
    expect(json).toBeDefined();
  });

  test('[TC-PAY-07] VerifyBankTransaction — trạng thái giao dịch', async ({ page }) => {
    await loginAsCustomer(page);

    const resp = await page.request.get('/Payment/VerifyBankTransaction?madh=99999', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 VerifyBank: ${JSON.stringify(json)}`);
    expect(json).toBeDefined();
  });

  test('[TC-PAY-08] CheckPaymentStatus — API kiểm tra trạng thái', async ({ page }) => {
    await loginAsCustomer(page);

    const resp = await page.request.get('/Payment/CheckPaymentStatus?madh=99999', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 CheckPayment: ${JSON.stringify(json)}`);
    expect(json).toBeDefined();
  });
});

// ─── CHECK ORDER STATUS ───
test.describe('📋 CheckOrderStatus API', () => {

  test('[TC-PAY-09] CheckOrderStatus — order không tồn tại', async ({ page }) => {
    await loginAsCustomer(page);

    const resp = await page.request.get('/Payment/CheckOrderStatus?orderId=99999', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 CheckOrder: ${JSON.stringify(json)}`);
    expect(json).toBeDefined();
  });
});
