declare const bootstrap: any;
declare const katex: any;

(() => {
    const list = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    list.forEach((el: Element) => new bootstrap.Tooltip(el));

    window.onload = function () {
        const tex = document.getElementsByClassName("math");
        Array.prototype.forEach.call(tex, function (el) {
            let content = el.textContent.trim();
            if (content.startsWith("\\[")) content = content.slice(2);
            if (content.endsWith("\\]")) content = content.slice(0, -2);

            katex.render(content, el);
        });
    };
})();
