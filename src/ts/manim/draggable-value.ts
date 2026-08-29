import type {Scene, ThreeDScene} from 'manim-web';
import {ensureKatexLoaded} from './katex-loader.ts';

export interface DisplayNumberOptions {
    /** Initial value. */
    value: number;
    /** Decimal places to display and round to. Defaults to 2. */
    precision?: number;
    /** CSS color for the rendered value. Defaults to white. Shorthand for the common case - `prefix`/`suffix` can
     *  still wrap the number in raw LaTeX color commands (e.g. `\color{}`) for anything this can't express. */
    color?: string;
    /** Raw LaTeX to render before the number, e.g. `"t = "` (wrap in `\text{}` for upright text, not italic). */
    prefix?: string;
    /** Raw LaTeX to render after the number, e.g. `"^\\circ"`. */
    suffix?: string;
    /** Whether to always show a leading +/- sign rather than omitting it for positive values. Defaults to false. */
    alwaysShowSign?: boolean;
}

type ResolvedDisplayOptions = Required<Omit<DisplayNumberOptions, 'color' | 'prefix' | 'suffix'>> &
    Pick<DisplayNumberOptions, 'color' | 'prefix' | 'suffix'>;

/**
 * A number rendered as LaTeX, as a bare, unpositioned element, with no interactivity of its own - just `setValue()`
 * to update it programmatically. This is the read-only half of the pair: `DraggableNumber` extends this, adding
 * drag/wheel/keyboard interaction on top; a bare `DisplayNumber` is for a value that's computed from *other* things
 * (e.g. the result of a lerp) rather than one a reader adjusts directly.
 */
export class DisplayNumber {
    public readonly element: HTMLSpanElement;
    protected readonly options: ResolvedDisplayOptions;
    protected currentValue: number;

    constructor(options: DisplayNumberOptions) {
        this.options = {
            precision: 2,
            alwaysShowSign: false,
            ...options,
        };
        this.currentValue = options.value;

        this.element = document.createElement('span');
        this.element.className = 'manim-display-value';
        this.element.style.color = options.color ?? '#fff';

        this.render();
    }

    /**
     * The current value.
     */
    public get value(): number {
        return this.currentValue;
    }

    /**
     * Removes this value's element.
     */
    public destroy(): void {
        this.element.remove();
    }

    /**
     * Sets the value and re-renders. There's no `onChange` at this level to call - this class has no interaction of its own.
     */
    public setValue(value: number): void {
        if (value === this.currentValue) {
            return;
        }

        this.currentValue = value;
        this.render();
    }

    protected render(): void {
        const {precision, alwaysShowSign, prefix, suffix} = this.options;
        const magnitude = Math.abs(this.value).toFixed(precision);
        const sign = this.value < 0 ? '-' : (alwaysShowSign ? '+' : '');
        const latex = `${prefix ?? ''}${sign}${magnitude}${suffix ?? ''}`;

        // shows as plain text immediately, then upgrades to real LaTeX typesetting the moment KaTeX is loaded (once
        // per page, regardless of how many values exist)
        this.element.textContent = latex;
        ensureKatexLoaded()
            .then(katex => katex.render(latex, this.element, {throwOnError: false}))
            .catch(error => console.error('Failed to load KaTeX for a displayed value:', error));
    }
}

export interface DraggableNumberOptions extends DisplayNumberOptions {
    /** Minimum value the drag/wheel/keyboard interactions can reach. */
    min: number;
    /** Maximum value the drag/wheel/keyboard interactions can reach. */
    max: number;
    /** Pixel distance a drag must travel to move through the entire min-max range. Defaults to 200. */
    sensitivity?: number;
    /** Values the dragged number snaps to when within `snapDistance` of them. */
    snapPoints?: number[];
    /** How close (in value units) the current value must be to a snap point to snap to it. Defaults to 0 (off). */
    snapDistance?: number;
    /** Called with the new value whenever it changes via drag, wheel, or keyboard. */
    onChange: (value: number) => void;
}

type ResolvedDragOptions = Required<Pick<DraggableNumberOptions, 'min' | 'max' | 'sensitivity' | 'snapPoints' | 'snapDistance' | 'onChange'>>;

