import type {Scene, ThreeDScene} from 'manim-web';
import {DisplayNumber, type DisplayNumberOptions, DraggableNumber, type DraggableNumberOptions} from './draggable-value.ts';
import {ensureKatexLoaded} from './katex-loader.ts';

/** A fixed, non-interactive piece of the expression - just LaTeX, e.g. `"\\text{lerp}("` or `", "`. */
export interface TextSegment {
    type: 'text';
    /** Raw LaTeX to render. */
    latex: string;
    /** CSS color. Defaults to white. Raw LaTeX color commands (`\color{}`) work too, self-contained to this segment. */
    color?: string;
}

/** An independently click-and-drag-to-adjust number within the expression - same options as `DraggableValue`, minus position. */
export interface ValueSegment extends DraggableNumberOptions {
    type: 'value';
}

/**
 * A number within the expression that a reader can't adjust directly - e.g. the result of a computation over the expression's
 * other segments.
 */
export interface ReadonlySegment extends DisplayNumberOptions {
    type: 'readonly';
}

export type ExpressionSegment = TextSegment | ValueSegment | ReadonlySegment;

export interface DraggableExpressionOptions {
    /** Horizontal offset, in pixels, from the scene's container's top-left corner. */
    x: number;
    /** Vertical offset, in pixels, from the scene's container's top-left corner. */
    y: number;
    /** The pieces making up the expression, in order - static text and independently-draggable numbers, mixed freely. */
    segments: ExpressionSegment[];
}

/**
 * Represents several `DraggableValue`-style numbers laid out inline alongside static LaTeX text, as one continuous expression -
 * e.g. `lerp(5, 15, 0.5)` with each argument independently draggable. Anchored at one `{x, y}` point; the browser's own flex
 * layout handles spacing between segments.
 * @remarks Each segment renders as its own independent KaTeX call into its own element - LaTeX commands (like `\color{}`) in one
 * segment's `latex`/`prefix`/`suffix` never reach into a different segment.
 */
export class DraggableExpression {
    private readonly wrapper: HTMLSpanElement;
    private readonly numbers: DisplayNumber[] = [];

    /**
     * @param scene The scene to overlay this expression onto - anchored to `scene.getContainer()`.
     * @param options Configuration; see {@link DraggableExpressionOptions}.
     */
    constructor(scene: Scene | ThreeDScene, options: DraggableExpressionOptions) {
        this.wrapper = document.createElement('span');
        this.wrapper.className = 'manim-draggable-expression';
        this.wrapper.style.left = `${options.x}px`;
        this.wrapper.style.top = `${options.y}px`;

        for (const segment of options.segments) {
            if (segment.type === 'value') {
                const {type: _type, ...numberOptions} = segment;
                const number = new DraggableNumber(numberOptions);
                this.numbers.push(number);
                this.wrapper.append(number.element);
            } else if (segment.type === 'readonly') {
                const {type: _type, ...numberOptions} = segment;
                const number = new DisplayNumber(numberOptions);
                this.numbers.push(number);
                this.wrapper.append(number.element);
            } else {
                this.wrapper.append(this.renderTextSegment(segment));
            }
        }

        scene.getContainer().append(this.wrapper);
    }

    /**
     * The current values of each `type: 'value'`/`type: 'readonly'` segment, in the order they appear in `segments`.
     */
    public get values(): number[] {
        return this.numbers.map(number => number.value);
    }

    /**
     * Sets the value of the `index`-th `type: 'value'`/`type: 'readonly'` segment (0 = the first one, counting only those, not
     * text) programmatically.
     */
    public setValue(index: number, value: number): void {
        this.numbers[index]?.setValue(value);
    }

    /**
     * Removes this expression's overlay element from the scene.
     */
    public destroy(): void {
        for (const number of this.numbers) {
            number.destroy();
        }
        this.wrapper.remove();
    }

    private renderTextSegment(segment: TextSegment): HTMLSpanElement {
        const span = document.createElement('span');
        span.style.color = segment.color ?? '#fff';
        span.textContent = segment.latex;

        ensureKatexLoaded()
            .then(katex => katex.render(segment.latex, span, {throwOnError: false}))
            .catch(error => console.error('Failed to load KaTeX for a draggable expression:', error));

        return span;
    }
}
