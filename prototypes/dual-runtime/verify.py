import json
import os
from pathlib import Path
import xml.etree.ElementTree as xml

from playwright.sync_api import expect, sync_playwright


ROOT = Path(__file__).resolve().parent
OUTPUT = ROOT / "previews"
checks = []


def passed(name):
    checks.append(name)
    print(f"PASS {name}")


def verify():
    OUTPUT.mkdir(exist_ok=True)
    namespaces = {"svg": "http://www.w3.org/2000/svg"}
    source = xml.parse(ROOT / "icon-blue.svg")
    board = xml.parse(ROOT / "icon-review.svg")
    source_points = [polygon.get("points") for polygon in source.findall(".//svg:polygon", namespaces)]
    board_points = [polygon.get("points") for polygon in board.findall(".//svg:polygon", namespaces)]
    assert len(source_points) == 2 and source_points == board_points
    passed("icon and review board preserve the same two source polygons")

    with sync_playwright() as playwright:
        executable = os.environ.get("PLAYWRIGHT_CHROMIUM_EXECUTABLE")
        browser = playwright.chromium.launch(headless=True, executable_path=executable, channel=None if executable else "msedge")
        context = browser.new_context(viewport={"width": 1200, "height": 1000}, locale="zh-CN", color_scheme="light")
        page = context.new_page()
        errors = []
        page.on("pageerror", lambda error: errors.append(str(error)))
        try:
            page.goto((ROOT / "index.html").as_uri())
            page.wait_for_load_state("networkidle")
            page.locator("#language").select_option("zh-CN")
            page.locator("#theme").select_option("light")
            expect(page.locator(".project-row")).to_have_count(6)
            expect(page.locator(".control-sample")).to_have_count(4)
            assert page.locator("img").evaluate_all("images => images.every(image => image.complete && image.naturalWidth > 0)")
            ids = page.locator("[id]").evaluate_all("nodes => nodes.map(node => node.id)")
            assert len(ids) == len(set(ids))
            passed("offline page loads its icons and shared prototype without duplicate IDs")
            page.screenshot(path=str(OUTPUT / "01-review-light-zh.png"), full_page=True)
            page.locator("#app").screenshot(path=str(OUTPUT / "02-net10-light.png"))

            page.locator("#search").fill("pjt1")
            expect(page.locator(".project-row")).to_have_count(2)
            assert "".join(page.locator(".project-row").first.locator("mark").all_text_contents()) == "pjt1"
            page.keyboard.press("ArrowDown")
            selected_path = page.locator("#project-path").inner_text()
            page.locator("#launch-options summary").click()
            page.locator("#extra-args").fill('-tclargs "设计 test"')
            for runtime in ["net48", "net10", "net48"]:
                page.locator("#runtime").select_option(runtime)
                expect(page.locator("html")).to_have_attribute("data-runtime", runtime)
                expect(page.locator("#search")).to_have_value("pjt1")
                expect(page.locator("#project-path")).to_have_text(selected_path)
                expect(page.locator("#extra-args")).to_have_value('-tclargs "设计 test"')
            passed("fzf nonconsecutive matching and all state survive edition switching")

            page.locator("#language").select_option("en")
            expect(page.locator("#identity-title")).to_have_text("The same arrow, in blue.")
            expect(page.locator("#extra-args")).to_have_value('-tclargs "设计 test"')
            expect(page.locator("#runtime-note")).to_contain_text("Framework 4.8")
            passed("new review controls translate without losing application inputs")

            for runtime in ["net10", "net48"]:
                page.locator("#runtime").select_option(runtime)
                for theme in ["light", "dark"]:
                    page.locator("#theme").select_option(theme)
                    expect(page.locator("html")).to_have_attribute("data-theme", theme)
                    expect(page.locator('meta[name="color-scheme"]')).to_have_attribute("content", theme)
                    page.locator("#scenario").select_option("missing-binding")
                    expect(page.locator("#launch-button")).to_be_disabled()
                    expect(page.locator('#version option[value="missing-2023"]')).to_be_disabled()
                    expect(page.locator('.project-row[data-project-id="zu24"] .unavailable-version')).to_contain_text("Binding unavailable")
                    page.locator("#manage-button").click()
                    expect(page.locator("#manage-dialog")).to_be_visible()
                    expect(page.locator('.install-card[data-install-id="missing-2023"]')).to_contain_text("Unavailable")
                    page.keyboard.press("Escape")
                    expect(page.locator("#manage-dialog")).not_to_be_visible()
                    page.locator("#version").select_option("amd-2025")
                    page.locator("#launch-button").click()
                    expect(page.locator("#confirm-dialog")).to_be_visible()
                    page.keyboard.press("Escape")
                    page.keyboard.press("Control+f")
                    expect(page.locator("#search")).to_be_focused()
                    page.locator("#search").dispatch_event("keydown", {"key": "Enter", "isComposing": True, "bubbles": True})
                    expect(page.locator("#confirm-dialog")).not_to_be_visible()
                    passed(f"{runtime}/{theme}: warnings, disabled selection, mismatch, Escape and IME guard")

            page.locator("#language").select_option("zh-CN")
            page.locator("#scenario").select_option("missing-binding")
            page.locator("#app").screenshot(path=str(OUTPUT / "03-net48-dark-unavailable.png"))
            page.locator("#control-review").screenshot(path=str(OUTPUT / "04-control-comparison.png"))
            for sample in page.locator(".control-sample").all():
                scheme = sample.get_attribute("data-scheme")
                background = sample.evaluate("node => getComputedStyle(node).backgroundColor")
                foreground = sample.evaluate("node => getComputedStyle(node).color")
                assert background == ("rgb(43, 43, 43)" if scheme == "dark" else "rgb(255, 255, 255)")
                assert foreground == ("rgb(243, 243, 243)" if scheme == "dark" else "rgb(31, 31, 31)")
                expect(sample.locator("button:disabled")).to_be_disabled()
                sample.locator(".demo-action").click()
                expect(sample.locator(".sample-status")).to_contain_text("未创建真实进程")
                sample.locator("summary").click()
                expect(sample.locator(".sample-details input")).to_be_visible()
            passed("all four independently themed control samples retain contrast and work")

            page.locator("#theme").select_option("system")
            page.emulate_media(color_scheme="dark")
            expect(page.locator("html")).to_have_attribute("data-theme", "dark")
            page.emulate_media(color_scheme="light")
            expect(page.locator("html")).to_have_attribute("data-theme", "light")
            passed("system appearance responds to live preference changes")

            for scenario in ["empty", "no-installs", "missing-project"]:
                page.locator("#scenario").select_option(scenario)
                expect(page.locator("#launch-button")).to_be_disabled()
            page.locator("#scenario").select_option("scan")
            expect(page.locator("#status")).to_contain_text("扫描")
            expect(page.locator("#status")).to_contain_text("扫描完成", timeout=5000)
            page.locator("#scenario").select_option("launch-error")
            page.locator("#launch-button").click()
            expect(page.locator("#status")).to_contain_text("启动失败", timeout=5000)
            page.locator("#view-error").click()
            expect(page.locator("#log-content")).to_contain_text("模拟")
            page.keyboard.press("Escape")
            passed("empty, no-install, missing-file, scanning and launch-error scenarios")

            page.locator("#scenario").select_option("normal")
            page.locator("#search").fill("no-such-project-39484")
            expect(page.locator("#empty-state")).to_be_visible()
            page.locator("#search").fill("")
            page.keyboard.press("Control+o")
            expect(page.locator("#browse-path")).to_be_focused()
            page.keyboard.press("Escape")
            passed("empty search result and keyboard browsing remain available")

            page.locator("#language").select_option("en")
            for width in [720, 390]:
                page.set_viewport_size({"width": width, "height": 900})
                assert page.evaluate("document.documentElement.scrollWidth <= innerWidth")
            page.screenshot(path=str(OUTPUT / "05-narrow-en.png"), full_page=True)
            passed("English review remains within 720px and 390px viewports")

            page.set_viewport_size({"width": 1200, "height": 1000})
            for runtime in ["net10", "net48"]:
                page.locator("#runtime").select_option(runtime)
                for theme in ["light", "dark"]:
                    page.locator("#theme").select_option(theme)
                    page.locator("#scenario").select_option("missing-both")
                    missing = page.locator('.project-row[data-project-id="zu24"]')
                    expect(missing.locator(".project-path-warning")).to_have_text("Path unavailable")
                    expect(missing.locator(".unavailable-version")).to_contain_text("Binding unavailable")
                    expect(page.locator("#project-issues")).to_be_visible()
                    expect(page.locator("#launch-button")).to_be_disabled()
                    assert missing.evaluate("node => node.querySelector('.project-path-warning').getBoundingClientRect().bottom <= node.getBoundingClientRect().bottom")
                    selected_background = missing.evaluate("node => getComputedStyle(node).backgroundColor")
                    page.locator('.project-row[data-project-id="zu15"]').click()
                    assert missing.evaluate("node => getComputedStyle(node).backgroundColor") == selected_background
                    expect(page.locator("#project-issues")).not_to_be_visible()
                    page.locator("#search").fill("pjt1")
                    expect(missing.locator(".project-path-warning")).to_be_visible()
                    assert "".join(missing.locator(".project-row-title mark").all_text_contents()) == "pjt1"
                    passed(f"{runtime}/{theme}: missing project stays highlighted on selection and fzf filtering")

            page.locator("#language").select_option("zh-CN")
            page.locator("#scenario").select_option("missing-both")
            page.locator("#app").screenshot(path=str(OUTPUT / "07-missing-project-and-version.png"))
            for language in ["zh-CN", "en"]:
                page.locator("#language").select_option(language)
                for runtime in ["net10", "net48"]:
                    page.locator("#runtime").select_option(runtime)
                    for theme in ["light", "dark"]:
                        page.locator("#theme").select_option(theme)
                        page.locator("#scenario").select_option("warning-comparison")
                        labels = page.locator(".warning-state .row-warning-label")
                        expect(labels).to_have_count(3)
                        assert labels.all_text_contents() == (["绑定失效", "路径失效", "版本失效"] if language == "zh-CN" else ["Binding unavailable", "Path unavailable", "Version unavailable"])
                        styles = labels.evaluate_all("nodes => nodes.map(node => { const style = getComputedStyle(node); return [style.color, style.fontSize, style.lineHeight, style.marginTop, getComputedStyle(node.parentElement).backgroundColor, Math.round(node.getBoundingClientRect().right)]; })")
                        assert styles[0] == styles[1] == styles[2]
                        expect(page.locator(".project-row-description .project-path-warning")).to_have_count(0)
                        for project_id in ["zu24", "zu9", "blm"]:
                            row = page.locator(f'.project-row[data-project-id="{project_id}"]')
                            row.click()
                            expect(page.locator("#project-issues .project-issue")).to_have_count(1)
                            assert row.locator(".row-warning-label").inner_text() == page.locator("#project-issues strong").inner_text()
                        for width in [720, 390]:
                            page.set_viewport_size({"width": width, "height": 1000})
                            assert page.evaluate("document.documentElement.scrollWidth <= innerWidth")
                        page.set_viewport_size({"width": 1200, "height": 1000})
            passed("all three warning types share right-column position, typography and detail placement across editions, languages and themes")
            page.locator("#language").select_option("zh-CN")
            page.locator("#theme").select_option("light")
            page.locator("#scenario").select_option("warning-comparison")
            page.locator("#app").screenshot(path=str(OUTPUT / "10-unified-warning-labels.png"))
            for runtime, language, theme in [("net10", "zh-CN", "light"), ("net48", "en", "dark")]:
                page.locator("#runtime").select_option(runtime)
                page.locator("#language").select_option(language)
                page.locator("#theme").select_option(theme)
                page.locator("#scenario").select_option("missing-binding")
                page.locator("#manage-button").click()
                removal = page.locator('[data-remove-id="missing-2023"]')
                expect(removal).to_be_visible()
                if language == "zh-CN":
                    page.locator("#manage-dialog").screenshot(path=str(OUTPUT / "08-remove-install-entry.png"))
                removal.click()
                expect(page.locator('#confirm-dialog button[value="cancel"]')).to_be_focused()
                expect(page.locator("#confirm-description")).to_contain_text(r"D:\EDA Tools\Vivado\2023.2")
                expect(page.locator("#confirm-description")).to_contain_text("1 个工程绑定" if language == "zh-CN" else "1 projects are bound")
                if language == "zh-CN":
                    page.locator("#confirm-dialog").screenshot(path=str(OUTPUT / "09-remove-install-confirmation.png"))
                page.keyboard.press("Escape")
                expect(removal).to_be_focused()
                expect(page.locator("#removed-install-actions")).not_to_be_visible()
                removal.click()
                page.locator("#confirm-action").click()
                expect(removal).to_have_count(0)
                expect(page.locator("#undo-remove")).to_be_focused()
                expect(page.locator('#version option[value="missing-2023"]')).to_have_count(0)
                page.locator("#refresh-button").click()
                expect(page.locator("#undo-remove")).to_be_disabled()
                expect(page.locator("#refresh-button")).to_be_enabled(timeout=5000)
                expect(removal).to_have_count(0)
                page.locator("#undo-remove").click()
                expect(removal).to_be_focused()
                expect(page.locator('#version option[value="missing-2023"]')).to_be_disabled()
                removal.click()
                page.locator("#confirm-action").click()
                page.keyboard.press("Escape")
                expect(page.locator("#launch-button")).to_be_disabled()
                expect(page.locator('.project-row[data-project-id="zu24"] .unavailable-version')).to_be_visible()
                expect(page.locator('.project-row[data-project-id="blm"] .unavailable-version')).to_be_visible()
                expect(page.locator(".project-row")).to_have_count(6)
                page.locator("#manage-button").click()
                page.locator("#add-path-button").click()
                page.locator("#install-path").fill(r"E:\Other Tools\Vivado\2023.2")
                page.locator('#add-path-form button[type="submit"]').click()
                page.keyboard.press("Escape")
                expect(page.locator("#launch-button")).to_be_disabled()
                expect(page.locator('.project-row[data-project-id="zu24"] .unavailable-version')).to_be_visible()
                page.locator("#manage-button").click()
                page.locator("#add-path-button").click()
                page.locator("#install-path").fill(r"D:\EDA Tools\Vivado\2023.2")
                page.locator('#add-path-form button[type="submit"]').click()
                expect(page.locator("#removed-install-actions")).not_to_be_visible()
                page.keyboard.press("Escape")
                expect(page.locator("#version")).to_have_value("missing-2023")
                expect(page.locator("#launch-button")).to_be_enabled()
                expect(page.locator('.project-row[data-project-id="zu24"] .unavailable-version')).to_have_count(0)
                passed(f"{runtime}/{language}: cancel, removal, rescan, undo and same-path recovery preserve binding identity")

            page.set_viewport_size({"width": 1120, "height": 700})
            page.goto((ROOT / "icon-review.svg").as_uri())
            page.wait_for_load_state("networkidle")
            page.screenshot(path=str(OUTPUT / "06-icon-review.png"))
            assert not errors, errors
            passed("review SVG renders and browser reports no JavaScript errors")
            (OUTPUT / "verification.json").write_text(json.dumps({"checks": checks, "browser": browser.version, "errors": errors}, indent=2), encoding="utf-8")
        finally:
            browser.close()
    print(f"{len(checks)} check groups passed")


if __name__ == "__main__":
    verify()
