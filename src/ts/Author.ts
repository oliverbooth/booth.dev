class Author {
    private readonly _id: string;
    private readonly _name: string;
    private readonly _avatarUrl: string;

    constructor(json: any) {
        this._id = json.id;
        this._name = json.name;
        this._avatarUrl = json.avatarUrl;
    }

    get id(): string {
        return this._id;
    }

    get name(): string {
        return this._name;
    }

    get avatarUrl(): string {
        return this._avatarUrl;
    }
}

export default Author;