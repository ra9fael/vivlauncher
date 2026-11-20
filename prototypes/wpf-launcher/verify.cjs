const assert = require("node:assert/strict");
const path = require("node:path");
const fs = require("node:fs");
const { pathToFileURL } = require("node:url");
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || "playwright");

async function verify() {
  const browser = await chromium.launch({ headless: true, executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE || undefined });
  const output = path.join(__dirname, "previews");
  fs.mkdirSync(output, { recursive: true });
  const context = await browser.newContext({ viewport: { width: 1200, height: 900 }, locale: "zh-CN", colorScheme: "light" });
  const page = await context.newPage();
  const errors = [];
  page.on("pageerror", error => errors.push(error.message));
  const check = async (name, action) => { await action(); console.log(`PASS ${name}`); };
  const scenario = async value => { await page.locator("#scenario").selectOption(value); };
  const statusContains = async value => { await page.waitForFunction(text => document.querySelector("#status").textContent.includes(text), value); };
  const closeDialog = async () => { await page.keyboard.press("Escape"); };

  try {
    await page.goto(pathToFileURL(path.join(__dirname, "index.html")).href);
    await page.waitForLoadState("networkidle");
    await page.locator("#language").selectOption("zh-CN");
    await page.locator("#theme").selectOption("light");
    await check("initial project and binding", async () => {
      assert.equal(await page.locator(".project-row").count(), 6);
      assert.equal(await page.locator("#version").inputValue(), "xilinx-2024");
      assert.match(await page.locator("#selection-source").innerText(), /绑定/);
      assert.equal(await page.locator("#launch-button").isEnabled(), true);
    });
    await page.screenshot({ path: path.join(output, "01-light-zh.png"), fullPage: true });

    await check("search, keyboard navigation, no results", async () => {
      await page.locator("#search").fill("project_1");
      assert.equal(await page.locator(".project-row").count(), 2);
      await page.keyboard.press("ArrowDown");
      await page.keyboard.press("ArrowDown");
      assert.match(await page.locator("#project-path").innerText(), /zu15/);
      await page.locator("#search").fill("no-such-project");
      assert.equal(await page.locator("#empty-state").isVisible(), true);
      assert.equal(await page.locator("#launch-button").isDisabled(), true);
      await page.keyboard.press("Control+f");
      assert.equal(await page.locator("#search").evaluate(node => node === document.activeElement), true);
    });
    await scenario("normal");
    await check("fzf v2 non-consecutive matching and visible positions", async () => {
      await page.locator("#search").fill("pjt1");
      assert.equal(await page.locator(".project-row").count(), 2);
      const match = page.locator(".project-row").first();
      assert.equal((await match.locator("mark").allTextContents()).join(""), "pjt1");
      assert.equal(await match.getAttribute("aria-selected"), "true");
      assert.match(await page.locator("#project-count").innerText(), /相关度/);
    });
    await page.screenshot({ path: path.join(output, "06-fzf-search.png"), fullPage: true });
    await check("fzf extended matching, score order, and empty-query order", async () => {
      await page.locator("#search").fill("zu15 xpr");
      assert.equal(await page.locator(".project-row").count(), 2);
      assert.equal(await page.locator(".project-row").first().getAttribute("data-project-id"), "zu15");
      assert.equal((await page.locator(".project-row").first().locator(".project-row-path mark").allTextContents()).join(""), "zu15");
      await page.locator("#search").fill("base");
      assert.deepEqual(await page.locator(".project-row").evaluateAll(rows => rows.map(row => row.dataset.projectId)), ["pynq", "blm-old", "blm"]);
      await page.locator("#search").fill("zu15 !pynq");
      assert.equal(await page.locator(".project-row").count(), 1);
      assert.equal(await page.locator(".project-row").first().getAttribute("data-project-id"), "zu15");
      await page.locator("#search").fill("'pjt1");
      assert.equal(await page.locator(".project-row").count(), 0);
      await page.locator("#search").fill("'pynq | 'hifi");
      assert.equal(await page.locator(".project-row").count(), 2);
      await page.locator("#search").fill("PYNQ");
      assert.equal(await page.locator(".project-row").count(), 0);
      await page.locator("#search").fill("");
      assert.deepEqual(await page.locator(".project-row").evaluateAll(rows => rows.map(row => row.dataset.projectId)), ["zu24", "zu15", "zu9", "blm", "pynq", "blm-old"]);
      assert.equal(await page.locator(".search-match").count(), 0);
    });
    await check("IME conversion does not launch from fuzzy search", async () => {
      await page.locator("#search").fill("pjt1");
      await page.locator("#search").dispatchEvent("keydown", { key: "Enter", isComposing: true, bubbles: true });
      assert.equal(await page.locator("#launch-button").isEnabled(), true);
      await page.locator("#search").dispatchEvent("keydown", { key: "Enter", keyCode: 229, bubbles: true });
      assert.equal(await page.locator("#launch-button").isEnabled(), true);
      assert.equal(await page.locator("#confirm-dialog").isVisible(), false);
    });
    await scenario("normal");
    await check("language switch preserves arguments and controls", async () => {
      await page.locator("#launch-options summary").click();
      await page.locator("#extra-args").fill('-mode gui -tclargs "custom value"');
      await page.locator("#no-log").uncheck();
      await page.locator("#language").selectOption("en");
      assert.equal(await page.locator("html").getAttribute("lang"), "en");
      assert.equal(await page.locator("#extra-args").inputValue(), '-mode gui -tclargs "custom value"');
      assert.equal(await page.locator("#no-log").isChecked(), false);
      await page.locator("#theme").selectOption("dark");
      assert.equal(await page.locator("html").getAttribute("data-theme"), "dark");
    });
    await page.screenshot({ path: path.join(output, "02-dark-en.png"), fullPage: true });
    await check("modal focus, Escape and return focus", async () => {
      await page.locator("#settings-button").click();
      assert.equal(await page.locator("#settings-dialog").evaluate(dialog => dialog.contains(document.activeElement)), true);
      for (let index = 0; index < 12; index += 1) await page.keyboard.press("Tab");
      assert.equal(await page.locator("#settings-dialog").evaluate(dialog => dialog.contains(document.activeElement)), true);
      await closeDialog();
      assert.equal(await page.locator("#settings-button").evaluate(button => button === document.activeElement), true);
    });
    await page.locator("#language").selectOption("zh-CN");
    await page.locator("#theme").selectOption("light");
    await scenario("normal");
    await check("version mismatch cancellation and successful launch", async () => {
      await page.locator("#version").selectOption("amd-2025");
      await page.locator("#launch-button").click();
      assert.equal(await page.locator("#confirm-dialog").isVisible(), true);
      await closeDialog();
      assert.equal(await page.locator("#launch-button").isEnabled(), true);
      await page.locator("#launch-button").click();
      await page.locator("#confirm-action").click();
      await page.waitForFunction(() => document.querySelector("#launch-button").disabled);
      await statusContains("已模拟启动 Vivado 2025.2");
    });
    await check("binding updates immediately", async () => {
      await page.locator("#bind-button").click();
      assert.equal(await page.locator("#bind-button").getAttribute("aria-pressed"), "true");
      await page.locator("#bind-button").click();
      assert.equal(await page.locator("#bind-button").getAttribute("aria-pressed"), "false");
    });
    await check("default install and custom path with spaces", async () => {
      await page.locator("#manage-button").click();
      await page.locator('[data-default-id="amd-2025"]').click();
      assert.match(await page.locator(".install-card.default").innerText(), /2025\.2/);
      await page.locator("#add-path-button").click();
      await page.locator("#install-path").fill("D:\\EDA Tools\\Vivado\\2023.2");
      await page.locator('#add-path-form button[type="submit"]').click();
      assert.equal(await page.locator(".install-card").count(), 3);
      assert.match(await page.locator("#install-list").innerText(), /EDA Tools/);
    });
    await page.screenshot({ path: path.join(output, "03-installations.png"), fullPage: true });
    await closeDialog();
    await scenario("missing-binding");
    await check("invalid versions are marked in project and version lists", async () => {
      const boundRow = page.locator('.project-row[data-project-id="zu24"]');
      const historyRow = page.locator('.project-row[data-project-id="blm"]');
      assert.match(await boundRow.getAttribute("class"), /has-unavailable-version/);
      assert.match(await boundRow.locator(".unavailable-version").innerText(), /2023\.2\s*绑定失效/);
      assert.match(await historyRow.locator(".unavailable-version").innerText(), /2023\.2\s*上次版本失效/);
      assert.match(await boundRow.getAttribute("aria-label"), /绑定版本 2023\.2 已失效/);
      assert.equal(await page.locator('#version option[value="missing-2023"]').isDisabled(), true);
      assert.match(await page.locator('#version option[value="missing-2023"]').innerText(), /失效/);
      assert.equal(await page.locator("#launch-button").isDisabled(), true);
    });
    await page.screenshot({ path: path.join(output, "07-unavailable-versions.png"), fullPage: true });
    await check("invalid installation stays visible and cannot become default", async () => {
      await page.locator("#manage-button").click();
      const invalid = page.locator('.install-card[data-install-id="missing-2023"]');
      assert.match(await invalid.getAttribute("class"), /unavailable/);
      assert.match(await invalid.innerText(), /安装失效/);
      assert.equal(await invalid.locator("button").count(), 0);
      assert.equal(await page.locator("#install-count").innerText(), "2");
    });
    await page.screenshot({ path: path.join(output, "08-unavailable-install.png"), fullPage: true });
    await closeDialog();
    await page.locator("#language").selectOption("en");
    await page.locator("#theme").selectOption("dark");
    await check("invalid warnings translate and stay visible in dark theme", async () => {
      assert.match(await page.locator('.project-row[data-project-id="zu24"] .unavailable-version').innerText(), /Binding unavailable/);
      assert.match(await page.locator('.project-row[data-project-id="blm"] .unavailable-version').innerText(), /Last version unavailable/);
      assert.equal(await page.locator("#launch-button").isDisabled(), true);
      await page.setViewportSize({ width: 720, height: 640 });
      assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
      const badge = page.locator('.project-row[data-project-id="blm"] .unavailable-version');
      assert.equal(await badge.evaluate(node => node.scrollWidth <= node.clientWidth), true);
      await page.setViewportSize({ width: 1200, height: 900 });
    });
    await page.screenshot({ path: path.join(output, "09-unavailable-dark-en.png"), fullPage: true });
    await page.locator("#language").selectOption("zh-CN");
    await page.locator("#theme").selectOption("light");
    await check("missing binding requires explicit replacement", async () => {
      assert.equal(await page.locator("#launch-button").isDisabled(), true);
      assert.match(await page.locator("#selection-source").innerText(), /2023\.2/);
      await page.locator("#version").selectOption("xilinx-2024");
      assert.equal(await page.locator("#launch-button").isEnabled(), true);
    });
    await check("rebinding clears the bound warning without hiding history issues", async () => {
      await page.locator("#bind-button").click();
      assert.equal(await page.locator('.project-row[data-project-id="zu24"] .unavailable-version').count(), 0);
      assert.equal(await page.locator('.project-row[data-project-id="blm"] .unavailable-version').count(), 1);
      await page.locator('.project-row[data-project-id="blm"]').click();
      assert.equal(await page.locator("#version").inputValue(), "xilinx-2024");
      assert.equal(await page.locator("#launch-button").isEnabled(), true);
    });
    await scenario("missing-binding");
    await check("same version in another installation does not revive a binding", async () => {
      await page.locator("#manage-button").click();
      await page.locator("#add-path-button").click();
      await page.locator("#install-path").fill("E:\\Other Tools\\Vivado\\2023.2");
      await page.locator('#add-path-form button[type="submit"]').click();
      await closeDialog();
      assert.equal(await page.locator('.project-row[data-project-id="zu24"] .unavailable-version').count(), 1);
      assert.equal(await page.locator('.project-row[data-project-id="blm"] .unavailable-version').count(), 0);
      assert.equal(await page.locator("#launch-button").isDisabled(), true);
    });
    await scenario("missing-project");
    await check("unavailable project stays in history", async () => {
      assert.equal(await page.locator(".project-row").count(), 6);
      assert.equal(await page.locator("#launch-button").isDisabled(), true);
      assert.match(await page.locator("#status").innerText(), /不可访问/);
    });
    await scenario("no-installs");
    await check("missing installs and async scan", async () => {
      assert.equal(await page.locator("#launch-button").isDisabled(), true);
      await page.locator("#manage-button").click();
      await page.locator("#refresh-button").click();
      await closeDialog();
      await page.locator("#search").fill("pynq");
      assert.equal(await page.locator(".project-row").count(), 1);
      await statusContains("扫描完成");
      assert.equal(await page.locator("#launch-button").isEnabled(), true);
    });
    await scenario("scan");
    await scenario("empty");
    await check("scenario reset cancels pending scan", async () => {
      await page.waitForTimeout(1700);
      assert.equal(await page.locator(".project-row").count(), 0);
      assert.equal(await page.locator("#status").innerText(), "选择或浏览一个工程");
    });
    await check("browse validates and accepts Unicode paths", async () => {
      await page.keyboard.press("Control+o");
      await page.locator("#browse-path").fill("relative.xpr");
      await page.locator('#browse-form button[type="submit"]').click();
      assert.match(await page.locator("#browse-error").innerText(), /完整/);
      await page.locator("#browse-path").fill("D:\\FPGA 设计\\工程一\\project.xpr");
      await page.locator('#browse-form button[type="submit"]').click();
      assert.equal(await page.locator("#project-path").innerText(), "D:\\FPGA 设计\\工程一\\project.xpr");
      assert.equal(await page.locator("#launch-button").isEnabled(), true);
    });
    await check("Chinese fuzzy terms and direct absolute paths", async () => {
      await page.locator("#launch-button").click();
      await statusContains("已模拟启动");
      await page.locator("#search").fill("设计 工一");
      assert.equal(await page.locator(".project-row").count(), 1);
      assert.equal((await page.locator(".project-row-path mark").allTextContents()).join(""), "设计工一");
      await page.locator("#search").fill('"D:\\FPGA 设计\\工程一\\project.xpr"');
      assert.equal(await page.locator("#project-path").innerText(), "D:\\FPGA 设计\\工程一\\project.xpr");
      assert.equal(await page.locator(".search-match").count(), 0);
    });
    await check("dropped file requires demo path confirmation", async () => {
      const transfer = await page.evaluateHandle(() => {
        const data = new DataTransfer();
        data.items.add(new File([""], "dropped.xpr", { type: "application/xml" }));
        return data;
      });
      await page.locator("#app").dispatchEvent("drop", { dataTransfer: transfer });
      assert.equal(await page.locator("#browse-dialog").isVisible(), true);
      assert.equal(await page.locator("#browse-path").inputValue(), "D:\\PreviewDrops\\dropped.xpr");
      await closeDialog();
    });
    await scenario("launch-error");
    await check("launch failure exposes useful log", async () => {
      await page.locator("#launch-button").click();
      await statusContains("启动失败");
      assert.equal(await page.locator("#launch-button").isEnabled(), true);
      await page.locator("#view-error").click();
      assert.match(await page.locator("#log-content").innerText(), /Windows 错误：2/);
      await closeDialog();
    });
    await page.screenshot({ path: path.join(output, "04-launch-error.png"), fullPage: true });
    await check("clear history confirmation does not reuse stale result", async () => {
      await page.locator("#settings-button").click();
      await page.locator("#clear-recent").click();
      await closeDialog();
      assert.equal(await page.locator(".project-row").count(), 6);
      await page.locator("#clear-recent").click();
      await page.locator("#confirm-action").click();
      await page.waitForFunction(() => document.querySelectorAll(".project-row").length === 0);
      await closeDialog();
      assert.equal(await page.locator(".project-row").count(), 0);
    });
    await scenario("normal");
    await check("double-click on a different project launches", async () => {
      await page.locator('.project-row[data-project-id="zu15"]').dblclick();
      await statusContains("已模拟启动 Vivado 2024.2");
      assert.match(await page.locator("#project-path").innerText(), /zu15/);
    });
    await scenario("normal");
    await check("desktop and narrow layouts do not overflow horizontally", async () => {
      for (const width of [920, 720, 390]) {
        await page.setViewportSize({ width, height: width === 390 ? 844 : 720 });
        assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true, `Overflow at ${width}px`);
      }
    });
    await page.screenshot({ path: path.join(output, "05-narrow.png"), fullPage: true });
    await check("language and appearance preferences persist on reload", async () => {
      await page.locator("#language").selectOption("en");
      await page.locator("#theme").selectOption("dark");
      await page.reload();
      assert.equal(await page.locator("html").getAttribute("lang"), "en");
      assert.equal(await page.locator("html").getAttribute("data-theme"), "dark");
    });
    assert.deepEqual(errors, []);
    console.log("PASS no browser runtime errors");
    console.log(`Screenshots: ${output}`);
  } finally {
    await browser.close();
  }
}

verify().catch(error => { console.error(error); process.exitCode = 1; });
