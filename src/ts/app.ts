declare const bootstrap: any;
declare const katex: any;

(() => {
    const formatRelativeTime = function (timestamp) {
        const now = new Date();
        // @ts-ignore
        const diff = now - timestamp;
        const suffix = diff < 0 ? 'from now' : 'ago';

        const seconds = Math.floor(diff / 1000);
        if (seconds < 60) {
            return `${seconds} second${seconds !== 1 ? 's' : ''} ${suffix}`;
        }

        const minutes = Math.floor(diff / 60000);
        if (minutes < 60) {
            return `${minutes} minute${minutes !== 1 ? 's' : ''} ${suffix}`;
        }

        const hours = Math.floor(diff / 3600000);
        if (hours < 24) {
            return `${hours} hour${hours !== 1 ? 's' : ''} ${suffix}`;
        }

        const days = Math.floor(diff / 86400000);
        if (days < 30) {
            return `${days} day${days !== 1 ? 's' : ''} ${suffix}`;
        }

        const months = Math.floor(diff / 2592000000);
        if (months < 12) {
            return `${months} month${months !== 1 ? 's' : ''} ${suffix}`;
        }

        const years = Math.floor(diff / 31536000000);
        return `${years} year${years !== 1 ? 's' : ''} ${suffix}`;
    };

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
                    timestamp.textContent = formatRelativeTime(date);
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
