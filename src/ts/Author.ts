class Author {
    private readonly _id: string;
    private readonly _name: string;
    private readonly _avatarHash: string;

    constructor(json: any) {
        this._id = json.id;
        this._name = json.name;
        this._avatarHash = json.avatarHash;
    }

    get id(): string {
        return this._id;
    }

    get name(): string {
        return this._name;
    }

    get avatarHash(): string {
        return this._avatarHash;
    }
}

export default Author;