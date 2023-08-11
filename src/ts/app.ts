import API from "./API";
import UI from "./UI";
import Input from "./Input";
import Author from "./Author";

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
        const authors = [];
        const template = Handlebars.compile(UI.blogPostTemplate.innerHTML);
        API.getBlogPostCount().then(async (count) => {
            for (let i = 0; i < count; i += 5) {
                const posts = await API.getBlogPosts(i, 5);
                for (const post of posts) {
                    let author: Author;
                    if (authors[post.authorId]) {
                        author = authors[post.authorId];
                    } else {
                        author = await API.getAuthor(post.authorId);
                        authors[post.authorId] = author;
                    }

                    const card = UI.createBlogPostCard(template, post, author);
                    blogPostContainer.appendChild(card);
                    UI.updateUI(card);
                }
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
