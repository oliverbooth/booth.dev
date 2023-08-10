import API from "./API";
import TimeUtility from "./TimeUtility";
import UI from "./UI";

declare const bootstrap: any;
declare const katex: any;

(() => {
    const blogPostContainer = UI.blogPostContainer;
    if (blogPostContainer) {
        API.getBlogPostCount().then(async (count) => {
            for (let i = 0; i < count; i++) {
                const posts = await API.getBlogPosts(i, 5);
                for (const post of posts) {
                    const author = await API.getAuthor(post.authorId);

                    const card = document.createElement("div") as HTMLDivElement;
                    card.classList.add("card");
                    card.classList.add("blog-card");
                    card.classList.add("animate__animated");
                    card.classList.add("animate__fadeIn");
                    card.style.marginBottom = "50px";

                    const cardBody = document.createElement("div");
                    cardBody.classList.add("card-body");
                    card.appendChild(cardBody);

                    const postTitle = document.createElement("h2");
                    postTitle.classList.add("card-title");
                    cardBody.appendChild(postTitle);

                    const titleLink = document.createElement("a");
                    titleLink.href = post.url;
                    titleLink.innerText = post.title;
                    postTitle.appendChild(titleLink);

                    const metadata = document.createElement("p");
                    metadata.classList.add("text-muted");
                    cardBody.appendChild(metadata);

                    const authorIcon = document.createElement("img");
                    authorIcon.classList.add("blog-author-icon");
                    authorIcon.src = `https://gravatar.com/avatar/${author.avatarHash}?s=28`;
                    authorIcon.alt = author.name;
                    metadata.appendChild(authorIcon);

                    const authorName = document.createElement("span");
                    authorName.innerHTML = ` ${author.name} &bull; `;
                    metadata.appendChild(authorName);

                    const postDate = document.createElement("span");
                    if (post.updated) {
                        postDate.innerHTML = `Updated ${TimeUtility.formatRelativeTimestamp(post.updated)}`;
                    } else {
                        postDate.innerHTML = `Published ${TimeUtility.formatRelativeTimestamp(post.published)}`;
                    }
                    metadata.appendChild(postDate);

                    if (post.commentsEnabled) {
                        const bullet = document.createElement("span");
                        bullet.innerHTML = " &bull; ";
                        metadata.appendChild(bullet);

                        const commentCount = document.createElement("a");
                        commentCount.href = post.url + "#disqus_thread";
                        commentCount.innerHTML = "0 Comments";
                        commentCount.setAttribute("data-disqus-identifier", post.identifier);
                        metadata.appendChild(commentCount);
                    }

                    const postExcerpt = document.createElement("p");
                    postExcerpt.innerHTML = post.excerpt;
                    cardBody.appendChild(postExcerpt);

                    if (post.trimmed) {
                        const readMoreLink = document.createElement("a");
                        readMoreLink.href = post.url;
                        readMoreLink.innerHTML = "Read more &hellip;";
                        cardBody.appendChild(readMoreLink);
                    }

                    blogPostContainer.appendChild(card);
                }

                i += 4;
            }

            const disqusCounter = document.createElement("script");
            disqusCounter.id = "dsq-count-scr";
            disqusCounter.src = "https://oliverbooth-dev.disqus.com/count.js";
            disqusCounter.async = true;

            const spinner = document.querySelector("#blog_loading_spinner");
            if (spinner) {
                spinner.classList.add("removed");
                setTimeout(() => spinner.remove(), 1100);
            }
        });
    }

    document.querySelectorAll("pre code").forEach((block) => {
        let content = block.textContent;
        if (content.trim().split("\n").length > 1) {
            block.parentElement.classList.add("line-numbers");
        }

        content = block.innerHTML;
        // @ts-ignore
        content = content.replaceAll("&lt;mark&gt;", "<mark>");
        // @ts-ignore
        content = content.replaceAll("&lt;/mark&gt;", "</mark>");
        block.innerHTML = content;
    });

    const tex = document.getElementsByClassName("math");
    Array.prototype.forEach.call(tex, function (el) {
        let content = el.textContent.trim();
        if (content.startsWith("\\[")) content = content.slice(2);
        if (content.endsWith("\\]")) content = content.slice(0, -2);

        katex.render(content, el);
    });

    const timestamps = document.querySelectorAll("span[data-timestamp][data-format]");
    timestamps.forEach((timestamp) => {
        const seconds = parseInt(timestamp.getAttribute("data-timestamp"));
        const format = timestamp.getAttribute("data-format");
        const date = new Date(seconds * 1000);

        const shortTimeString = date.toLocaleTimeString([], {hour: "2-digit", minute: "2-digit"});
        const shortDateString = date.toLocaleDateString([], {day: "2-digit", month: "2-digit", year: "numeric"});
        const longTimeString = date.toLocaleTimeString([], {hour: "2-digit", minute: "2-digit", second: "2-digit"});
        const longDateString = date.toLocaleDateString([], {day: "numeric", month: "long", year: "numeric"});
        const weekday = date.toLocaleString([], {weekday: "long"});
        timestamp.setAttribute("title", `${weekday}, ${longDateString} ${shortTimeString}`);

        switch (format) {
            case "t":
                timestamp.textContent = shortTimeString;
                break;

            case "T":
                timestamp.textContent = longTimeString;
                break;

            case "d":
                timestamp.textContent = shortDateString;
                break;

            case "D":
                timestamp.textContent = longDateString;
                break;

            case "f":
                timestamp.textContent = `${longDateString} at ${shortTimeString}`
                break;

            case "F":
                timestamp.textContent = `${weekday}, ${longDateString} at ${shortTimeString}`
                break;

            case "R":
                setInterval(() => {
                    timestamp.textContent = TimeUtility.formatRelativeTimestamp(date);
                }, 1000);
                break;
        }
    });

    document.querySelectorAll("[title]").forEach((el) => {
        el.setAttribute("data-bs-toggle", "tooltip");
        el.setAttribute("data-bs-placement", "bottom");
        el.setAttribute("data-bs-html", "true");
        el.setAttribute("data-bs-title", el.getAttribute("title"));
    });

    const list = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    list.forEach((el: Element) => new bootstrap.Tooltip(el));
})();
