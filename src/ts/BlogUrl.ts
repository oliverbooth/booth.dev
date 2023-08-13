class BlogUrl {
    private readonly _year: string;
    private readonly _month: string;
    private readonly _day: string;
    private readonly _slug: string;

    constructor(json: any) {
        this._year = json.year;
        this._month = json.month;
        this._day = json.day;
        this._slug = json.slug;
    }


    get year(): string {
        return this._year;
    }

    get month(): string {
        return this._month;
    }

    get day(): string {
        return this._day;
    }

    get slug(): string {
        return this._slug;
    }
}

export default BlogUrl;