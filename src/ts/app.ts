declare const bootstrap: any;

(() => {
    const list = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    list.forEach((el: Element) => new bootstrap.Tooltip(el));
})();
