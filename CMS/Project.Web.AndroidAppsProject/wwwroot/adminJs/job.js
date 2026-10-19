"use strict";

document.addEventListener("submit", async function (event) {
    const form = event.target;
    if (!form.matches(".job-form")) return;
    event.preventDefault();
    if (form.dataset.submitting === "true") return;
    if (form.matches(".job-review") && !window.confirm("اثر عملیات را بررسی کرده‌اید و نمونهٔ قبلی برنامه متوقف است؟")) return;
    form.dataset.submitting = "true";
    const button = form.querySelector('button[type="submit"]');
    const message = document.getElementById("job-message");
    button.disabled = true;
    message.hidden = true;
    let failureMessage = "ارتباط با سرور برقرار نشد؛ دوباره تلاش کنید.";
    try {
        const response = await fetch(form.action, { method: "POST", body: new FormData(form), credentials: "same-origin", headers: { "X-Requested-With": "XMLHttpRequest" } });
        if (response.redirected || response.status === 401 || response.status === 403) {
            failureMessage = "برای مدیریت جاب‌ها دوباره با حساب مدیر وارد شوید.";
            throw new Error();
        }
        let data;
        try { data = await response.json(); }
        catch { failureMessage = "پاسخ سرور قابل‌خواندن نیست؛ صفحه را تازه‌سازی کنید."; throw new Error(); }
        if (!response.ok || data.status !== "1") {
            const errors = Array.isArray(data.errors) ? data.errors.join("؛ ") : data.message;
            failureMessage = errors || "ثبت تغییرات انجام نشد.";
            throw new Error();
        }
        window.location.reload();
    } catch {
        message.textContent = failureMessage;
        message.className = "alert alert-danger";
        message.hidden = false;
        button.disabled = false;
        delete form.dataset.submitting;
    }
});
