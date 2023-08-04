declare const hljs: any;

(() => {
    hljs.highlightAll();
    document.querySelectorAll("pre code").forEach((e: HTMLElement) => e.style.paddingTop = "0");
})();
