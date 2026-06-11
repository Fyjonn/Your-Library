

document.addEventListener("DOMContentLoaded", function () {

    const path = window.location.pathname;
    const isLogin = path.includes("/Account/Login");
    const isRegister = path.includes("/Account/Register");

    if (isLogin || isRegister) {
        if (!document.querySelector('link[href*="login_style.css"]')) {
            const loginStyles = document.createElement("link");
            loginStyles.rel = "stylesheet";
            loginStyles.href = "/css/login_style.css";
            document.head.appendChild(loginStyles);
        }

        const mainContent = document.querySelector("main[role='main']");
        if (mainContent) {
            mainContent.classList.add("account-identity-container");

            const h3s = mainContent.querySelectorAll("h3");
            h3s.forEach(h3 => {
                const text = h3.textContent.toLowerCase();
                if (text.includes("service") || text.includes("account")) {
                    const parentColumn = h3.closest(".col-md-6, .col-md-5, .col-md-4, .col-md-offset-2");
                    if (parentColumn) {
                        parentColumn.remove();
                    } else {
                        h3.remove();
                    }
                }
            });

            if (!document.querySelector(".identity-switcher")) {
                const switcher = document.createElement("div");
                switcher.className = "identity-switcher";
                switcher.innerHTML = `
                    <a href="/Identity/Account/Login" class="${isLogin ? 'active-tab' : ''}">Log In</a>
                    <a href="/Identity/Account/Register" class="${isRegister ? 'active-tab' : ''}">Register</a>
                `;

                const heading = mainContent.querySelector("h1");
                if (heading) {
                    heading.insertAdjacentElement("afterend", switcher);
                } else {
                    mainContent.insertBefore(switcher, mainContent.firstChild);
                }
            }
        }
    }
});