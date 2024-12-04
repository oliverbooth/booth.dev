import UI from "./UI";
import Input from "./Input";
import Callout from "./Callout";

declare const Prism: any;
declare const lucide: any;

(() => {
    Callout.foldAll();
    lucide.createIcons();

    Prism.languages.extend('markup', {});
    Prism.languages.hex = {
        'number': {
            pattern: /(?:[a-fA-F0-9]{3}){1,2}\b/i,
            lookbehind: true
        }
    };
    Prism.languages.binary = {
        'number': {
            pattern: /[10]+/i,
            lookbehind: true
        }
    };
    Prism.languages.insertBefore('custom', 'tag', {
        'mark': {
            pattern: /<\/?mark(?:\s+\w+(?:=(?:"[^"]*"|'[^']*'|[^\s'">=]+))?\s*|\s*)\/?>/,
            greedy: true
        }
    });

    Input.registerShortcut(Input.KONAMI_CODE, () => {
        window.open("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "_blank");
    });

    function setFavicon() {
        const darkMode = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches;
        const favicon = document.querySelector("link[rel~=\"icon\"]");
        // @ts-ignore
        favicon.href = `/img/${darkMode ? "favicon.png" : "favicon-dark.png"}`;
    }

    setFavicon();
    window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", setFavicon);

    document.getElementById("theme-toggle").addEventListener("click", () => {
        document.body.classList.toggle("dark");
        if (getCookie("theme") !== "light") {
            setCookie("theme", "light", 30);
        } else {
            setCookie("theme", "", -100);
            setCookie("theme", "dark", 30);
        }
    });

    UI.updateUI();

    function setCookie(name: string, value: any, expiry: number) {
        const d = new Date();
        d.setTime(d.getTime() + (expiry * 24 * 60 * 60 * 1000));
        const expires = `expires=${d.toUTCString()}`;
        document.cookie = `${name}=${value};${expires};SameSite=None;secure;path=/`;
    }

    function getCookie(name: string) {
        const cookieName = `${name}=`;
        const decodedCookie = decodeURIComponent(document.cookie);
        const cookies = decodedCookie.split(';');

        for (let cookie of cookies) {
            if (cookie.startsWith(cookieName)) {
                return cookie.substring(cookieName.length, cookie.length);
            }
        }

        return "";
    }
})();
