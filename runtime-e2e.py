from pathlib import Path
import re
from playwright.sync_api import sync_playwright, expect

BASE = "http://127.0.0.1:5080"
OUT = Path("runtime-evidence")
OUT.mkdir(exist_ok=True)

EMPLOYEE_EMAIL = "e2e.employee@secureportal.local"
EMPLOYEE_PASSWORD = "E2eEmployee!2026"
ADMIN_EMAIL = "runtime.admin@secureportal.local"
ADMIN_PASSWORD = "RuntimeAdmin!2026"


def shot(page, name: str):
    page.evaluate("window.scrollTo(0, 0)")
    page.wait_for_timeout(350)
    page.screenshot(path=str(OUT / name), full_page=False)


def login(page, email: str, password: str):
    page.goto(f"{BASE}/", wait_until="networkidle")
    page.locator("#email").fill(email)
    page.locator("#password").fill(password)
    page.locator("button.sign-in-button").click()
    page.wait_for_url(re.compile(r".*/dashboard(?:\?.*)?$"), timeout=15000)
    expect(page.get_by_role("heading", name=re.compile("Welcome|Good|Hello", re.I))).to_be_visible(timeout=10000)


with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)

    # Anonymous surface + redirect boundary.
    anonymous = browser.new_context(viewport={"width": 1440, "height": 900})
    page = anonymous.new_page()
    response = page.goto(f"{BASE}/", wait_until="networkidle")
    assert response and response.ok, f"Login page failed: {response.status if response else 'no response'}"
    expect(page.get_by_role("heading", name="Welcome back")).to_be_visible()
    shot(page, "01-login.png")

    page.goto(f"{BASE}/admin", wait_until="networkidle")
    assert page.url.startswith(BASE + "/?returnUrl="), f"Anonymous admin route did not redirect safely: {page.url}"
    anonymous.close()

    # Public registration + employee self-service workflow.
    employee = browser.new_context(viewport={"width": 1440, "height": 900})
    page = employee.new_page()
    page.goto(f"{BASE}/register", wait_until="networkidle")
    page.locator("#first-name").fill("Avery")
    page.locator("#last-name").fill("Mokoena")
    page.locator("#work-email").fill(EMPLOYEE_EMAIL)
    page.locator("#register-password").fill(EMPLOYEE_PASSWORD)
    page.locator("#confirm-password").fill(EMPLOYEE_PASSWORD)
    page.locator("input[type=checkbox]").check()
    page.get_by_role("button", name="Create Account").click()
    page.wait_for_url(re.compile(r".*/\?registration=success$"), timeout=15000)

    login(page, EMPLOYEE_EMAIL, EMPLOYEE_PASSWORD)
    shot(page, "02-employee-dashboard.png")

    page.goto(f"{BASE}/profile", wait_until="networkidle")
    expect(page.get_by_role("heading", name=re.compile("Profile", re.I))).to_be_visible()
    shot(page, "03-profile.png")

    page.goto(f"{BASE}/attendance", wait_until="networkidle")
    expect(page.get_by_role("heading", name=re.compile("Attendance", re.I))).to_be_visible()
    page.get_by_role("button", name="Clock in").click()
    expect(page.get_by_role("button", name="Clock out")).to_be_visible(timeout=10000)
    shot(page, "04-attendance-clocked-in.png")
    page.get_by_role("button", name="Clock out").click()
    expect(page.get_by_role("button", name="Clock in")).to_be_visible(timeout=10000)

    page.goto(f"{BASE}/requests", wait_until="networkidle")
    expect(page.get_by_role("heading", name=re.compile("Access", re.I))).to_be_visible()
    page.locator("form input").fill("Finance reporting workspace")
    page.locator("form textarea").fill("Required for the runtime verification workflow and approved reporting access.")
    page.get_by_role("button", name="Submit request").click()
    expect(page.get_by_text("Pending", exact=True).first).to_be_visible(timeout=10000)
    shot(page, "05-access-request-pending.png")
    employee.close()

    # Administrator runtime workflow.
    admin = browser.new_context(viewport={"width": 1440, "height": 900})
    page = admin.new_page()
    login(page, ADMIN_EMAIL, ADMIN_PASSWORD)

    page.goto(f"{BASE}/admin", wait_until="networkidle")
    expect(page.get_by_role("heading", name="Security Administration")).to_be_visible()
    shot(page, "06-admin-dashboard.png")

    page.goto(f"{BASE}/admin/users", wait_until="networkidle")
    expect(page.get_by_role("heading", name=re.compile("User", re.I))).to_be_visible()
    expect(page.get_by_text(EMPLOYEE_EMAIL, exact=False)).to_be_visible(timeout=10000)
    shot(page, "07-admin-users.png")

    page.goto(f"{BASE}/admin/access-requests", wait_until="networkidle")
    expect(page.get_by_text("Finance reporting workspace", exact=False)).to_be_visible(timeout=10000)
    shot(page, "08-admin-access-review.png")
    page.get_by_role("button", name="Approve").first.click()
    expect(page.get_by_role("button", name="Confirm decision")).to_be_visible(timeout=10000)
    page.get_by_role("button", name="Confirm decision").click()
    expect(page.get_by_text("Approved", exact=True).first).to_be_visible(timeout=10000)

    page.goto(f"{BASE}/admin/security-events", wait_until="networkidle")
    expect(page.get_by_role("heading", name=re.compile("Security", re.I))).to_be_visible()
    shot(page, "09-security-events.png")

    page.goto(f"{BASE}/admin/audit-logs", wait_until="networkidle")
    expect(page.get_by_role("heading", name=re.compile("Audit", re.I))).to_be_visible()
    shot(page, "10-audit-logs.png")

    page.goto(f"{BASE}/admin/roles", wait_until="networkidle")
    expect(page.get_by_role("heading", name=re.compile("Roles|Permissions", re.I))).to_be_visible()
    shot(page, "11-roles-permissions.png")

    admin.close()
    browser.close()

print("Runtime browser workflow completed successfully.")