/**
 * Keeps a mounted giscus comment widget in step with the site's own theme toggle, using giscus's documented
 * `postMessage` API (https://giscus.app/ - "Advanced usage").
 */
export function initGiscusTheme(): void {
    document.addEventListener('themechange', (event) => {
        const {theme} = (event as CustomEvent<{ theme: string }>).detail;
        const iframe = document.querySelector<HTMLIFrameElement>('iframe.giscus-frame');
        iframe?.contentWindow?.postMessage({giscus: {setConfig: {theme}}}, 'https://giscus.app');
    });
}
