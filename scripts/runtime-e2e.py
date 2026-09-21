import asyncio
import json
import os
import re
from pathlib import Path

from playwright.async_api import async_playwright, expect

BASE_URL = os.environ.get("PORTAL_BASE_URL", "http://127.0.0.1:5080").rstrip("/")
EMPLOYEE_EMAIL = os.environ.get("E2E_EMPLOYEE_EMAIL", "nandi.employee@secureportal.test")
EMPLOYEE_PASSWORD = os.environ["E2E_EMPLOYEE_PASSWORD"]
ADMIN_EMAIL = os.environ["E2E_ADMIN_EMAIL"]
ADMIN_PASSWORD = os.environ["E2E_ADMIN_PASSWORD"]

OUT = Path(os.environ.get("E2E_OUTPUT_DIR", "artifacts/runtime-evidence"))
OUT.mkdir(parents=True, exist_ok=True)

checks = []


async def assert_page_healthy(page, label):
    await page.wait_for_load_state("domcontentloaded")
    await page.wait_for_timeout(450)

    body = await page.locator("body").inner_text()
    forbidden = [
        "An unhandled error has occurred",
        "This page isn’t working",
        "HTTP ERROR 500",
        "Internal Server Error",
    ]
    for text in forbidden:
        if text.lower() in body.lower():
            raise AssertionError(f"{label}: runtime error marker found: {text}")

    overflow = await page.evaluate(
        "() => Math.max(document.documentElement.scrollWidth, document.body.scrollWidth) - window.innerWidth"
    )
    if overflow > 2:
        raise AssertionError(f"{label}: document-level horizontal overflow: {overflow}px")

    checks.append({"page": label, "url": page.url, "horizontal_overflow_px": overflow})


async def snap(page, filename, label):
    await page.evaluate("() => window.scrollTo(0, 0)")
    await page.wait_for_timeout(250)
    await assert_page_healthy(page, label)
    await page.screenshot(path=str(OUT / filename), full_page=False)


async def login(page, email, password):
    await page.goto(BASE_URL + "/", wait_until="domcontentloaded")
    await page.locator("#email").fill(email)
    await page.locator("#password").fill(password)
    await page.get_by_role("button", name="Sign In", exact=True).click()
    await page.wait_for_url(re.compile(r".*/dashboard(?:\?.*)?$"), timeout=20000)
    await expect(page.get_by_role("heading", name=re.compile("Dashboard", re.I))).to_be_visible(timeout=15000)


