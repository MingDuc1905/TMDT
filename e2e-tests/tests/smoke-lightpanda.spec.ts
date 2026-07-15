import { test, expect } from '@playwright/test';
import { USERS, URLS, SEED } from '../fixtures/users';
import { HomePage } from '../pages/HomePage';
import { LoginPage } from '../pages/LoginPage';
import { DetailRestaurantPage } from '../pages/DetailRestaurantPage';
import { CartPage } from '../pages/CartPage';
import { RestaurantPage } from '../pages/RestaurantPage';
import { ShipperPage } from '../pages/ShipperPage';
import { AdminPage } from '../pages/AdminPage';

const BASE = 'https://fastship-web.onrender.com';

test.setTimeout(120_000);

// ─── Public Pages Load ───

test.describe('Smoke: Public Pages Load', () => {
  test('homepage loads with title and navbar', async ({ page }) => {
    const home = new HomePage(page);
    await page.goto(`${BASE}/`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await expect(page).toHaveTitle(/FastShip/i);
    await expect(home.navbar).toBeVisible();
  });

  test('login page loads with auth card', async ({ page }) => {
    await page.goto(`${BASE}/Home/Login`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await expect(page.locator('.auth-card')).toBeVisible();
  });

  test('signup page loads with role cards', async ({ page }) => {
    await page.goto(`${BASE}/Home/Signup`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    const body = page.locator('body');
    await expect(body).not.toBeEmpty();
  });

  test('restaurant detail page loads (id=6)', async ({ page }) => {
    await page.goto(`${BASE}/Home/DetailRestaurant?id=6`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await expect(page.locator('.name-restaurant')).toBeVisible();
  });

  test('health endpoint returns OK', async ({ page }) => {
    const resp = await page.goto(`${BASE}/health`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    const text = await resp?.text() ?? '';
    expect(text.toLowerCase()).toContain('healthy');
  });
});

// ─── Authenticated Pages Redirect ───

test.describe('Smoke: Authenticated Pages Redirect', () => {
  test('cart redirects to login when not authenticated', async ({ page }) => {
    await page.goto(`${BASE}/Cart`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await expect(page).toHaveURL(/\/Home\/Login/);
  });

  test('checkout redirects to login when not authenticated', async ({ page }) => {
    await page.goto(`${BASE}/Cart/Checkout`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await expect(page).toHaveURL(/\/Home\/Login/);
  });

  test('restaurant dashboard redirects to login', async ({ page }) => {
    await page.goto(`${BASE}/Restaurant`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await expect(page).toHaveURL(/\/Home\/Login/);
  });

  test('shipper dashboard redirects to login', async ({ page }) => {
    await page.goto(`${BASE}/Shipper`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await expect(page).toHaveURL(/\/Home\/Login/);
  });

  test('admin dashboard redirects to login', async ({ page }) => {
    await page.goto(`${BASE}/Admin`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await expect(page).toHaveURL(/\/Home\/Login/);
  });
});

// ─── Restaurant Menu ───

test.describe('Smoke: Restaurant Menu', () => {
  test('restaurant menu has items', async ({ page }) => {
    await page.goto(`${BASE}/Home/DetailRestaurant?id=6`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    const items = page.locator('.item-restaurant-row');
    await expect(items.first()).toBeVisible({ timeout: 30_000 });
    const count = await items.count();
    expect(count).toBeGreaterThan(0);
  });

  test('restaurant name is displayed', async ({ page }) => {
    await page.goto(`${BASE}/Home/DetailRestaurant?id=6`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    const name = page.locator('.name-restaurant');
    await expect(name).toBeVisible();
    const text = await name.textContent();
    expect(text?.trim().length).toBeGreaterThan(0);
  });
});

// ─── Homepage Elements ───

test.describe('Smoke: Homepage Elements', () => {
  test('homepage has carousel', async ({ page }) => {
    await page.goto(`${BASE}/`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await expect(page.locator('#header-carousel')).toBeAttached();
  });

  test('homepage has category pills', async ({ page }) => {
    await page.goto(`${BASE}/`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await expect(page.locator('#categoryRow')).toBeAttached();
  });

  test('homepage has restaurant cards', async ({ page }) => {
    await page.goto(`${BASE}/`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    const cards = page.locator('.product-item');
    await expect(cards.first()).toBeVisible({ timeout: 30_000 });
    const count = await cards.count();
    expect(count).toBeGreaterThan(0);
  });
});
