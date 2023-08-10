import API from "./API";
import TimeUtility from "./TimeUtility";
import UI from "./UI";

declare const Handlebars: any;

(() => {
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
                setTimeout(() => spinner.remove(), 1100);
            }
        });
    }

    UI.updateUI();

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
})();
