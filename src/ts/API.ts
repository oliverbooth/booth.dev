import BlogPost from "./BlogPost";
import Author from "./Author";

class API {
    private static readonly BASE_URL: string = "https://api.oliverbooth.dev";
    private static readonly BLOG_URL: string = "/blog";

    static async getBlogPostCount(): Promise<number> {
        const response = await API.getResponse(`count`);
        return response.count;
    }

    static async getBlogPost(id: string): Promise<BlogPost> {
        const response = await API.getResponse(`post/${id}`);
        return new BlogPost(response);
    }

    static async getBlogPosts(page: number): Promise<BlogPost[]> {
        const response = await API.getResponse(`posts/${page}`);
        return response.map(obj => new BlogPost(obj));
    }

    static async getAuthor(id: string): Promise<Author> {
        const response = await API.getResponse(`author/${id}`);
        return new Author(response);
    }
    
    private static async getResponse(url: string): Promise<any> {
        const response = await fetch(`${API.BASE_URL + API.BLOG_URL}/${url}`);
        if (response.status !== 200) {
            throw new Error("Invalid response from server");
        }
        
        const text = await response.text();
        return JSON.parse(text);
    }
}

export default API;