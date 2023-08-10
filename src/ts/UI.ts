class UI {
    public static get blogPostContainer(): HTMLDivElement {
        return document.querySelector("#all-blog-posts");
    }

    public static get blogPostTemplate(): HTMLDivElement {
        return document.querySelector("#blog-post-template");
    }
}

export default UI;