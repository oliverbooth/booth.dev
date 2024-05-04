class Callout {
    private readonly _callout: HTMLElement;
    private readonly _title: HTMLElement;
    private readonly _content: HTMLElement;
    private _foldEnabled: boolean;

    constructor(element: HTMLElement) {
        this._callout = element;
        this._title = element.querySelector(".callout-title");
        this._content = element.querySelector(".callout-content");
    }

    public static foldAll(element?: HTMLElement): void {
        element = element || document.body;
        this.findAll(element).forEach(c => c.fold());
    }

    public static findAll(element?: HTMLElement): Array<Callout> {
        element = element || document.body;
        return Array.from(element.querySelectorAll("div.callout")).map(c => {
            return new Callout(c as HTMLElement);
        });
    }

    public get content(): HTMLElement {
        return this._content;
    }

    public get element(): HTMLElement {
        return this._callout;
    }

    public get isFoldable(): boolean {
        const fold: string = this._callout.dataset.calloutFold;
        return fold !== null && fold !== undefined;
    }

    public get title(): HTMLElement {
        return this._title;
    }

    public fold(): void {
        if (this._foldEnabled || !this.isFoldable) {
            return;
        }

        const callout: HTMLElement = this._callout;

        if (callout === null) {
            console.error("Callout element for ", this, " is null!");
            return;
        }

        callout.classList.add("collapsible", "collapsed");
        this._title.addEventListener("click", () => {
            callout.classList.toggle("collapsed");
        });

        this._foldEnabled = true;
    }
}

export default Callout;