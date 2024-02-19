import API from "./API";
import UI from "./UI";
import Input from "./Input";
import Author from "./Author";
import BlogPost from "./BlogPost";

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

    Handlebars.registerHelper("urlEncode", encodeURIComponent);

    function getQueryVariable(variable: string): string {
        const query = window.location.search.substring(1);
        const vars = query.split("&");
        for (const element of vars) {
            const pair = element.split("=");
            if (pair[0] == variable) {
                return pair[1];
            }
        }

        return null;
    }

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
                let posts: BlogPost[];

                const tag = getQueryVariable("tag");
                if (tag !== null && tag !== "") {
                    posts = await API.getBlogPostsByTag(decodeURIComponent(tag), i);
                } else {
                    posts = await API.getBlogPosts(i);
                }

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
    
    setInterval(() => {
        const countdown = document.querySelector("#usa-countdown p");
        const start = new Date().getTime();
        const end = Date.UTC(2024, 2, 7, 13, 20);
        const diff = end - start;
        let days = Math.floor(diff / (1000 * 60 * 60 * 24));
        let hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        let minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
        let seconds = Math.floor((diff % (1000 * 60)) / 1000);

        if (days < 0) days = 0
        if (hours < 0) hours = 0;
        if (minutes < 0) minutes = 0;
        if (seconds < 0) seconds = 0;
        
        countdown.innerHTML = `${days.toString().padStart(2, '0')} : ${hours.toString().padStart(2, '0')} : ${minutes.toString().padStart(2, '0')} : ${seconds.toString().padStart(2, '0')}`;
    }, 1000);
})();
