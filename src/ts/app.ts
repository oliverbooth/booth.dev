import UI from "./UI";
import Input from "./Input";
import Callout from "./Callout";

declare const Handlebars: any;
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

    Handlebars.registerHelper("urlEncode", encodeURIComponent);

    Input.registerShortcut(Input.KONAMI_CODE, () => {
        window.open("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "_blank");
    });

    UI.updateUI();

    let avatarType = 0;
    const headshot = document.getElementById("index-headshot") as HTMLImageElement;
    if (headshot) {
        headshot.addEventListener("click", (ev: MouseEvent) => {
            if (avatarType === 0) {
                headshot.classList.add("headshot-spin", "headshot-spin-start");
                setTimeout(() => {
                    headshot.classList.remove("headshot-spin-start");
                    headshot.src = "/img/avatar_512x512.jpg"
                    headshot.classList.add("headshot-spin", "headshot-spin-end");

                    setTimeout(() => {
                        headshot.classList.remove("headshot-spin", "headshot-spin-end");
                        avatarType = 1;
                    }, 800);
                }, 400);
            } else if (avatarType === 1) {
                headshot.classList.add("headshot-spin", "headshot-spin-start");
                setTimeout(() => {
                    headshot.classList.remove("headshot-spin-start");
                    headshot.src = "/img/headshot_512x512_2023.jpg"
                    headshot.classList.add("headshot-spin", "headshot-spin-end");

                    setTimeout(() => {
                        headshot.classList.remove("headshot-spin", "headshot-spin-end");
                        avatarType = 0;
                    }, 800);
                }, 400);
            }
        });
    }

    function setFavicon() {
        const darkMode = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches;
        const favicon = document.querySelector("link[rel~=\"icon\"]");
        // @ts-ignore
        favicon.href = `/img/${darkMode ? "favicon.png" : "favicon-dark.png"}`;
    }

    setFavicon();
    window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", setFavicon);
})();
