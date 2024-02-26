import BlogPost from "../app/BlogPost";
import API from "../app/API";

(() => {
    getCurrentBlogPost().then(post => {
        if (!post) {
            return;
        }

        const saveButton = document.getElementById("save-button");
        const preview = document.getElementById("article-preview");
        const content = document.getElementById("content") as HTMLTextAreaElement;

        saveButton.addEventListener("click", async (e: MouseEvent) => {
            await savePost();
        });

        document.addEventListener("keydown", async (e: KeyboardEvent) => {
            if (e.ctrlKey && e.key === "s") {
                e.preventDefault();
                await savePost();
                preview.innerHTML = post.content;
            }
        });

        async function savePost(): Promise<void> {
            saveButton.classList.add("btn-primary");
            saveButton.classList.remove("btn-success");

            saveButton.setAttribute("disabled", "disabled");
            saveButton.innerHTML = '<i class="fa-solid fa-spinner fa-spin fa-fw"></i> Saving ...';

            post = await API.updatePost(post, content.value);

            saveButton.classList.add("btn-success");
            saveButton.classList.remove("btn-primary");
            saveButton.removeAttribute("disabled");
            saveButton.innerHTML = '<i class="fa-solid fa-circle-check fa-fw"></i> Saved';

            setTimeout(() => {
                saveButton.classList.add("btn-primary");
                saveButton.classList.remove("btn-success");
                saveButton.innerHTML = '<i class="fa-solid fa-floppy-disk fa-fw"></i> Save <span class="text-muted">(Ctrl+S)</span>';
            }, 2000);
        }
    });

    async function getCurrentBlogPost(): Promise<BlogPost> {
        const blogPostRef: Element = document.querySelector('input[type="hidden"][data-blog-pid]');

        if (blogPostRef) {
            const pid: string = blogPostRef.getAttribute("data-blog-pid");
            return await API.getBlogPost(pid);
        }

        return null;
    }
})();