/**
 * A `DisplayNumber` a reader can click-and-drag (or wheel/arrow-key) to adjust, as a bare, unpositioned element -
 * shared by `DraggableValue`, which positions exactly one of these on its own, absolutely, over a scene; and
 * `DraggableExpression`, which lays several of these out inline alongside static text and read-only values, as one
 * continuous expression.
 */
export class DraggableNumber extends DisplayNumber {
    private readonly dragOptions: ResolvedDragOptions;
    private dragStart: { pointerX: number; pointerY: number; value: number } | null = null;

    /**
     * Initializes a new instance of the `DraggableNumber` class.
     * @param options The configuration for this number, including its initial value, min/max, and `onChange` callback.
     */
    constructor(options: DraggableNumberOptions) {
        super(options);
        this.dragOptions = {
            sensitivity: 200,
            snapPoints: [],
            snapDistance: 0,
            ...options,
        };

        this.element.classList.add('manim-draggable-value');
        this.element.tabIndex = 0;

        this.element.addEventListener('mousedown', this.onPointerDown);
        this.element.addEventListener('touchstart', this.onPointerDown, {passive: false});
        this.element.addEventListener('wheel', this.onWheel, {passive: false});
        this.element.addEventListener('keydown', this.onKeyDown);
    }

    /** Detaches this value's drag listeners in addition to removing its element. */
    public override destroy(): void {
        this.stopDragging();
        super.destroy();
    }

    /**
     * Sets the value programmatically.
     * @param value The new value. Will be clamped to the configured min/max and snapped to any configured snap points.
     */
    public override setValue(value: number): void {
        const {min, max, snapPoints, snapDistance} = this.dragOptions;
        let next = Math.max(min, Math.min(max, value));

        for (const snapPoint of snapPoints) {
            if (Math.abs(next - snapPoint) <= snapDistance) {
                next = snapPoint;
                break;
            }
        }

        super.setValue(next);
    }

    private readonly onPointerDown = (event: MouseEvent | TouchEvent): void => {
        event.preventDefault();
        const point = 'touches' in event ? event.touches[0] : event;
        this.dragStart = {pointerX: point.pageX, pointerY: point.pageY, value: this.value};
        this.element.classList.add('dragging');

        // bound to `document`, not the element itself - a drag must keep tracking the pointer even once it leaves
        // this element's own (small) bounds, same as the reference implementation this is modeled on
        document.addEventListener('mousemove', this.onPointerMove);
        document.addEventListener('touchmove', this.onPointerMove, {passive: false});
        document.addEventListener('mouseup', this.onPointerUp);
        document.addEventListener('touchend', this.onPointerUp);
        document.addEventListener('touchcancel', this.onPointerUp);
    };

    private readonly onPointerMove = (event: MouseEvent | TouchEvent): void => {
        if (!this.dragStart) {
            return;
        }

        event.preventDefault();
        const point = 'touches' in event ? event.touches[0] : event;
        const {min, max, sensitivity} = this.dragOptions;

        // diagonal scrub: moving right OR up increases the value, left OR down decreases it - lets one drag gesture
        // read from both axes at once, rather than pinning the whole interaction to a single direction
        const delta = (point.pageX - this.dragStart.pointerX) + (this.dragStart.pointerY - point.pageY);
        this.setValueFromInteraction(this.dragStart.value + (delta / sensitivity) * (max - min));
    };

    private readonly onPointerUp = (): void => {
        this.stopDragging();
    };

    private stopDragging(): void {
        this.dragStart = null;
        this.element.classList.remove('dragging');

        document.removeEventListener('mousemove', this.onPointerMove);
        document.removeEventListener('touchmove', this.onPointerMove);
        document.removeEventListener('mouseup', this.onPointerUp);
        document.removeEventListener('touchend', this.onPointerUp);
        document.removeEventListener('touchcancel', this.onPointerUp);
    }

    private readonly onWheel = (event: WheelEvent): void => {
        event.preventDefault();
        const step = (this.dragOptions.max - this.dragOptions.min) / 100;
        this.setValueFromInteraction(this.value + (event.deltaY < 0 ? step : -step));
    };

