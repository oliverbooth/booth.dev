/**
 * A 1-indexed position, either from the start or end of a line. This mirrors the C# implementation's SpecBound struct, but is a simple
 * interface here since TS has no System.Index/Range to lean on.
 */
export interface SpecBound {
    /**
     * The 1-indexed position from the start or end of a line.
     */
    readonly value: number;

    /**
     * Whether the position is counted from the end of a line (true) or from the start (false).
     */
    readonly isFromEnd: boolean;
}

/**
 * Creates a SpecBound that counts from the start of a line.
 * @param value The 1-indexed position from the start of a line.
 * @returns A SpecBound representing the position.
 */
export function forward(value: number): SpecBound {
    return {value, isFromEnd: false};
}

/**
 * Creates a SpecBound that counts from the end of a line.
 * @param value The 1-indexed position from the end of a line.
 * @returns A SpecBound representing the position.
 */
export function backward(value: number): SpecBound {
    return {value, isFromEnd: true};
}

/**
 * Resolves a SpecBound to a concrete 0-indexed position, given the total length being indexed into
 * (a line count, or a single line's character count).
 * @param bound The SpecBound to resolve.
 * @param role Whether the bound is a start or end position. This affects how the bound is resolved to 0-indexed coordinates.
 * @param length The total length being indexed into (a line count, or a single line's character count).
 * @returns The resolved 0-indexed position.
 */
export function resolveBound(bound: SpecBound, role: 'start' | 'end', length: number): number {
    if (role === 'start') {
        return bound.isFromEnd
            ? length - bound.value // back-start: unchanged relative to length
            : bound.value - 1;     // forward-start: -1 (1-based -> 0-based)
    }

    // role === 'end' (exclusive, for use as an array/slice upper bound)
    return bound.isFromEnd
        ? length - bound.value + 1 // back-end: -1 relative to back-start (inclusive -> exclusive)
        : bound.value;             // forward-end: unchanged (1-based inclusive == 0-based exclusive)
}

/**
 * An inclusive range of 1-indexed bounds.
 */
export interface SpecRange {
    readonly start: SpecBound;
    readonly end: SpecBound;
}

/**
 * Resolves a SpecRange to a concrete [start, end) pair of 0-indexed positions, clamped to [0, length].
 * @param range The SpecRange to resolve.
 * @param length The total length being indexed into (a line count, or a single line's character count).
 * @returns An object containing the resolved start and end positions.
 */
export function resolveRange(range: SpecRange, length: number): { start: number; end: number } {
    const start = clamp(resolveBound(range.start, 'start', length), 0, length);
    const end = clamp(resolveBound(range.end, 'end', length), 0, length);
    return {start, end: Math.max(start, end)};
}

function clamp(value: number, min: number, max: number): number {
    return Math.min(Math.max(value, min), max);
}
