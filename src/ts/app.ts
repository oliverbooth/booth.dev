import API from "./API";
import UI from "./UI";
import Input from "./Input";

const pkg = require("../../package.json");

declare const Handlebars: any;
declare const Prism: any;

(() => {
    Prism.languages.extend('markup', {});
    Prism.languages.insertBefore('custom', 'tag', {
        'mark': {
            pattern: /<\/?mark(?:\s+\w+(?:=(?:"[^"]*"|'[^']*'|[^\s'">=]+))?\s*|\s*)\/?>/,
            greedy: true
        }
    });

    let isCtrl = false;
    document.addEventListener('keyup', (e) => {
        if (e.ctrlKey) isCtrl = false;
    });

    document.addEventListener('keydown', (e) => {
        if (e.ctrlKey) isCtrl = true;
        if (isCtrl && e.key === "u") {
            window.open(pkg.repository.url, "_blank");
            return false;
        }
    });

    Input.registerShortcut(Input.KONAMI_CODE, () => {
        window.open("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "_blank");
    });

    const blogPostContainer = UI.blogPostContainer;
    if (blogPostContainer) {
        const template = Handlebars.compile(UI.blogPostTemplate.innerHTML);
        API.getBlogPostCount().then(async (count) => {
            for (let i = 0; i < count; i++) {
                const posts = await API.getBlogPosts(i, 5);
                for (const post of posts) {
                    const author = await API.getAuthor(post.authorId);
                    const card = UI.createBlogPostCard(template, post, author);
                    blogPostContainer.appendChild(card);
                    UI.updateUI(card);
                }

                i += 4;
            }

            document.body.appendChild(UI.createDisqusCounterScript());

            const spinner = document.querySelector("#blog-loading-spinner");
            if (spinner) {
                spinner.classList.add("removed");
                setTimeout(() => spinner.remove(), 1000);
            }
        });
    }

    UI.updateUI();
})();