    private readonly onKeyDown = (event: KeyboardEvent): void => {
        const step = (this.dragOptions.max - this.dragOptions.min) / 100;
        if (event.key === 'ArrowUp' || event.key === 'ArrowRight') {
            event.preventDefault();
            this.setValueFromInteraction(this.value + step);
        } else if (event.key === 'ArrowDown' || event.key === 'ArrowLeft') {
            event.preventDefault();
            this.setValueFromInteraction(this.value - step);
        }
    };

    private setValueFromInteraction(value: number): void {
        const before = this.value;
        this.setValue(value);
        if (this.value !== before) {
            this.dragOptions.onChange(this.value);
        }
    }
}

export interface DraggableValueOptions extends DraggableNumberOptions {
    /** Horizontal offset, in pixels, from the scene's container's top-left corner. */
    x: number;
    /** Vertical offset, in pixels, from the scene's container's top-left corner. */
    y: number;
}

/**
 * A click-and-drag-to-adjust number, rendered as LaTeX and overlaid directly on a manim-web scene's canvas - the
 * "diegetic" alternative to `Controls.addSlider`'s bolted-on panel, modeled on the interaction Ben Eater and Grant
 * Sanderson (3Blue1Brown) built for their quaternions series (eater.net/quaternions): the value reads as part of the
 * on-screen math rather than as a separate widget floating over the artwork - and, unlike a fixed slider panel, it
 * sits wherever the scene's own script places it, so it never has a fixed reason to end up on top of the very
 * mobject it controls.
 */
export class DraggableValue {
    private readonly number: DraggableNumber;

    /**
     * The current value - reflects drag/wheel/keyboard changes immediately, not just what the last `onChange` call reported.
     */
    public get value(): number {
        return this.number.value;
    }

    /**
     * Initializes a new instance of the `DraggableValue` class.
     * @param scene The scene to overlay this value onto - anchored to `scene.getContainer()`.
     * @param options Configuration; see {@link DraggableValueOptions}.
     */
    constructor(scene: Scene | ThreeDScene, options: DraggableValueOptions) {
        this.number = new DraggableNumber(options);
        this.number.element.style.position = 'absolute';
        this.number.element.style.left = `${options.x}px`;
        this.number.element.style.top = `${options.y}px`;

        scene.getContainer().append(this.number.element);
    }

    /**
     * Sets the value programmatically, e.g. to keep this in sync with a mobject that got moved some other way (its
     * own native drag handling, say). Updates the display but does not call `onChange` - see `DraggableNumber`.
     */
    public setValue(value: number): void {
        this.number.setValue(value);
    }

    /** Removes this value's overlay element from the scene. */
    public destroy(): void {
        this.number.destroy();
    }
}

export interface DisplayValueOptions extends DisplayNumberOptions {
    /** Horizontal offset, in pixels, from the scene's container's top-left corner. */
    x: number;
    /** Vertical offset, in pixels, from the scene's container's top-left corner. */
    y: number;
}

/**
 * A read-only number, rendered as LaTeX and overlaid directly on a manim-web scene's canvas - the standalone
 * counterpart to `DraggableValue`, for a value that's computed from other things.
 */
export class DisplayValue {
    private readonly number: DisplayNumber;

    /**
     * The current value.
     */
    public get value(): number {
        return this.number.value;
    }

    /**
     * Initializes a new instance of the `DisplayValue` class.
     * @param scene The scene to overlay this value onto - anchored to `scene.getContainer()`.
     * @param options Configuration; see {@link DisplayValueOptions}.
     */
    constructor(scene: Scene | ThreeDScene, options: DisplayValueOptions) {
        this.number = new DisplayNumber(options);
        this.number.element.style.position = 'absolute';
        this.number.element.style.left = `${options.x}px`;
        this.number.element.style.top = `${options.y}px`;

        scene.getContainer().append(this.number.element);
    }

    /**
     * Sets the value and re-renders the display.
     * @param value The new value.
     */
    public setValue(value: number): void {
        this.number.setValue(value);
    }

    /**
     * Removes this value's overlay element from the scene.
     */
    public destroy(): void {
        this.number.destroy();
    }
}
