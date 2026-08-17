/**
 * Initializes the favicon based on the user's preferred color scheme.
 */
export function initFavicon() {
    setFavicon();
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', setFavicon);
}

function setFavicon() {
    const darkMode = window.matchMedia?.('(prefers-color-scheme: dark)').matches ?? false;
    const favicon = document.querySelector('link[rel~=\'icon\']') as HTMLLinkElement;
    if (!favicon) {
        return;
    }

    favicon.href = `/img/${darkMode ? 'favicon.png' : 'favicon-dark.png'}`;
}
