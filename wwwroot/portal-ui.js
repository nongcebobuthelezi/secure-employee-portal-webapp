// Final UI behaviours that do not require an interactive Blazor render boundary.
(() => {
    const setSidebar = (open) => {
        const shell = document.querySelector('.employee-shell');
        if (!shell) return;
        shell.classList.toggle('sidebar-open', open);
        document.body.classList.toggle('portal-nav-open', open);
        document.querySelectorAll('[data-sidebar-toggle]').forEach((button) => {
            button.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
    };

    document.addEventListener('click', (event) => {
        const toggle = event.target.closest('[data-sidebar-toggle]');
        if (toggle) {
            const shell = document.querySelector('.employee-shell');
            setSidebar(!shell?.classList.contains('sidebar-open'));
            return;
        }

        if (event.target.closest('[data-sidebar-close]')) {
            setSidebar(false);
            return;
        }

        const navLink = event.target.closest('.employee-sidebar a');
        if (navLink && window.matchMedia('(max-width: 900px)').matches) {
            setSidebar(false);
            return;
        }

        const passwordToggle = event.target.closest('[data-password-toggle]');
        if (passwordToggle) {
            const id = passwordToggle.getAttribute('data-password-toggle');
            const input = id ? document.getElementById(id) : null;
            if (!(input instanceof HTMLInputElement)) return;

            const reveal = input.type === 'password';
            input.type = reveal ? 'text' : 'password';
            passwordToggle.setAttribute('aria-pressed', reveal ? 'true' : 'false');
            passwordToggle.setAttribute('aria-label', reveal ? 'Hide password' : 'Show password');
        }
    });

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') setSidebar(false);
    });

    window.addEventListener('resize', () => {
        if (window.innerWidth > 900) setSidebar(false);
    });

    window.securePortalAssistant = {
        focusInput: () => {
            const input = document.getElementById('secure-assistant-input');
            if (input instanceof HTMLInputElement) input.focus();
        },
        scrollToLatest: () => {
            const messages = document.getElementById('secure-assistant-messages');
            if (messages instanceof HTMLElement) messages.scrollTop = messages.scrollHeight;
        }
    };
})();
