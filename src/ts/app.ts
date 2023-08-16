import API from "./API";
import UI from "./UI";
import Input from "./Input";
import Author from "./Author";

declare const Handlebars: any;
declare const Prism: any;

(() => {
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

    const blogPost = UI.blogPost;
    if (blogPost) {
        const id = blogPost.dataset.blogId;
        API.getBlogPost(id).then((post) => {
            blogPost.innerHTML = post.content;
            UI.updateUI(blogPost);
        });
    }

    const blogPostContainer = UI.blogPostContainer;
    if (blogPostContainer) {
        const authors = [];
        const template = Handlebars.compile(UI.blogPostTemplate.innerHTML);
        API.getBlogPostCount().then(async (count) => {
            for (let i = 0; i <= count / 10; i++) {
                const posts = await API.getBlogPosts(i);
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
