class Author {
    private _name: string;
    private _avatarHash: string;

    constructor(json: any) {
        this._name = json.name;
        this._avatarHash = json.avatarHash;
    }

    get name(): string {
        return this._name;
    }

    get avatarHash(): string {
        return this._avatarHash;
    }
}
export default Author;