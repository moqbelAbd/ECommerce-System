document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.getElementById("adminSidebar");
    const sidebarToggle = document.querySelector("[data-sidebar-toggle]");
    const backdrop = document.querySelector(".sidebar-backdrop");

    if (sidebarToggle) {
        sidebarToggle.addEventListener("click", function () {
            sidebar.classList.toggle("collapsed");
            document.querySelector(".admin-main").classList.toggle("expanded");
        });
    }

    // الوضع الليلي (Dark Mode Toggle)
    const themeToggle = document.querySelector("[data-theme-toggle]");
    if (themeToggle) {
        themeToggle.addEventListener("click", function () {
            document.body.classList.toggle("dark-mode");
            const icon = this.querySelector("[data-theme-icon]");
            if (document.body.classList.contains("dark-mode")) {
                icon.classList.remove("bi-moon-stars");
                icon.classList.add("bi-sun");
            } else {
                icon.classList.remove("bi-sun");
                icon.classList.add("bi-moon-stars");
            }
        });
    }
});