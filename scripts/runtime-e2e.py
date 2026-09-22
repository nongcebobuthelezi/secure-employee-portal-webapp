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
    await page.wait_for_timeout(500)

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
        offenders = await page.evaluate("""() => Array.from(document.querySelectorAll('*'))
            .map(el => {
                const r = el.getBoundingClientRect();
                const s = getComputedStyle(el);
                return {
                    tag: el.tagName,
                    cls: el.className && String(el.className).slice(0, 140),
                    left: Math.round(r.left),
                    right: Math.round(r.right),
                    width: Math.round(r.width),
                    position: s.position,
                    display: s.display,
                    transform: s.transform
                };
            })
            .filter(x => x.right > window.innerWidth + 2 || x.left < -2)
            .slice(0, 40)""")
        print("OVERFLOW_DIAGNOSTIC", json.dumps({
            "label": label,
            "viewport": await page.evaluate("() => ({w: innerWidth, h: innerHeight})"),
            "scroll_width": await page.evaluate("() => document.documentElement.scrollWidth"),
            "offenders": offenders
        }))
        await page.screenshot(path=str(OUT / "overflow-diagnostic.png"), full_page=False)
        raise AssertionError(f"{label}: document-level horizontal overflow: {overflow}px")

    checks.append({"page": label, "url": page.url, "horizontal_overflow_px": overflow})


async def snap(page, filename, label):
    await page.evaluate("() => window.scrollTo(0, 0)")
    await page.wait_for_timeout(300)
    await assert_page_healthy(page, label)
    await page.screenshot(path=str(OUT / filename), full_page=False)


async def open_and_snap(page, route, filename, label):
    await page.goto(BASE_URL + route, wait_until="domcontentloaded")
    await page.wait_for_timeout(650)
    await snap(page, filename, label)


async def login(page, email, password):
    await page.goto(BASE_URL + "/", wait_until="domcontentloaded")
    await page.locator("#email").fill(email)
    await page.locator("#password").fill(password)
    await page.get_by_role("button", name="Sign In", exact=True).click()
    await page.wait_for_url(re.compile(r".*/dashboard(?:\?.*)?$"), timeout=20000)
    await expect(page.locator("h1").first).to_be_visible(timeout=15000)


