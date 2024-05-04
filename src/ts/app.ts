import API from "./API";
import UI from "./UI";
import Input from "./Input";
import Author from "./Author";
import BlogPost from "./BlogPost";
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
})();