async def main():
    async with async_playwright() as p:
        browser = await p.chromium.launch()

        desktop = await browser.new_context(viewport={"width": 1440, "height": 900})
        page = await desktop.new_page()

        # Public authentication UI.
        await page.goto(BASE_URL + "/", wait_until="domcontentloaded")
        await snap(page, "01-login-desktop.png", "Login")

        # Register a least-privileged employee account.
        await page.goto(BASE_URL + "/register", wait_until="domcontentloaded")
        await page.locator("#first-name").fill("Nandi")
        await page.locator("#last-name").fill("Mthembu")
        await page.locator("#work-email").fill(EMPLOYEE_EMAIL)
        await page.locator("#register-password").fill(EMPLOYEE_PASSWORD)
        await page.locator("#confirm-password").fill(EMPLOYEE_PASSWORD)
        await page.locator(".registration-terms input[type=checkbox]").check()
        await page.get_by_role("button", name="Create Account", exact=True).click()
        await page.wait_for_url(re.compile(r".*registration=success.*"), timeout=20000)
        await expect(page.get_by_text(re.compile("account.*created|registration", re.I))).to_be_visible(timeout=10000)

        # Employee journey.
        await login(page, EMPLOYEE_EMAIL, EMPLOYEE_PASSWORD)
        await snap(page, "02-employee-dashboard-desktop.png", "Employee dashboard")

        await page.goto(BASE_URL + "/profile", wait_until="domcontentloaded")
        await expect(page.get_by_role("heading", name="My Profile")).to_be_visible(timeout=15000)
        await snap(page, "03-employee-profile-desktop.png", "Employee profile")

        await page.goto(BASE_URL + "/attendance", wait_until="domcontentloaded")
        await expect(page.get_by_role("button", name="Clock in")).to_be_visible(timeout=15000)
        await page.get_by_role("button", name="Clock in").click()
        await expect(page.get_by_role("button", name="Clock out")).to_be_visible(timeout=15000)
        await snap(page, "04-attendance-clocked-in-desktop.png", "Attendance clocked in")
        await page.get_by_role("button", name="Clock out").click()
        await expect(page.get_by_role("button", name="Clock in")).to_be_visible(timeout=15000)
        await expect(page.get_by_text("Complete", exact=True)).to_be_visible(timeout=15000)

        await page.goto(BASE_URL + "/requests", wait_until="domcontentloaded")
        await page.get_by_label("Requested resource").fill("Finance reporting workspace")
        await page.get_by_label("Business reason").fill(
            "Required to prepare the monthly operational reporting pack for the finance team."
        )
        await page.get_by_role("button", name="Submit request", exact=True).click()
        await expect(page.get_by_text("Finance reporting workspace", exact=True)).to_be_visible(timeout=15000)
        await expect(page.get_by_text("Pending", exact=True).first).to_be_visible(timeout=15000)
        await snap(page, "05-access-request-desktop.png", "Employee access request")

        await page.goto(BASE_URL + "/security", wait_until="domcontentloaded")
        await expect(page.get_by_role("heading", name="Security")).to_be_visible(timeout=15000)
        await snap(page, "06-security-desktop.png", "Employee security")

        # Normal POST/antiforgery logout.
        await page.get_by_role("button", name="Sign out", exact=True).click()
        await page.wait_for_url(re.compile(r".*/(?:\?.*)?$"), timeout=20000)
        await expect(page.locator("#email")).to_be_visible(timeout=15000)

        # Administrator journey.
        await login(page, ADMIN_EMAIL, ADMIN_PASSWORD)

        await page.goto(BASE_URL + "/admin", wait_until="domcontentloaded")
        await expect(page.get_by_role("heading", name="Security Administration")).to_be_visible(timeout=15000)
        await snap(page, "07-admin-dashboard-desktop.png", "Administrator dashboard")

        await page.goto(BASE_URL + "/admin/users", wait_until="domcontentloaded")
        await expect(page.get_by_role("heading", name="User Management")).to_be_visible(timeout=15000)
        await expect(page.get_by_text(EMPLOYEE_EMAIL, exact=True)).to_be_visible(timeout=15000)
        await snap(page, "08-user-management-desktop.png", "User management")

        await page.goto(BASE_URL + "/admin/access-requests", wait_until="domcontentloaded")
        await expect(page.get_by_role("heading", name="Access Request Review")).to_be_visible(timeout=15000)
        await expect(page.get_by_text("Finance reporting workspace", exact=True)).to_be_visible(timeout=15000)
        await page.get_by_label("Decision note for Finance reporting workspace").fill(
            "Approved for the requested operational reporting responsibility."
        )
        await page.get_by_role("button", name="Approve", exact=True).click()
        await expect(page.get_by_text("Confirm access decision")).to_be_visible(timeout=10000)
        await page.get_by_role("button", name="Confirm decision", exact=True).click()
        await expect(page.get_by_text("Approved", exact=True).first).to_be_visible(timeout=15000)
        await snap(page, "09-access-review-approved-desktop.png", "Approved access request")

        await page.goto(BASE_URL + "/admin/audit-logs", wait_until="domcontentloaded")
        await expect(page.get_by_role("heading", name="Audit Logs")).to_be_visible(timeout=15000)
        await snap(page, "10-audit-logs-desktop.png", "Audit logs")

        await page.goto(BASE_URL + "/admin/security-events", wait_until="domcontentloaded")
        await expect(page.get_by_role("heading", name="Security Events")).to_be_visible(timeout=15000)
        await snap(page, "11-security-events-desktop.png", "Security events")

        # Mobile evidence uses a fresh employee session and normal viewport capture.
        mobile = await browser.new_context(viewport={"width": 390, "height": 844})
        mobile_page = await mobile.new_page()
        await mobile_page.goto(BASE_URL + "/", wait_until="domcontentloaded")
        await snap(mobile_page, "12-login-mobile.png", "Login mobile")
        await login(mobile_page, EMPLOYEE_EMAIL, EMPLOYEE_PASSWORD)
        await snap(mobile_page, "13-employee-dashboard-mobile.png", "Employee dashboard mobile")

        await mobile.close()
        await desktop.close()
        await browser.close()

    (OUT / "runtime-e2e-report.json").write_text(
        json.dumps(
            {
                "base_url": BASE_URL,
                "employee": EMPLOYEE_EMAIL,
                "checks": checks,
                "status": "passed",
            },
            indent=2,
        ),
        encoding="utf-8",
    )


if __name__ == "__main__":
    asyncio.run(main())