async def main():
    async with async_playwright() as p:
        browser = await p.chromium.launch()

        # A slightly taller desktop viewport captures complete, polished page frames
        # without the sticky-sidebar distortion produced by full-page screenshots.
        desktop = await browser.new_context(viewport={"width": 1440, "height": 1000})
        page = await desktop.new_page()

        # ---------------------------------------------------------------------
        # PUBLIC AUTHENTICATION EXPERIENCE
        # ---------------------------------------------------------------------
        await open_and_snap(page, "/", "01-login-desktop.png", "Login")
        await open_and_snap(page, "/register", "02-register-desktop.png", "Register")
        await open_and_snap(page, "/forgot-password", "03-forgot-password-desktop.png", "Forgot password")

        # Register a least-privileged employee account using the real UI.
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

        # ---------------------------------------------------------------------
        # EMPLOYEE EXPERIENCE
        # ---------------------------------------------------------------------
        await login(page, EMPLOYEE_EMAIL, EMPLOYEE_PASSWORD)
        await snap(page, "04-employee-dashboard-desktop.png", "Employee dashboard")

        await open_and_snap(page, "/profile", "05-employee-profile-desktop.png", "Employee profile")
        await expect(page.get_by_role("heading", name="My Profile")).to_be_visible(timeout=15000)

        await open_and_snap(page, "/security", "06-employee-security-desktop.png", "Employee security")
        await expect(page.get_by_role("heading", name="My Security", exact=True)).to_be_visible(timeout=15000)

        await page.goto(BASE_URL + "/attendance", wait_until="domcontentloaded")
        await expect(page.get_by_role("button", name="Clock in")).to_be_visible(timeout=15000)
        await page.get_by_role("button", name="Clock in").click()
        await expect(page.get_by_role("button", name="Clock out")).to_be_visible(timeout=15000)
        await snap(page, "07-attendance-clocked-in-desktop.png", "Attendance clocked in")
        await page.get_by_role("button", name="Clock out").click()
        await expect(page.get_by_role("button", name="Clock in")).to_be_visible(timeout=15000)
        await expect(page.get_by_text("Complete", exact=True)).to_be_visible(timeout=15000)

        await page.goto(BASE_URL + "/requests", wait_until="domcontentloaded")
        await page.wait_for_timeout(900)
        resource_input = page.get_by_label("Requested resource")
        reason_input = page.get_by_label("Business reason")
        await resource_input.fill("Finance reporting workspace")
        await reason_input.fill(
            "Required to prepare the monthly operational reporting pack for the finance team."
        )
        # Interactive Server hydration can replace prerendered form controls shortly
        # after navigation. Verify the live values before submitting the real form.
        if await resource_input.input_value() != "Finance reporting workspace":
            await resource_input.fill("Finance reporting workspace")
        await expect(resource_input).to_have_value("Finance reporting workspace")
        await expect(reason_input).to_have_value(
            "Required to prepare the monthly operational reporting pack for the finance team."
        )
        await page.get_by_role("button", name="Submit request", exact=True).click()
        await expect(page.get_by_text("Finance reporting workspace", exact=True)).to_be_visible(timeout=15000)
        await expect(page.get_by_text("Pending", exact=True).first).to_be_visible(timeout=15000)
        await snap(page, "08-access-request-desktop.png", "Employee access request")

        await open_and_snap(page, "/account-activity", "09-account-activity-desktop.png", "Account activity")
        await expect(page.get_by_role("heading", name="Account Activity")).to_be_visible(timeout=15000)

        await open_and_snap(page, "/support", "10-help-support-desktop.png", "Help and support")
        await expect(page.get_by_role("heading", name=re.compile("Help", re.I)).first).to_be_visible(timeout=15000)

        # Employee role must be denied from administrator routes.
        await page.goto(BASE_URL + "/admin", wait_until="domcontentloaded")
        await page.wait_for_url(re.compile(r".*/access-denied(?:\?.*)?$"), timeout=15000)
        await expect(page.get_by_role("heading", name="Access denied")).to_be_visible(timeout=15000)
        await snap(page, "11-access-denied-desktop.png", "Access denied")

        # Return to the authenticated shell before exercising the normal
        # POST/antiforgery logout control.
        await page.goto(BASE_URL + "/dashboard", wait_until="domcontentloaded")
        await expect(page.get_by_role("button", name="Sign out", exact=True)).to_be_visible(timeout=15000)
        await page.get_by_role("button", name="Sign out", exact=True).click()
        await page.wait_for_url(re.compile(r".*/(?:\?.*)?$"), timeout=20000)
        await expect(page.locator("#email")).to_be_visible(timeout=15000)

        # ---------------------------------------------------------------------
        # ADMINISTRATOR + MANAGER EXPERIENCE
        # ---------------------------------------------------------------------
        await login(page, ADMIN_EMAIL, ADMIN_PASSWORD)

        await open_and_snap(page, "/admin", "12-admin-dashboard-desktop.png", "Administrator dashboard")
        await expect(page.get_by_role("heading", name="Security Administration")).to_be_visible(timeout=15000)

        await page.goto(BASE_URL + "/admin/users", wait_until="domcontentloaded")
        await expect(page.get_by_role("heading", name="User Management")).to_be_visible(timeout=15000)
        employee_row = page.locator("tr").filter(has_text=EMPLOYEE_EMAIL)
        await expect(employee_row).to_be_visible(timeout=15000)
        await snap(page, "13-user-management-desktop.png", "User management")

        await open_and_snap(page, "/admin/users/create", "14-create-employee-desktop.png", "Create employee account")
        await expect(page.get_by_role("heading", name="Create Employee Account")).to_be_visible(timeout=15000)

        # Follow the real Manage link for the employee created earlier.
        await page.goto(BASE_URL + "/admin/users", wait_until="domcontentloaded")
        employee_row = page.locator("tr").filter(has_text=EMPLOYEE_EMAIL)
        manage_link = employee_row.get_by_role("link", name=re.compile("Manage"))
        detail_href = await manage_link.get_attribute("href")
        if not detail_href:
            raise AssertionError("User Management did not expose a Manage link for the runtime employee.")
        await open_and_snap(page, detail_href, "15-employee-account-desktop.png", "Employee account administration")
        await expect(page.get_by_role("heading", name="Employee Account")).to_be_visible(timeout=15000)

        await open_and_snap(page, "/admin/roles", "16-roles-permissions-desktop.png", "Roles and permissions")
        await expect(page.get_by_role("heading", name=re.compile("Roles", re.I)).first).to_be_visible(timeout=15000)

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
        await snap(page, "17-access-review-approved-desktop.png", "Approved access request")

        await open_and_snap(page, "/admin/security-events", "18-security-events-desktop.png", "Security events")
        await expect(page.get_by_role("heading", name="Security Events")).to_be_visible(timeout=15000)

        await open_and_snap(page, "/admin/audit-logs", "19-audit-logs-desktop.png", "Audit logs")
        await expect(page.get_by_role("heading", name="Audit Logs")).to_be_visible(timeout=15000)

        await open_and_snap(page, "/manager", "20-manager-team-desktop.png", "Manager team view")
        await expect(page.get_by_role("heading", name="My Team")).to_be_visible(timeout=15000)

        # ---------------------------------------------------------------------
        # RESPONSIVE EVIDENCE
        # ---------------------------------------------------------------------
        mobile = await browser.new_context(viewport={"width": 390, "height": 844})
        mobile_page = await mobile.new_page()

        await open_and_snap(mobile_page, "/", "21-login-mobile.png", "Login mobile")
        await login(mobile_page, EMPLOYEE_EMAIL, EMPLOYEE_PASSWORD)
        await snap(mobile_page, "22-employee-dashboard-mobile.png", "Employee dashboard mobile")

        await open_and_snap(mobile_page, "/profile", "23-employee-profile-mobile.png", "Employee profile mobile")
        await open_and_snap(mobile_page, "/attendance", "24-attendance-mobile.png", "Attendance mobile")

        await mobile.close()

        admin_mobile = await browser.new_context(viewport={"width": 390, "height": 844})
        admin_mobile_page = await admin_mobile.new_page()
        await login(admin_mobile_page, ADMIN_EMAIL, ADMIN_PASSWORD)
        await open_and_snap(admin_mobile_page, "/admin", "25-admin-dashboard-mobile.png", "Administrator dashboard mobile")
        await admin_mobile.close()

        await desktop.close()
        await browser.close()

    (OUT / "runtime-e2e-report.json").write_text(
        json.dumps(
            {
                "base_url": BASE_URL,
                "employee": EMPLOYEE_EMAIL,
                "checks": checks,
                "status": "passed",
                "screenshot_count": len(list(OUT.glob("*.png"))),
            },
            indent=2,
        ),
        encoding="utf-8",
    )


if __name__ == "__main__":
    asyncio.run(main())
