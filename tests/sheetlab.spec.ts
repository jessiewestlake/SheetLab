import { test, expect } from '@playwright/test';

async function exportFromApp(page) {
  const xml = await page.evaluate(() => (window as any).sheetlabTest.exportXML());
  return xml;
}

async function importViaFileInput(page, filepath: string) {
  await page.setInputFiles('#fileInput', filepath);
  await page.waitForTimeout(400);
}

function getMeasureTokens(page, index: number) {
  return page.evaluate((i) => {
    (window as any).selectMeasure(i);
    return (document.querySelector('#measureTokens') as HTMLTextAreaElement).value.trim();
  }, index);
}

function setMeasureTokensAndApply(page, tokens: string) {
  return page.evaluate((t) => {
    const ta = document.querySelector('#measureTokens') as HTMLTextAreaElement;
    ta.value = t;
    (window as any).pushUndo();
    (window as any).applyMeasureTokens();
  }, tokens);
}

function clickAddMeasure(page) {
  return page.click('#btnAddMeasure');
}

function selectMeasure(page, idx: number) {
  return page.evaluate((i) => (window as any).selectMeasure(i), idx);
}

test.describe('Measure Editor idempotence and duplication', () => {
  test('import → apply same tokens is a no-op; then duplicate measure 1 to 3 (semantic equality)', async ({ page }) => {
    await page.goto('/sheetlab.html');

    await importViaFileInput(page, 'moonlight_sonata_intro.musicxml');

    // Measure 1 tokens on import
    await selectMeasure(page, 0);
    const tokensM1 = await getMeasureTokens(page, 0);
    expect(tokensM1.length).toBeGreaterThan(0);

    // Apply exact tokens back and re-check tokens (should be unchanged)
    await setMeasureTokensAndApply(page, tokensM1);
    const tokensM1After = await getMeasureTokens(page, 0);
    expect(tokensM1After).toBe(tokensM1);

    // Duplicate into measure 3 via tokens
    await clickAddMeasure(page);
    await selectMeasure(page, 2);
    await setMeasureTokensAndApply(page, tokensM1);
  const tokensM3 = await getMeasureTokens(page, 2);
  expect(tokensM3).toBe(tokensM1);

  // Export and compare that measure 1 and 3 XML are identical
  const xmlOut = await exportFromApp(page);
    const measures = [...xmlOut.matchAll(/<measure[\s\S]*?<\/measure>/gi)].map(m => m[0]);
  expect(measures.length).toBeGreaterThanOrEqual(3);
    const normalizeMeasure = (s: string) => s
      .replace(/number="\d+"/g, 'number="X"')
      .replace(/width="[^"]+"/g, 'width="W"')
      .replace(/\snumber="\d+"/g, ' number="X"') // inside child elements
      .replace(/>\s+</g, '><')
      .replace(/\s+/g, ' ')
      .trim();
    expect(normalizeMeasure(measures[2])).toBe(normalizeMeasure(measures[0]));
  });
});
