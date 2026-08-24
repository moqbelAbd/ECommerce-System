function showToast(message, type) {
    type = type || "success";

    var container = document.getElementById("toast-container");
    if (!container) {
        container = document.createElement("div");
        container.id = "toast-container";
        container.className = "toast-container";
        document.body.appendChild(container);
    }

    var toast = document.createElement("div");
    toast.className = "toast-item toast-" + type;

    var text = document.createElement("span");
    text.textContent = message;
    toast.appendChild(text);

    var closeBtn = document.createElement("button");
    closeBtn.type = "button";
    closeBtn.className = "toast-close";
    closeBtn.innerHTML = "&times;";
    toast.appendChild(closeBtn);

    container.appendChild(toast);

    requestAnimationFrame(function () {
        toast.classList.add("show");
    });

    var dismissed = false;
    var remove = function () {
        if (dismissed) return;
        dismissed = true;
        toast.classList.remove("show");
        toast.classList.add("hide");
        setTimeout(function () {
            toast.remove();
        }, 300);
    };

    var timer = setTimeout(remove, 4000);

    closeBtn.addEventListener("click", function () {
        clearTimeout(timer);
        remove();
    });
}
