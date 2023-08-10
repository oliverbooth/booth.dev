class BlogPost {
    private readonly _id: number;
    private readonly _commentsEnabled: boolean;
    private readonly _title: string;
    private readonly _excerpt: string;
    private readonly _authorId: number;
    private readonly _published: Date;
    private readonly _updated?: Date;
    private readonly _url: string;
    private readonly _trimmed: boolean;
    private readonly _identifier: string;
    private readonly _humanizedTimestamp: string;

    constructor(json: any) {
        this._id = json.id;
        this._commentsEnabled = json.commentsEnabled;
        this._title = json.title;
        this._excerpt = json.excerpt;
        this._authorId = parseInt(json.author);
        this._published = new Date(json.published * 1000);
        this._updated = (json.updated && new Date(json.updated * 1000)) || null;
        this._url = json.url;
        this._trimmed = json.trimmed;
        this._identifier = json.identifier;
        this._humanizedTimestamp = json.humanizedTimestamp;
    }

    get id(): number {
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

    get authorId(): number {
        return this._authorId;
    }

    get published(): Date {
        return this._published;
    }

    get updated(): Date {
        return this._updated;
    }

    get url(): string {
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
}

export default BlogPost;