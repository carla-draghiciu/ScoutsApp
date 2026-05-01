import { test, expect } from '@playwright/test';

test('should create a new event', async ({ page }) => {
  await page.goto('http://localhost:4200/create');

  await page.fill('input[name="name"]', 'Test Event');
  await page.fill('input[name="location"]', 'Test City');
  await page.fill('input[name="startDate"]', '2026-05-01');
  await page.fill('input[name="endDate"]', '2026-05-02');
  await page.fill('input[name="price"]', '50');
  await page.fill('textarea[name="description"]', 'Test Description');
  await page.fill('input[name="registrationDeadline"]', '2026-04-28');

  await page.click('button[type="submit"]');

  await page.click('button[name="next"]');

  await expect(page.locator('text=Test Event')).toBeVisible();
});


test('should filter events by price', async ({ page }) => {
  await page.goto('http://localhost:4200/events');

  const allEvents = await page.locator('.card').count();
  await page.selectOption('select[name="filter-price"]', 'free');

  await page.waitForTimeout(500);

  const freeEvents = await page.locator('.card').count();
  expect(freeEvents).toBeLessThanOrEqual(0);
});

test('should save filterPrice to cookie and restore it on reload', async ({ page, context }) => {
  await page.goto('http://localhost:4200/events');

  await page.selectOption('select[name="filter-price"]', 'free');
  await page.waitForTimeout(300);

  const cookiesAfterSelect = await context.cookies();
  const priceCookie = cookiesAfterSelect.find(c => c.name === 'filterPrice');

  expect(priceCookie).toBeDefined();
  expect(priceCookie?.value).toBe('free');

  await page.reload();
  await page.waitForTimeout(300);

  const selectedValue = await page.inputValue('select[name="filter-price"]');
  expect(selectedValue).toBe('free');
});