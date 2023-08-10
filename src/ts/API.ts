import BlogPost from "./BlogPost";
import Author from "./Author";

class API {
    private static readonly BASE_URL: string = "/api";
    private static readonly BLOG_URL: string = "/blog";

    static async getBlogPostCount(): Promise<number> {
        const response = await fetch(`${API.BASE_URL + API.BLOG_URL}/count`);
        const text = await response.text();
        return JSON.parse(text).count;
    }
    
    static async getBlogPosts(skip: number, take: number): Promise<BlogPost[]> {
        const response = await fetch(`${API.BASE_URL + API.BLOG_URL}/all/${skip}/${take}`);
        const text = await response.text();
        return JSON.parse(text).map(obj => new BlogPost(obj));
    }
    
    static async getAuthor(id: number): Promise<Author> {
        const response = await fetch(`${API.BASE_URL + API.BLOG_URL}/author/${id}`);
        const text = await response.text();
        return new Author(JSON.parse(text));
    }
}

export default API;