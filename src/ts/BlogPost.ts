import BlogUrl from "./BlogUrl";

class BlogPost {
    private readonly _id: string;
    private readonly _commentsEnabled: boolean;
    private readonly _title: string;
    private readonly _excerpt: string;
    private readonly _content: string;
    private readonly _authorId: string;
    private readonly _published: Date;
    private readonly _updated?: Date;
    private readonly _url: BlogUrl;
    private readonly _trimmed: boolean;
    private readonly _identifier: string;
    private readonly _humanizedTimestamp: string;
    private readonly _formattedDate: string;

    constructor(json: any) {
        this._id = json.id;
        this._commentsEnabled = json.commentsEnabled;
        this._title = json.title;
        this._excerpt = json.excerpt;
        this._content = json.content;
        this._authorId = json.author;
        this._published = new Date(json.published * 1000);
        this._updated = (json.updated && new Date(json.updated * 1000)) || null;
        this._url = new BlogUrl(json.url);
        this._trimmed = json.trimmed;
        this._identifier = json.identifier;
        this._humanizedTimestamp = json.humanizedTimestamp;
        this._formattedDate = json.formattedDate;
    }

    get id(): string {
        return this._id;
    }

    get commentsEnabled(): boolean {
        return this._commentsEnabled;
    }

    get title(): string {
        return this._title;
    }

    get excerpt(): string {
        return this._excerpt;
    }

    get content(): string {
        return this._content;
    }

    get authorId(): string {
        return this._authorId;
    }

    get published(): Date {
        return this._published;
    }

    get updated(): Date {
        return this._updated;
    }

    get url(): BlogUrl {
        return this._url;
    }

    get trimmed(): boolean {
        return this._trimmed;
    }

    get identifier(): string {
        return this._identifier;
    }

    get humanizedTimestamp(): string {
        return this._humanizedTimestamp;
    }

    get formattedDate(): string {
        return this._formattedDate;
    }
}

export default BlogPost;